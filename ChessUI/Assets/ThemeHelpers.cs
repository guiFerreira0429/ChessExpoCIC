using ChessLogic;
using System.IO;
using System.Windows.Media;

namespace ChessUI;

public static class BoardThemeHelper
{
    public static string GetBoardTexturePath(BoardTheme theme)
    {
        string fileName = theme.ToString().ToLower();
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;  // Caminho absoluto do diretório atual
        string pngPath = Path.Combine(baseDir, "Assets", "Boards", $"{fileName}.png");
        string jpgPath = Path.Combine(baseDir, "Assets", "Boards", $"{fileName}.jpg");

        if (File.Exists(pngPath))
        {
            return pngPath;
        }
        else if (File.Exists(jpgPath))
        {
            return jpgPath;
        }


        Images.LoadImage("Assets/Boards/default.png");
        return "";
    }

    public static void ChangeBoardTheme(BoardTheme theme)
    {
        string newPath = GetBoardTexturePath(theme);
        Console.WriteLine($"Tema do tabuleiro alterado para: {newPath}");

        MainWindow.BoardUrl = newPath; 
        if (MainWindow.Instance.gameState != null)
        {
            MainWindow.Instance.DrawBoard(MainWindow.Instance.gameState.Board);
        }
    }
}

public static class PieceThemeHelper
{
    private static readonly Dictionary<PieceType, ImageSource> whiteSources = new();
    private static readonly Dictionary<PieceType, ImageSource> blackSources = [];

    public static string PieceUrl { get; set; }

    private static void LoadPieceImages()
    {
        foreach (PieceType type in Enum.GetValues(typeof(PieceType)))
        {
            whiteSources[type] = Images.LoadImage($"{PieceUrl}w{GetPieceCode(type)}.svg");
            blackSources[type] = Images.LoadImage($"{PieceUrl}b{GetPieceCode(type)}.svg");
        }
    }

    private static string GetPieceCode(PieceType type) => type switch
    {
        PieceType.Pawn => "P",
        PieceType.Bishop => "B",
        PieceType.Knight => "N",
        PieceType.Rook => "R",
        PieceType.King => "K",
        PieceType.Queen => "Q",
        _ => throw new ArgumentException("Tipo de peça inválido.")
    };

    public static void ChangePieceTheme(PieceTheme theme)
    {
        PieceUrl = $"assets/pieces/{theme.ToString().ToLower()}/";
        LoadPieceImages();
        Console.WriteLine($"Tema das peças alterado para: {PieceUrl}");

        if (MainWindow.Instance.gameState != null)
        {
            MainWindow.Instance.DrawBoard(MainWindow.Instance.gameState.Board);
        }
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
        return piece == null ? null : GetImage(piece.Color, piece.Type);
    }
}

