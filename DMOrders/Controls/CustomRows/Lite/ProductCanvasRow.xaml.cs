using DMSA.Models.Odoo.Native;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ProductCanvasRow : ContentView
{
    public static readonly BindableProperty ShowPriceProperty =
            BindableProperty.Create(nameof(ShowPrice), typeof(bool), typeof(ProductCanvasRow), false);

    public bool ShowPrice
    {
        get => (bool)GetValue(ShowPriceProperty);
        set => SetValue(ShowPriceProperty, value);
    }

    public static readonly BindableProperty ShowSelectButtonProperty =
        BindableProperty.Create(nameof(ShowSelectButton), typeof(bool), typeof(ProductCanvasRow), false);

    public bool ShowSelectButton
    {
        get => (bool)GetValue(ShowSelectButtonProperty);
        set => SetValue(ShowSelectButtonProperty, value);
    }

    public static readonly BindableProperty ActionButtonProperty =
        BindableProperty.Create(nameof(ActionButton), typeof(ICommand), typeof(ProductCanvasRow), null);

    public ICommand ActionButton
    {
        get => (ICommand)GetValue(ActionButtonProperty);
        set => SetValue(ActionButtonProperty, value);
    }

    public ProductCanvasRow()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(nameof(Item), typeof(product_product), typeof(ProductCanvasRow), null);

    public product_product Item
    {
        get => (product_product)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }
}