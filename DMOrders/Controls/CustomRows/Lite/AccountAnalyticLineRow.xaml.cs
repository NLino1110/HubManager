using DMSA.Models.Odoo.Tareas;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class AccountAnalyticLineRow : ContentView
{
    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(AccountAnalyticLineRow));

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public static readonly BindableProperty DeleteCommandProperty =
        BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(AccountAnalyticLineRow));

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    //public static readonly BindableProperty ItemProperty =
    //    BindableProperty.Create(nameof(Item), typeof(AccountAnalyticLineRow), typeof(AccountAnalyticLineRow), null);

    //public AccountAnalyticLineRow Item
    //{
    //    get => (AccountAnalyticLineRow)GetValue(ItemProperty);
    //    set => SetValue(ItemProperty, value);
    //}

    public AccountAnalyticLineRow()
	{
		InitializeComponent();
	}
}