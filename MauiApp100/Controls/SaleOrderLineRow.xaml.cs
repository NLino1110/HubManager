using DMSA.Models.Odoo.Native;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRowsV2;

public partial class SaleOrderLineRow : ContentView
{
    public static readonly BindableProperty LockEditionProperty =
        BindableProperty.Create(
            nameof(LockEdition),
            typeof(bool),
            typeof(SaleOrderLineRow),
            false);

    public bool LockEdition
    {
        get => (bool)GetValue(LockEditionProperty);
        set => SetValue(LockEditionProperty, value);
    }

    public sale_order_line Item
    {
        get => (sale_order_line)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(
            nameof(Item),
            typeof(sale_order_line),
            typeof(SaleOrderLineRow),
            propertyChanged: OnItemChanged);

    static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SaleOrderLineRow row)
        {
            row.BindingContext = newValue;
        }
    }

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(SaleOrderLineRow));

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public static readonly BindableProperty DeleteCommandProperty =
        BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(SaleOrderLineRow));


    public SaleOrderLineRow()
	{
		InitializeComponent();
	}
}