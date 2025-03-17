using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ChessUI;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }

    public static string BoardUrl;
    public static BoardTheme SelectedBoardTheme;

    public static string PieceUrl;
    public static PieceTheme SelectedPieceTheme;

    private readonly Image[,] pieceImages = new Image[8, 8];
    private readonly Rectangle[,] highlights = new Rectangle[8, 8];
    private readonly Dictionary<Position, Move> moveCache = [];

    public GameState gameState;
    private Position selectedPos = null;

    private bool Loading;

    private ObservableCollection<ImageSource> whiteCapturedPieces = [];
    private ObservableCollection<ImageSource> blackCapturedPieces = [];

    private DispatcherTimer whiteTimer;
    private DispatcherTimer blackTimer;

    private int whiteTimeRemaining = 600;
    private int blackTimeRemaining = 600;

    private string whitePlayerName = "Jogador Branco";
    private string blackPlayerName = "Jogador Preto";

    private bool gameInProgress = false;

    public MainWindow()
    {
        InitializeComponent();

        Loading = true;
        Instance = this;
        
        gameState = new GameState(Player.White, Board.Initial());
        PieceThemeHelper.LoadPieceImages();

        InitializeUI();

        BoardThemeHelper.ChangeBoardTheme(BoardTheme.Default);
        PieceThemeHelper.ChangePieceTheme(PieceTheme.Anarcandy);
        ColorThemeHelper.ApplyTheme(ColorTheme.Blue);

        InitializeBoard();

        DrawBoard(gameState.Board);
        SetCursor(gameState.CurrentPlayer);
        Loading = false;
    }

    private void InitializeBoard()
    {

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                Image image = new();
                pieceImages[r, c] = image;
                PieceGrid.Children.Add(image);

                Rectangle highlight = new();
                highlights[r, c] = highlight;
                HighlightGrid.Children.Add(highlight);
            }
        }
    }

    public void DrawBoard(Board board)
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                Piece piece = board[r, c];
                pieceImages[r, c].Source = PieceThemeHelper.GetImage(piece);              
            }
        }
    }

    private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsMenuOnScreen())
        {
            return;
        }

        Point point = e.GetPosition(BoardGrid);
        Position pos = ToSquarePosition(point);

        if(selectedPos == null)
        {
            OnFromPositionSelected(pos);
        }
        else
        {
            OnToPositionSelected(pos);
        }
    }

    private void OnFromPositionSelected(Position pos)
    {
        IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);

        if (moves.Any())
        {
            selectedPos = pos;
            CacheMoves(moves);
            ShowHighlights();
        }
    }

    private void OnToPositionSelected(Position pos)
    {
        selectedPos = null;
        HideHighlights();

        if (moveCache.TryGetValue(pos, out Move move))
        {
            if (move.Type == MoveType.PawnPromotion)
            {
                HandlePromotion(move.FromPos, move.ToPos);
            }
            else
            {
                HandleMove(move);
            }
        }
    }

    private void HandlePromotion(Position from, Position to)
    {
        pieceImages[to.Row, to.Column].Source = PieceThemeHelper.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
        pieceImages[from.Row, from.Column].Source = null;

        PromotionMenu promMenu = new(gameState.CurrentPlayer);
        MenuContainer.Content = promMenu;

        promMenu.PieceSelected += type =>
        {
            MenuContainer.Content = null;
            Move promMove = new PawnPromotion(from, to, type);
            HandleMove(promMove);
        };
    }

    private void HandleMove(Move move)
    {
        gameState.MakeMove(move);
        DrawBoard(gameState.Board);
        SetCursor(gameState.CurrentPlayer);

        if (gameState.IsGameOver())
        {
            ShowGameOver();
        }
    }

    private Position ToSquarePosition(Point point)
    {
        double squareSize = BoardGrid.ActualWidth / 8;
        int row = (int)(point.Y / squareSize);
        int col = (int)(point.X / squareSize);
        return new Position(row, col);
    }

    private void CacheMoves(IEnumerable<Move> moves)
    {
        moveCache.Clear();

        foreach (Move move in moves)
        {
            moveCache[move.ToPos] = move;
        }
    }

    private void ShowHighlights()
    {
        Color color = Color.FromArgb(150, 125, 255, 125);

        foreach (Position to in moveCache.Keys)
        {
            highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
        }
    }

    private void HideHighlights()
    {
        foreach (Position to in moveCache.Keys)
        {
            highlights[to.Row, to.Column].Fill = Brushes.Transparent;
        }
    }

    private void SetCursor(Player player)
    {
        if (player == Player.White)
        {
            Cursor = ChessCursors.WhiteCursor;
        }
        else
        {
            Cursor = ChessCursors.BlackCursor;
        }
    }

    private bool IsMenuOnScreen()
    {
        return MenuContainer.Content != null;
    }

    private void ShowGameOver()
    {
        GameOverMenu gameOverMenu = new GameOverMenu(gameState);
        MenuContainer.Content = gameOverMenu;

        gameOverMenu.OptionSelected += option =>
        {
            if (option == Option.Restart)
            {
                MenuContainer.Content = null;
                RestartGame();
            }
            else
            {
                Application.Current.Shutdown();
            }
        };
    }

    private void RestartGame()
    {
        selectedPos = null;
        HideHighlights();
        moveCache.Clear();
        gameState = new GameState(Player.White, Board.Initial());
        DrawBoard(gameState.Board);
        SetCursor(gameState.CurrentPlayer);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (!IsMenuOnScreen() && e.Key == Key.Escape)
        {
            ShowPauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        PauseMenu pauseMenu = new();
        MenuContainer.Content = pauseMenu;

        pauseMenu.OptionSelected += option =>
        {
            MenuContainer.Content = null;

            if (option == Option.Restart)
            {
                RestartGame();
            }
        };
    }

    private void InitializeUI()
    {
        BoardThemeComboBox.Items.Clear();
        PieceThemeComboBox.Items.Clear();
        ColorThemeComboBox.Items.Clear();

        foreach (BoardTheme theme in Enum.GetValues(typeof(BoardTheme)))
        {
            BoardThemeComboBox.Items.Add(new
            {
                Theme = theme,
                Name = theme.ToString(),
                ThumbnailPath = GetBoardThumbnailPath(theme)
            });
        }


        foreach (PieceTheme theme in Enum.GetValues(typeof(PieceTheme)))
        {
            PieceThemeComboBox.Items.Add(new
            {
                Theme = theme,
                Name = theme.ToString(),
                ThumbnailPath = GetPieceThumbnailPath(theme)
            });
        }

        var temas = ColorThemeHelper.GetAllThemes();
        foreach (ColorThemeInfo themeInfo in temas)
        {
            ColorThemeComboBox.Items.Add(new
            {
                Theme = themeInfo.Theme,
                Name = themeInfo.Name.ToString(),
                ColorPreview = themeInfo.PreviewBrush
            });
        }

        BoardThemeComboBox.SelectedIndex = 0;
        PieceThemeComboBox.SelectedIndex = 0;
        ColorThemeComboBox.SelectedIndex = 0;
    }

    private ImageSource GetBoardThumbnailPath(BoardTheme theme)
    {
        string basePath = $"Assets/Boards/{theme.ToString().ToLower()}";
        if (File.Exists($"{basePath}.thumbnail.png"))
        {
            return Images.LoadImage($"{basePath}.thumbnail.png");
        }
        else if (File.Exists($"{basePath}.thumbnail.jpg"))
        {
            return Images.LoadImage($"{basePath}.thumbnail.jpg");
        }
        
        return Images.LoadImage(BoardThemeHelper.GetBoardTexturePath(theme));
    }

    private ImageSource GetPieceThumbnailPath(PieceTheme theme)
    {
        string basePath = $"Assets/Pieces/{theme.ToString().ToLower()}/wN.svg";
        return Images.LoadSvg(basePath);
    }

    private void BoardThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Loading)
        {
            if (BoardThemeComboBox.SelectedItem != null)
            {
                dynamic item = BoardThemeComboBox.SelectedItem;
                BoardTheme selectedTheme = item.Theme;
                BoardThemeHelper.ChangeBoardTheme(selectedTheme);
            }
        }
    }

    private void PieceThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Loading)
        {
            if (PieceThemeComboBox.SelectedItem != null)
            {
                dynamic item = PieceThemeComboBox.SelectedItem;
                PieceTheme selectedTheme = item.Theme;
                PieceThemeHelper.ChangePieceTheme(selectedTheme);
                PieceThemeHelper.LoadPieceImages();
                if (gameState != null)
                {
                    DrawBoard(gameState.Board);
                }
            }
        }
    }

    private void ColorThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Loading)
        {
            if (ColorThemeComboBox.SelectedItem != null)
            {
                dynamic item = ColorThemeComboBox.SelectedItem;
                ColorTheme selectedTheme = item.Theme;
                ColorThemeHelper.ApplyTheme(selectedTheme);
            }
        }
    }

    private void StartGameButton_Click(object sender, RoutedEventArgs e)
    {
        RestartGame();
        gameState.StartGame();
        gameInProgress = true;

        whiteCapturedPieces.Clear();
        blackCapturedPieces.Clear();
    }

    // O DAVID É BESUNTADO COM leite manteiga de piça
}