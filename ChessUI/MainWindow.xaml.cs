using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChessLogic;

namespace ChessUI;

public partial class MainWindow : Window
{
    public static string AssetsUrl;
    public static string SelectedTheme = "Wood";
    private readonly Image[,] pieceImages = new Image[8, 8];
    private GameState gameState;
    //private GameType gameType;

    public MainWindow()
    {
        InitializeComponent();
        InitializeTheme();
        InitializeBoard();

        gameState = new GameState(Player.White, Board.Initial());
        DrawBoard(gameState.Board);
    }

    private static void InitializeTheme()
    {
        AssetsUrl = $"AssetsUrl/{SelectedTheme}/";
    }

    private void InitializeBoard()
    {
        BoardGridBrush.ImageSource = Images.LoadImage($"{AssetsUrl}Board.png");
        
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                Image image = new();
                pieceImages[r, c] = image;
                PieceGrid.Children.Add(image);
            }
        }
    }

    private void DrawBoard(Board board)
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                Piece piece = board[r, c];
                pieceImages[r, c].Source = Images.GetImage(piece);
            }
        }
    }
}