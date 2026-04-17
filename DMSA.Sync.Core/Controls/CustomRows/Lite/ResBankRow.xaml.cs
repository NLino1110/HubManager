using DMSA.Models.Odoo.Accounting;
using System.Windows.Input;

namespace DMSA.Sync.Core.Controls.CustomRows.Lite;

public partial class ResBankRow : ContentView
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
            typeof(ResBank),
            null);

    public ResBankRow()
	{
		InitializeComponent();
	}
}