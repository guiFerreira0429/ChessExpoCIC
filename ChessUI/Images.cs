using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessLogic;

namespace ChessUI;

public static class Images
{
    public static ImageSource LoadImage(string resourcePath)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/ChessUI;component/{resourcePath}");
            return new BitmapImage(uri);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar a imagem '{resourcePath}': {ex.Message}");
            return null;
        }
    }
}
