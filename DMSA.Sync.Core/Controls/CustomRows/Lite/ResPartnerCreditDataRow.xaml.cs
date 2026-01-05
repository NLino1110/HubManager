using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ResPartnerCreditDataRow : ContentView
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
            typeof(ResPartnerCreditDataRow),
            null);

    public ResPartnerCreditDataRow()
	{
		InitializeComponent();
	}
}