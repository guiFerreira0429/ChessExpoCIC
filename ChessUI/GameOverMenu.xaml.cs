using System.Windows;
using System.Windows.Controls;
using ChessLogic;

namespace ChessUI;

public partial class GameOverMenu : UserControl
{
    public event Action
        <Option> OptionSelected;

    public GameOverMenu(GameState gameState)
    {
        InitializeComponent();
        Result result = gameState.Result;
        WinnerText.Text = GetWinnerText(result.Winner);
        ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer);
    }

    private static string GetWinnerText(Player winner)
    {
        return winner
        switch
        {
            Player.White => "Brancas ganharam!",
            Player.Black => "Pretas ganharam!",
            _ => "Empate!"
        };
    }

    private static string PlayerString(Player player)
    {
        return player
        switch
        {
            Player.White => "Brancas",
            Player.Black => "Pretas",
            _ => ""
        };
    }

    private static string GetReasonText(EndReason reason, Player currentPlayer)
    {
        return reason
        switch
        {
            EndReason.Stalemate => $"StaleMate - Empate",
            EndReason.Checkmate => $"CheckMate - {PlayerString(currentPlayer)} Perderam",
            EndReason.FiftyMoveRule => "Regra das 50 jogadas - Empate",
            EndReason.ThreefoldRepetition => "Repetição de jogadas - Empate",
            EndReason.TimeOut => $"Tempo esgotado - {PlayerString(currentPlayer)} Perderam",
            _ => ""
        };
    }

    private void Restart_Click(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Restart);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Exit);
    }
}