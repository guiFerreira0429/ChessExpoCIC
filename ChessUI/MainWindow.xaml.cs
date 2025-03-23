using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using System.Windows.Threading;

namespace ChessUI;

public partial class MainWindow : Window
{
    public static MainWindow Instance
    {
        get;
        private set;
    }

    private ChessTimer _chessTimer;
    private CapturedPiecesTracker _capturedPiecesTracker;

    public static string BoardUrl;
    public static string PieceUrl;
    public static BoardTheme SelectedBoardTheme;
    public static PieceTheme SelectedPieceTheme;
    public static ColorTheme SelectedColorTheme;
    public static GameType SelectedGameType;
    public static GameDuration SelectedGameDuration;
    public static GameIncrement SelectedGameIncrement;

    private readonly Image[,] pieceImages = new Image[8, 8];
    private readonly Rectangle[,] highlights = new Rectangle[8, 8];
    private readonly Dictionary<Position, Move> moveCache = [];
    public GameState gameState;
    private Position selectedPos = null;
    private readonly bool Loading;

    private bool _gameInProgress = false;
    public bool gameInProgress
    {
        get
        {
            return _gameInProgress;
        }
        set
        {
            _gameInProgress = value;
            GameTypeComboBox.IsEnabled = !value;
            GameDurationComboBox.IsEnabled = !value;
            GameIncrementComboBox.IsEnabled = !value;
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        _chessTimer = new ChessTimer(GameDuration.Default, GameIncrement.Default);
        _chessTimer.OnTimeUpdated += ChessTimer_Tick;
        _chessTimer.OnGameOver += ShowGameOver;

        _capturedPiecesTracker = new CapturedPiecesTracker();
        _capturedPiecesTracker.OnTrackerUpdated += CapturedPieces_Change;

        Loading = true;
        Instance = this;

        gameState = new GameState(Player.White, Board.Initial(GameType.Default), _capturedPiecesTracker);

        PieceThemeHelper.LoadPieceImages();

        InitializeUI();

        ApplyTheme(ColorTheme.Default);
        BoardThemeHelper.ChangeBoardTheme(BoardTheme.Default);
        PieceThemeHelper.ChangePieceTheme(PieceTheme.Default);

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
                pieceImages[r, c].Source = PieceThemeHelper.GetImage(piece, SelectedGameType);
            }
        }
    }

    private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsMenuOnScreen() || !gameInProgress)
        {
            return;
        }

        Point point = e.GetPosition(BoardGrid);
        Position pos = ToSquarePosition(point);

        if (selectedPos == null)
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
        _chessTimer.Pause();
        pieceImages[to.Row, to.Column].Source = PieceThemeHelper.GetImage(gameState.CurrentPlayer, PieceType.Pawn, SelectedGameType);
        pieceImages[from.Row, from.Column].Source = null;

        PromotionMenu promMenu = new(gameState.CurrentPlayer, SelectedGameType);
        MenuContainer.Content = promMenu;

        promMenu.PieceSelected += type => {
            MenuContainer.Content = null;
            Move promMove = new PawnPromotion(from, to, type);
            _chessTimer.Start();
            HandleMove(promMove);
        };
    }
    private void HandleMove(Move move)
    {
        gameState.MakeMove(move);
        DrawBoard(gameState.Board);
        SetCursor(gameState.CurrentPlayer);
        _chessTimer.ChangePlayer();
        Dispatcher.Invoke(() => 
        {
            WhiteClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];
            BlackClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];

            if(gameState.CurrentPlayer == Player.White)
            {
                WhiteClockBorder.Background = (SolidColorBrush)Application.Current.Resources["TextColor"];
            }
            else
            {
                BlackClockBorder.Background = (SolidColorBrush)Application.Current.Resources["TextColor"];
            }
        });

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
    private void ShowGameOver(EndReason endReason = EndReason.None)
    {
        if (EndReason.TimeOut == endReason)
        {
            gameState.Result = Result.Win(gameState.CurrentPlayer.Opponent(), EndReason.TimeOut);
        }
        Dispatcher.Invoke(() =>
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
        });
    }
    private void RestartGame()
    {
        ResetUI();
        selectedPos = null;
        HideHighlights();
        moveCache.Clear();
        _chessTimer.Reset();
        _capturedPiecesTracker.Reset();
        gameState = new GameState(Player.White, Board.Initial(SelectedGameType), _capturedPiecesTracker);
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

        pauseMenu.OptionSelected += option => {
            MenuContainer.Content = null;

            if (option == Option.Restart)
            {
                RestartGame();
            }
        };
    }
    private void InitializeUI()
    {
        Dispatcher.Invoke(() => 
        {
            BoardThemeComboBox.Items.Clear();
            PieceThemeComboBox.Items.Clear();
            ColorThemeComboBox.Items.Clear();
            GameTypeComboBox.Items.Clear();
            GameDurationComboBox.Items.Clear();
            GameIncrementComboBox.Items.Clear();

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
                if (theme != PieceTheme.Mono && theme != PieceTheme.Disguised)
                {
                    PieceThemeComboBox.Items.Add(new
                    {
                        Theme = theme,
                        Name = theme.ToString(),
                        ThumbnailPath = GetPieceThumbnailPath(theme)
                    });
                }
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
            foreach (GameType gameType in Enum.GetValues(typeof(GameType)))
            {
                GameTypeComboBox.Items.Add(new
                {
                    Theme = gameType,
                    Name = gameType.ToString(),
                    ThumbnailPath = GetGameTypeThumbnailPath(gameType)
                });
            }
            foreach (GameDuration gameDuration in Enum.GetValues(typeof(GameDuration)))
            {
                GameDurationComboBox.Items.Add(new
                {
                    Duration = gameDuration,
                    Value = (int)gameDuration
                });
            }
            foreach (GameIncrement gameIncrement in Enum.GetValues(typeof(GameIncrement)))
            {
                GameIncrementComboBox.Items.Add(new
                {
                    Increment = gameIncrement,
                    Value = (int)gameIncrement
                });
            }

            BoardThemeComboBox.SelectedIndex = (int)BoardTheme.Default;
            PieceThemeComboBox.SelectedIndex = (int)PieceTheme.Default;
            ColorThemeComboBox.SelectedIndex = (int)ColorTheme.Default;
            GameTypeComboBox.SelectedIndex = (int)GameType.Default;
            GameDurationComboBox.SelectedIndex = (int)GameDuration.Default;
            GameIncrementComboBox.SelectedIndex = (int)GameIncrement.Default;
        });
    }
    private void ResetUI()
    {
        HideHighlights();
        _chessTimer.Reset();
        _chessTimer.ActiveClock = Player.White;
        _capturedPiecesTracker.Reset();
        Dispatcher.Invoke(() => {
            StartGameButton.Content = "Jogar";
            StartGameButton.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];
            WhiteClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];
            BlackClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];

            WhiteCapturedPieces.Items.Clear();
            BlackCapturedPieces.Items.Clear();
            WhiteAdvantage.Visibility = Visibility.Collapsed;
            BlackAdvantage.Visibility = Visibility.Collapsed;
            WhiteClock.Text = $"{FormatTime((int)SelectedGameDuration)}";
            BlackClock.Text = $"{FormatTime((int)SelectedGameDuration)}";
        });
        gameState = new GameState(Player.White, Board.Initial(SelectedGameType), _capturedPiecesTracker);
        DrawBoard(gameState.Board);
    }
    private ImageSource GetBoardThumbnailPath(BoardTheme theme)
    {
        string basePath = $"Assets/Boards/{theme.ToString().ToLower()}";
        if (File.Exists($"{basePath}.png"))
        {
            return Images.LoadImage($"{basePath}.png");
        }
        else if (File.Exists($"{basePath}.jpg"))
        {
            return Images.LoadImage($"{basePath}.jpg");
        }

        return Images.LoadImage(BoardThemeHelper.GetBoardTexturePath(theme));
    }
    private ImageSource GetPieceThumbnailPath(PieceTheme theme)
    {
        string basePath = $"Assets/Pieces/{theme.ToString().ToLower()}/wN.svg";
        return Images.LoadSvg(basePath);
    }
    private ImageSource GetGameTypeThumbnailPath(GameType gameType)
    {
        string basePath = "Assets/Pieces/";
        return Images.LoadSvg(gameType
            switch
        {
            GameType.Default => $"{basePath}default/wN.svg",
                    GameType.Disguised => $"{basePath}disguised/w.svg",
                    GameType.Mono => $"{basePath}mono/N.svg",
                    GameType.Mixed => $"{basePath}anarcandy/wN.svg",
                    _ =>
                    throw new ArgumentException("Tipo de Jogo Inválido", nameof(gameType))
            });
    }
    private void BoardThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Loading)
        {
            if (BoardThemeComboBox.SelectedItem != null)
            {
                dynamic item = BoardThemeComboBox.SelectedItem;
                SelectedBoardTheme = item.Theme;
                BoardThemeHelper.ChangeBoardTheme(SelectedBoardTheme);
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
                SelectedPieceTheme = item.Theme;
                PieceThemeHelper.ChangePieceTheme(SelectedPieceTheme);
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
                SelectedColorTheme = item.Theme;

                try
                {
                    ApplyTheme(SelectedColorTheme);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao alterar o tema: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    private void GameTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Loading)
        {
            if (GameTypeComboBox.SelectedItem != null)
            {
                dynamic item = GameTypeComboBox.SelectedItem;
                SelectedGameType = item.Theme;
                if (SelectedGameType == GameType.Disguised || SelectedGameType == GameType.Mono)
                {
                    PieceThemeComboBox.IsEnabled = false;
                }
                else
                {
                    PieceThemeComboBox.IsEnabled = true;
                }

                ResetUI();
            }
        }
    }
    private void GameDurationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Loading)
        {
            if (GameDurationComboBox.SelectedItem != null)
            {
                dynamic item = GameDurationComboBox.SelectedItem;
                SelectedGameDuration = item.Duration;
                _chessTimer.InitialTime = item.Value;
                ResetUI();
            }
        }
    }
    private void GameIncrementComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Loading)
        {
            if (GameIncrementComboBox.SelectedItem != null)
            {
                dynamic item = GameIncrementComboBox.SelectedItem;
                SelectedGameIncrement = item.Increment;
                _chessTimer.Increment = item.Value;
                ResetUI();
            }
        }
    }

    private void StartGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (!gameInProgress)
        {
            GameType gameType = GameTypeComboBox.SelectedItem != null ?
                (GameTypeComboBox.SelectedItem as dynamic).Theme : GameType.Default;

            StartGameButton.Content = "Pausar";
            StartGameButton.Background = new SolidColorBrush(Colors.Orange);

            _chessTimer.Start();

            gameInProgress = true;
        }
        else
        {
            StartGameButton.Content = "Continuar";
            StartGameButton.Background = new SolidColorBrush(Colors.Green);

            _chessTimer.Pause();

            gameInProgress = false;
        }

        Dispatcher.Invoke(() =>
        {
            WhiteClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];
            BlackClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];

            if (gameState.CurrentPlayer == Player.White)
            {
                WhiteClockBorder.Background = (SolidColorBrush)Application.Current.Resources["TextColor"];
            }
            else
            {
                BlackClockBorder.Background = (SolidColorBrush)Application.Current.Resources["TextColor"];
            }
        });
    }
    private void ChessTimer_Tick(Player player, int seconds)
    {
        Dispatcher.Invoke(() => {
            if (!gameInProgress)
            {
                return;
            }
            else
            {
                string formatedTime = FormatTime(seconds);
                if (player == Player.White)
                {
                    WhiteClock.Text = formatedTime;
                }
                else
                {
                    BlackClock.Text = formatedTime;
                }
            }
        });
    }
    private void CapturedPieces_Change(List<Piece> capturedByWhite, List<Piece> capturedByBlack, int materialAdvantage)
    {
        Dispatcher.Invoke(() => {
            WhiteCapturedPieces.Items.Clear();
            BlackCapturedPieces.Items.Clear();

            foreach (Piece piece in capturedByWhite)
            {
                WhiteCapturedPieces.Items.Add(new Image
                {
                    Source = PieceThemeHelper.GetImage(piece, SelectedGameType),
                    Width = 30,
                    Height = 30
                });
            }
            foreach (Piece piece in capturedByBlack)
            {
                BlackCapturedPieces.Items.Add(new Image
                {
                    Source = PieceThemeHelper.GetImage(piece, SelectedGameType),
                    Width = 30,
                    Height = 30
                });
            }

            BlackAdvantage.Visibility = Visibility.Collapsed;
            WhiteAdvantage.Visibility = Visibility.Collapsed;

            if (materialAdvantage > 0)
            {

                WhiteAdvantage.Visibility = Visibility.Visible;
                WhiteAdvantage.Text = $"+{materialAdvantage}";
            }
            else if (materialAdvantage < 0)
            {

                BlackAdvantage.Visibility = Visibility.Visible;
                BlackAdvantage.Text = $"+{-1 * materialAdvantage}";
            }
        });
    }
    private string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        return $"{minutes}:{remainingSeconds:00}";
    }
    private void ApplyTheme(ColorTheme theme)
    {
        ColorThemeInfo themeInfo = ColorThemeHelper.GetThemeInfo(theme);
        Dispatcher.Invoke(() => {
            Application.Current.Resources["StrokeColor"] = new SolidColorBrush(themeInfo.StrokeColor);
            Application.Current.Resources["FillColor"] = new SolidColorBrush(themeInfo.FillColor);
            Application.Current.Resources["TextColor"] = new SolidColorBrush(themeInfo.TextColor);
            Application.Current.Resources["ButtonColor"] = new SolidColorBrush(themeInfo.ButtonColor);
            StartGameButton.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];
            WhiteClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];
            BlackClockBorder.Background = (SolidColorBrush)Application.Current.Resources["ButtonColor"];
        });
    }
}