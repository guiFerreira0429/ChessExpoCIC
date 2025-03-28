using System.Windows;
using System.Windows.Controls;

namespace ChessUI;

public partial class PauseMenu : UserControl
{
    public event Action<Option> OptionSelected;

    public PauseMenu()
    {
        InitializeComponent();
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Continue);
    }

    private void Restart_Click(object sender, RoutedEventArgs e)
    {
        OptionSelected?.Invoke(Option.Restart);
    }
}