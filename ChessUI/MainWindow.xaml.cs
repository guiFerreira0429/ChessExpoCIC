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

    private readonly bool Loading;

    private ObservableCollection<ImageSource> whiteCapturedPieces = [];
    private ObservableCollection<ImageSource> blackCapturedPieces = [];

    //private DispatcherTimer whiteTimer;
    //private DispatcherTimer blackTimer;

    //private int whiteTimeRemaining = 600;
    //private int blackTimeRemaining = 600;

    //private string whitePlayerName = "Jogador Branco";
    //private string blackPlayerName = "Jogador Preto";

    //private bool gameInProgress = false;

    public MainWindow()
    {
        InitializeComponent();

        InitializeThemeHandling();

        Loading = true;
        Instance = this;
        
        gameState = new GameState(Player.White, Board.Initial());
        PieceThemeHelper.LoadPieceImages();

        InitializeUI();

        ColorThemeHelper.ApplyTheme(ColorTheme.Blue);
        BoardThemeHelper.ChangeBoardTheme(BoardTheme.Default);
        PieceThemeHelper.ChangePieceTheme(PieceTheme.Anarcandy);

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

        BoardThemeComboBox.SelectedIndex = (int)BoardTheme.Default;
        PieceThemeComboBox.SelectedIndex = (int)PieceTheme.Default;
        ColorThemeComboBox.SelectedIndex = (int)ColorTheme.Default;
    }

    private ImageSource GetBoardThumbnailPath(BoardTheme theme)
    {
        string basePath = $"Assets/Boards/{theme.ToString().ToLower()}";
        if (File.Exists($"{basePath}_thumbnail.png"))
        {
            return Images.LoadImage($"{basePath}_thumbnail.png");
        }
        else if (File.Exists($"{basePath}_thumbnail.jpg"))
        {
            return Images.LoadImage($"{basePath}_thumbnail.jpg");
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

                try
                {
                    // Aplica o tema (isso acionará o evento ThemeChanged)
                    ColorThemeHelper.ApplyTheme(selectedTheme);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao alterar o tema: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void StartGameButton_Click(object sender, RoutedEventArgs e)
    {
        //Check if the game is already in progress
        //if (!IsGameInProgress)
        //{
        //    Retrieve the selected game type from a ComboBox or other control
        //    string gameType = GameTypeComboBox.SelectedItem?.ToString() ?? "Standard";

        //    Start the game with the selected type
        //    StartGame(gameType);

        //    Update button text and appearance
        //    StartGameButton.Content = "Pause Game";
        //    StartGameButton.Background = new SolidColorBrush(Colors.Orange);

        //    Set flag indicating game is in progress
        //   IsGameInProgress = true;
        //}
        //else
        //{
        //    Game is already in progress, so pause it
        //    PauseGame();

        //    Update button text and appearance
        //    StartGameButton.Content = "Resume Game";
        //    StartGameButton.Background = new SolidColorBrush(Colors.Green);

        //    Set flag indicating game is paused
        //   IsGameInProgress = false;
        //}
    }

    // Helper method to initialize and start the game
    private void StartGame(string gameType)
    {
        // Initialize the game board based on the selected game type
        //switch (gameType)
        //{
        //    case "Standard":
        //        // Initialize standard chess game
        //        InitializeStandardChessGame();
        //        break;
        //    case "Fischer Random":
        //        // Initialize Fischer Random (Chess960) game
        //        InitializeFischerRandomChessGame();
        //        break;
        //    case "Custom":
        //        // Initialize custom game position
        //        InitializeCustomChessGame();
        //        break;
        //    default:
        //        // Default to standard chess
        //        InitializeStandardChessGame();
        //        break;
        //}

        //// Start the game clock
        //StartGameClock();

        //// Enable the chess board for player interaction
        //ChessBoard.IsEnabled = true;
    }

    // Helper method to pause the game
    private void PauseGame()
    {
        // Pause the game clock
        //PauseGameClock();

        //// Disable the chess board during pause
        //ChessBoard.IsEnabled = false;

        //// Optional: Show pause overlay or message
        //ShowPauseOverlay();
    }

    private void ColorThemeHelper_ThemeChanged(object sender, ThemeChangedEventArgs e)
    {
        // Garante que o código seja executado na thread da UI
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Força a atualização de toda a interface
            RefreshEntireUI();
        });
    }

    private void RefreshEntireUI()
    {
        // Isso força todos os elementos visuais a serem renderizados novamente
        // Uma abordagem mais agressiva que pode ajudar com problemas de atualização de tema

        // Recria os recursos de estilo
        Style oldStyle = null;
        if (Resources.Contains("ThemeComboBoxStyle"))
        {
            oldStyle = Resources["ThemeComboBoxStyle"] as Style;
            Resources.Remove("ThemeComboBoxStyle");
            Resources.Add("ThemeComboBoxStyle", oldStyle);
        }

        // Força a atualização de cada controle individual
        InvalidateVisual();

        // Itera por todos os controles na janela e força a atualização
        ForceUpdateAllControls((Panel)Content);
    }

    private void ForceUpdateAllControls(Panel panel)
    {
        if (panel == null) return;

        foreach (UIElement element in panel.Children)
        {
            // Força redesenho do elemento
            element.InvalidateVisual();

            // Se for um painel, processa seus filhos recursivamente
            if (element is Panel childPanel)
            {
                ForceUpdateAllControls(childPanel);
            }
            else if (element is ContentControl contentControl && contentControl.Content is Panel contentPanel)
            {
                ForceUpdateAllControls(contentPanel);
            }
        }
    }

    private void InitializeThemeHandling()
    {
        ColorThemeHelper.ThemeChanged += ColorThemeHelper_ThemeChanged;
    }
}