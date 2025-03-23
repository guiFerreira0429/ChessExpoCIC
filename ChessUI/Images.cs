using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace ChessUI;

public static class Images
{
    public static ImageSource LoadImage(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Arquivo '{filePath}' não encontrado.");
            }
            return new BitmapImage(new Uri(filePath, UriKind.Relative));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar a imagem '{filePath}': {ex.Message}");
            return null;
        }
    }

    public static ImageSource LoadSvg(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Arquivo '{filePath}' não encontrado.");
            }

            var settings = new WpfDrawingSettings
            {
                IncludeRuntime = true,
                TextAsGeometry = true
            };

            var converter = new FileSvgReader(settings);
            DrawingGroup drawing = converter.Read(filePath);

            if (drawing == null)
            {
                throw new Exception("Falha ao processar o arquivo SVG.");
            }

            return new DrawingImage(drawing);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar SVG '{filePath}': {ex.Message}");
            return null;
        }
    }
}
