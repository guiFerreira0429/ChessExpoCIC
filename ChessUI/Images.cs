using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessLogic;

namespace ChessUI;

public static class Images
{
    private static readonly Dictionary<PieceType, ImageSource> whiteSources = new()
    {
        { PieceType.Pawn, LoadImage($"{MainWindow.AssetsUrl}PawnW.png") },
        { PieceType.Bishop, LoadImage($"{MainWindow.AssetsUrl}BishopW.png") },
        { PieceType.Knight, LoadImage($"{MainWindow.AssetsUrl}KnightW.png") },
        { PieceType.Rook, LoadImage($"{MainWindow.AssetsUrl}RookW.png") },
        { PieceType.King, LoadImage($"{MainWindow.AssetsUrl}KingW.png") },
        { PieceType.Queen, LoadImage($"{MainWindow.AssetsUrl}QueenW.png") },
    };

    private static readonly Dictionary<PieceType, ImageSource> blackSources = new()
    {
        { PieceType.Pawn, LoadImage($"{MainWindow.AssetsUrl}PawnB.png") },
        { PieceType.Bishop, LoadImage($"{MainWindow.AssetsUrl}BishopB.png") },
        { PieceType.Knight, LoadImage($"{MainWindow.AssetsUrl}KnightB.png") },
        { PieceType.Rook, LoadImage($"{MainWindow.AssetsUrl}RookB.png") },
        { PieceType.King, LoadImage($"{MainWindow.AssetsUrl}KingB.png") },
        { PieceType.Queen, LoadImage($"{MainWindow.AssetsUrl}QueenB.png") },
    };

    public static ImageSource LoadImage(string filePath)
    {
        return new BitmapImage(new Uri(filePath, UriKind.Relative));
    }

    public static ImageSource GetImage(Player color, PieceType type)
    {
        return color switch
        {
            Player.White => whiteSources[type],
            Player.Black => blackSources[type],
            _ => null
        };
    }

    public static ImageSource GetImage(Piece piece)
    {
        if (piece == null)
        {
            return null;
        }

        return GetImage(piece.Color, piece.Type);
    }


}
