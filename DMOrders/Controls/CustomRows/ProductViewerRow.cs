using DMOrders.Controls.Base;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public class ProductViewerRow : RowAdvance<product_product>
    {
        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ActivityRow), null);

        public ProductViewerRow()
        {            
            BackgroundColor = Colors.Red;
        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            base.BuildLeftGridContent(leftGrid);

            if (Item == null)
                return;

            var nameLabel = new Label
            {
                Text = Item.name,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black
            };

            var codeLabel = new Label
            {
                Text = $"Código: {Item.default_code}",
                FontSize = 12,
                TextColor = Colors.Gray
            };

            var _barCodeLabel = new Label {
                Text = $"Barcode: {Item.barcode}",
                FontSize = 12, TextColor = Colors.DimGray, InputTransparent = true };

            //uom_display
            var priceLabel = new Label
            {
                Text = $"Precio: {Item.list_price:C4}",
                FontSize = 12,
                TextColor = Colors.DarkGreen
            };

            var activityIndicator = new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var image = new Image
            {
                HeightRequest = 400,
                WidthRequest = 400,
                Aspect = Aspect.AspectFill,
                IsVisible = false
            };

            var imageContainer = new Grid
            {
                HeightRequest = 450,
                WidthRequest = 450,
                Children =
                {
                    image,
                    activityIndicator
                }
            };

            // Inicia la carga de la imagen en un hilo aparte
            _ = Task.Run(async () =>
            {
                try
                {
                    string img_string = Item.image_1920;
                    if (!string.IsNullOrWhiteSpace(img_string) && !img_string.Equals("false"))
                    {
                        Debug.WriteLine("===================");
                        Debug.WriteLine("ImageString to Bytes");
                        byte[] imageBytes = Convert.FromBase64String(img_string);
                        Debug.WriteLine("Create Stream");
                        var stream = new MemoryStream(imageBytes);

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Debug.WriteLine("Put Image from ImageSource");
                            image.Source = ImageSource.FromStream(() => stream);
                            Debug.WriteLine("Done.");
                            //image.Source = "image_not_found.png";
                            activityIndicator.IsRunning = false;
                            activityIndicator.IsVisible = false;
                            image.IsVisible = true;
                        });
                    }
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            image.Source = "image_not_found_gray_opt.png";
                            activityIndicator.IsRunning = false;
                            activityIndicator.IsVisible = false;
                            image.IsVisible = true;
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Puedes loguear o manejar errores aquí
                    Console.WriteLine("Error loading image: " + ex.Message);
                }
            });

            var contentStack = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Padding = 4,
                Children = { nameLabel, codeLabel, _barCodeLabel, priceLabel }
            };

            var containerGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                }
            };

            containerGrid.Children.Add(contentStack);
            containerGrid.Children.Add(imageContainer);
            Grid.SetRow(contentStack, 0);
            Grid.SetRow(imageContainer, 0);
            Grid.SetRowSpan(imageContainer, 2);

            AddCell(containerGrid, region: "left", row: 0, column: 0);
        }
    }
}
