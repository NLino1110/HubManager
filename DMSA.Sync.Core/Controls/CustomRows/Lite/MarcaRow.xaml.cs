using System.Windows.Input;

namespace DMSA.Sync.Core.Controls.CustomRows.Lite;

public partial class MarcaRow : ContentView
{
    public ICommand ActionCommand
    {
        get => (ICommand)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(MarcaRow),
            null);

    public MarcaRow()
	{
		InitializeComponent();
	}

    //private void OnPressed(object sender, PointerEventArgs e)
    //{
    //    RootBorder.BackgroundColor = Color.FromArgb("#E6F0FF");
    //    RootBorder.Stroke = Colors.DodgerBlue;
    //}

    //private void OnReleased(object sender, PointerEventArgs e)
    //{
    //    RootBorder.BackgroundColor = Colors.FloralWhite;
    //    RootBorder.Stroke = Colors.LightGray;
    //}
}