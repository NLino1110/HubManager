using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ProductViewerRow : ContentView
{
	public ProductViewerRow()
	{
		InitializeComponent();
	}

    // -------------------------
    // ITEM
    // -------------------------
    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(
            nameof(Item),
            typeof(product_product),
            typeof(ProductViewerRow),
            null,
            propertyChanged: OnItemChanged);

    public product_product Item
    {
        get => (product_product)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    // -------------------------
    // EDIT COMMAND (igual al original)
    // -------------------------
    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(
            nameof(EditCommand),
            typeof(ICommand),
            typeof(ProductViewerRow),
            null);

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    // -------------------------
    // ITEM CHANGED
    // -------------------------
    private static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (ProductViewerRow)bindable;
        view.BindingContext = newValue;

        if (newValue is product_product product)
            view.LoadImageAsync(product);
    }

    // -------------------------
    // IMAGE LOADING (MISMA LÓGICA)
    // -------------------------
    private void LoadImageAsync(product_product item)
    {
        Activity.IsRunning = true;
        Activity.IsVisible = true;
        ProductImage.IsVisible = false;

        _ = Task.Run(() =>
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(item.image_1920) &&
                    !item.image_1920.Equals("false"))
                {
                    var bytes = Convert.FromBase64String(item.image_1920);
                    var stream = new MemoryStream(bytes);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProductImage.Source = ImageSource.FromStream(() => stream);
                        FinishImageLoad();
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProductImage.Source = "image_not_found_gray_opt.png";
                        FinishImageLoad();
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ProductImage.Source = "image_not_found_gray_opt.png";
                    FinishImageLoad();
                });
            }
        });
    }

    private void FinishImageLoad()
    {
        Activity.IsRunning = false;
        Activity.IsVisible = false;
        ProductImage.IsVisible = true;
    }

}