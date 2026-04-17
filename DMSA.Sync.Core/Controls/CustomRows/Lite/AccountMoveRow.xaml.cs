using System.Windows.Input;

namespace DMSA.Sync.Core.Controls.CustomRows.Lite;

public partial class AccountMoveRow : ContentView
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
            typeof(AccountMoveRow),
            null);

    public AccountMoveRow()
	{
		InitializeComponent();
	}
}