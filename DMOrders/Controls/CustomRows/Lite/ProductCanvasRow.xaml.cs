using DMSA.Models.Odoo.Native;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ProductCanvasRow : ContentView
{
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