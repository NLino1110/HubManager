using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ResPartnerSingle : ContentView
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

    public ResPartnerSingle()
	{
		InitializeComponent();
	}
}