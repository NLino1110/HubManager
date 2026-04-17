using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using System.Windows.Input;

namespace DMSA.Sync.Core.Controls.CustomRows.Lite;

public partial class ResCityRow : ContentView
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
            typeof(res_city),
            null);

    public ResCityRow()
	{
		InitializeComponent();
	}
}