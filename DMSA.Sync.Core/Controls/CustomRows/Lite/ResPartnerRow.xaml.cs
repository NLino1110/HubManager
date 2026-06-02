using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ResPartnerRow : ContentView
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
            typeof(ResPartnerRow),
            null);

    public ResPartnerRow()
	{
		InitializeComponent();
	}
}