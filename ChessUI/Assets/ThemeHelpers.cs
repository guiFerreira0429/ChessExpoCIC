using ChessLogic;
using System.IO;
using System.Windows.Media;

namespace ChessUI;

public static class BoardThemeHelper
{
    public static string GetBoardTexturePath(BoardTheme theme)
    {
        try
        {
            string fileName = theme.ToString().ToLower();
            string pngPath = $"Assets\\Boards\\{fileName}.png";
            string jpgPath = $"Assets\\Boards\\{fileName}.jpg";

            if (File.Exists(pngPath))
            {
                return pngPath;
            }
            else if (File.Exists(jpgPath))
            {
                return jpgPath;
            }

            return "";
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao encontrar Tema de Tabuleiro: {ex.Message}");
        }
    }

    public static void ChangeBoardTheme(BoardTheme theme)
    {
        string newPath = GetBoardTexturePath(theme);

        if (!String.IsNullOrEmpty(newPath))
        {
            MainWindow.BoardUrl = newPath;
            MainWindow.Instance.BoardGridBrush.ImageSource = Images.LoadImage(newPath);
        }
    }
}

public static class PieceThemeHelper
{
    private static readonly Dictionary<PieceType, ImageSource> whiteSources = [];
    private static readonly Dictionary<PieceType, ImageSource> blackSources = [];

    public static string PieceUrl { get; set; }

    public static void LoadPieceImages()
    {
        foreach (PieceType type in Enum.GetValues(typeof(PieceType)))
        {
            whiteSources[type] = Images.LoadSvg($"{PieceUrl}w{GetPieceCode(type)}.svg");
            blackSources[type] = Images.LoadSvg($"{PieceUrl}b{GetPieceCode(type)}.svg");
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
    }

    public static ImageSource GetImage(Player color, PieceType type, GameType gameType)
    {
        if (gameType == GameType.Disguised)
        {
            return color switch
            {
                Player.White => Images.LoadSvg($"assets/pieces/disguised/w.svg"),
                Player.Black => Images.LoadSvg($"assets/pieces/disguised/b.svg"),
                _ => null
            };
        }
        else if (gameType == GameType.Mono)
        {
            return Images.LoadSvg($"assets/pieces/mono/{GetPieceCode(type)}.svg");
        }
        else
        {
            return color switch
            {
                Player.White => whiteSources[type],
                Player.Black => blackSources[type],
                _ => null
            };
        }
    }

    public static ImageSource GetImage(Piece piece, GameType gameType)
    {
        return piece == null ? null : GetImage(piece.Color, piece.Type, gameType);
    }
}

public static class ColorThemeHelper
{
    private static Dictionary<ColorTheme, ColorThemeInfo> DicionariosCores = new Dictionary<ColorTheme, ColorThemeInfo>
    {
        {
            ColorTheme.Default, new ColorThemeInfo
            {
                Theme = ColorTheme.Default,
                Name = "Default",
                StrokeColor = (Color)ColorConverter.ConvertFromString("#260000"),
                FillColor = (Color)ColorConverter.ConvertFromString("#cc380404"),
                TextColor = (Color)ColorConverter.ConvertFromString("#ffa274"),
                ButtonColor = (Color)ColorConverter.ConvertFromString("#730909")
            }
        },
        {
            ColorTheme.Blue, new ColorThemeInfo
            {
                Theme = ColorTheme.Blue,
                Name = "Blue",
                StrokeColor = (Color)ColorConverter.ConvertFromString("#001a33"),
                FillColor = (Color)ColorConverter.ConvertFromString("#cc0a3a66"),
                TextColor = (Color)ColorConverter.ConvertFromString("#74c2ff"),
                ButtonColor = (Color)ColorConverter.ConvertFromString("#094573")
            }
        },
        {
            ColorTheme.Green, new ColorThemeInfo
            {
                Theme = ColorTheme.Green,
                Name = "Green",
                StrokeColor = (Color)ColorConverter.ConvertFromString("#002611"),
                FillColor = (Color)ColorConverter.ConvertFromString("#cc0a6642"),
                TextColor = (Color)ColorConverter.ConvertFromString("#74ffa2"),
                ButtonColor = (Color)ColorConverter.ConvertFromString("#097345")
            }
        },
        {
            ColorTheme.Purple, new ColorThemeInfo
            {
                Theme = ColorTheme.Purple,
                Name = "Purple",
                StrokeColor = (Color)ColorConverter.ConvertFromString("#190033"),
                FillColor = (Color)ColorConverter.ConvertFromString("#cc380466"),
                TextColor = (Color)ColorConverter.ConvertFromString("#c974ff"),
                ButtonColor = (Color)ColorConverter.ConvertFromString("#450973")
            }
        },
        {
            ColorTheme.Dark, new ColorThemeInfo
            {
                Theme = ColorTheme.Dark,
                Name = "Dark",
                StrokeColor = (Color)ColorConverter.ConvertFromString("#000000"),
                FillColor = (Color)ColorConverter.ConvertFromString("#cc181818"),
                TextColor = (Color)ColorConverter.ConvertFromString("#aaaaaa"),
                ButtonColor = (Color)ColorConverter.ConvertFromString("#333333")
            }
        }
    };

    public static List<ColorThemeInfo> GetAllThemes()
    {
        return DicionariosCores.Values.ToList();
    }

    public static ColorThemeInfo GetThemeInfo(ColorTheme theme)
    {
        return DicionariosCores[theme];
    }
}
