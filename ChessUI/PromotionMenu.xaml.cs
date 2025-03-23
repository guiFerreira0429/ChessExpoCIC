using ChessLogic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessUI;

public partial class PromotionMenu : UserControl
{
    public event Action<PieceType> PieceSelected;

    public PromotionMenu(Player player, GameType gameType)
    {
        InitializeComponent();

        QueenImg.Source = PieceThemeHelper.GetImage(player, PieceType.Queen, gameType);
        BishopImg.Source = PieceThemeHelper.GetImage(player, PieceType.Bishop, gameType);
        RookImg.Source = PieceThemeHelper.GetImage(player, PieceType.Rook, gameType);
        KnightImg.Source = PieceThemeHelper.GetImage(player, PieceType.Knight, gameType);

        if (gameType == GameType.Disguised)
        {
            var images = new List<UIElement> {
                QueenImg,
                BishopImg,
                RookImg,
                KnightImg
            };

            PromotionGrid.Children.Clear();

            Random random = new Random();
            var shuffledImages = images.OrderBy(x => random.Next()).ToList();

            Dispatcher.Invoke(() => {
                foreach (var image in shuffledImages)
                {
                    PromotionGrid.Children.Add(image);
                }
            });
        }
    }

    private void QueenImg_MouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Queen);
    }

    private void BishopImg_MouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Bishop);
    }

    private void RookImg_MouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Rook);
    }

    private void KnightImg_MouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Knight);
    }
}