using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ResPartnerSingleRow : ContentView
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
            typeof(ResPartnerSingleRow),
            null);

    public ResPartnerSingleRow()
	{
		InitializeComponent();
	}
}