using System.Windows.Input;

namespace DMSA.Sync.Core.Controls.CustomRows.Lite;

public partial class AccountMoveLineRow : ContentView
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
            typeof(AccountMoveLineRow),
            null);

    public AccountMoveLineRow()
	{
		InitializeComponent();
	}
}