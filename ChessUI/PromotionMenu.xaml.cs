﻿using ChessLogic;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessUI;

/// <summary>
/// Interação lógica para PromotionMenu.xam
/// </summary>
public partial class PromotionMenu : UserControl
{
    public event Action<PieceType> PieceSelected;

    public PromotionMenu(Player player)
    {
        InitializeComponent();

        QueenImg.Source = PieceThemeHelper.GetImage(player, PieceType.Queen, GameType.Default);
        BishopImg.Source = PieceThemeHelper.GetImage(player, PieceType.Bishop, GameType.Default);
        RookImg.Source = PieceThemeHelper.GetImage(player, PieceType.Rook, GameType.Default);
        KnightImg.Source = PieceThemeHelper.GetImage(player, PieceType.Knight, GameType.Default);
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
