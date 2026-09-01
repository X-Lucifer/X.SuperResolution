using Avalonia.Controls;

namespace X.SuperResolution.Controls;

public partial class ActivityPanel : UserControl
{
    public ActivityPanel()
    {
        InitializeComponent();
    }

    private void InfoTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        InfoTextBox.CaretIndex = InfoTextBox.Text?.Length ?? 0;
    }
}