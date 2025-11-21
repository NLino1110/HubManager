using CommunityToolkit.Maui.Converters;
using DMOrders.Controls.Base;
using DMOrders.Converters;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Native;
using SkiaSharp;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Headers
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public class SaleOrderLineRowHeader : RowAdvance<SaleOrderLineHeader>
    {
        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(SaleOrderLineRow), null);

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public static readonly BindableProperty DeleteCommandProperty =
            BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(SaleOrderLineRow), null);

        public SaleOrderLineRowHeader()
        {
            
        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            base.BuildLeftGridContent(leftGrid);

            if (Item == null)
                return;

            var grid = new Grid
            {
                ColumnSpacing = 8,
                Padding = 4,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.LightGray,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto},
                    //new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star)},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Star},
                    new ColumnDefinition { Width = GridLength.Auto},

                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },                    
                },
            };

            var idLabel = new Label
            {
                Text = $"{Item.Number}",
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start
            };
            grid.Children.Add(idLabel);
            Grid.SetColumn(idLabel, 0);            

            var codeLabel = new Label
            {
                Text = Item.Product,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start
            };           

            grid.Children.Add(codeLabel);
            Grid.SetRow(codeLabel, 0);
            Grid.SetColumn(codeLabel, 1);

            var nameLabel = new Label
            {
                Text = Item.UOM,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start
            };

            grid.Children.Add(nameLabel);
            Grid.SetRow(nameLabel, 0);
            Grid.SetColumn(nameLabel, 2);            

            var qty_real = new Label
            {
                Text = Item.QtyReal,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill
            };

            //qty_real.SetBinding(Label.TextProperty, new Binding("product_uom_qty_real"));

            grid.Children.Add(qty_real);            
            Grid.SetColumn(qty_real, 3);

            var qty = new Label
            {
                Text = Item.QtyDisp,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill
            };

            //qty.SetBinding(Label.TextProperty, new Binding("product_uom_qty"));

            grid.Children.Add(qty);
            Grid.SetColumn(qty, 4);

            //var priceLabel = new Label
            //{
            //    //Text = Item.price_unit.ToString("F3"),
            //    FontAttributes = FontAttributes.Bold,
            //    FontSize = 14,
            //    TextColor = Colors.Black,
            //    HorizontalOptions = LayoutOptions.Center
            //};

            //priceLabel.SetBinding(Label.TextProperty, new Binding("price_unit", stringFormat: "{0:F3}"));

            //grid.Children.Add(priceLabel);
            //Grid.SetColumn(priceLabel, 5);

            var priceLabel = new Label
            {
                Text = Item.Price,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End
            };

            //priceLabel.SetBinding(Label.TextProperty, new Binding("virtual_price_no_tax", stringFormat: "{0:N3}"));

            grid.Children.Add(priceLabel);
            Grid.SetColumn(priceLabel, 5);

            var priceUnitLabel = new Label
            {
                Text = Item.PriceTax,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End
            };

            //priceUnitLabel.SetBinding(Label.TextProperty, new Binding("price_unit", stringFormat: "{0:N3}"));

            grid.Children.Add(priceUnitLabel);
            Grid.SetColumn(priceUnitLabel, 6);

            var subtotalLabel = new Label
            {
                Text = Item.SubTotalNt,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End
            };

            //subtotalLabel.SetBinding(Label.TextProperty, new Binding("price_subtotal", stringFormat: "{0:N3}"));
            grid.Children.Add(subtotalLabel);
            Grid.SetColumn(subtotalLabel, 7);                    

            var discountPercentLabel = new Label
            {
                Text = Item.DiscountPercent,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End
            };
            //discountPercentLabel.SetBinding(Label.TextProperty, new Binding("discount", stringFormat: "{0:N0}"));
            grid.Children.Add(discountPercentLabel);
            Grid.SetColumn(discountPercentLabel, 8);            

            var discountLabel = new Label
            {
                Text = Item.DiscountValue,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End
            };
            //discountLabel.SetBinding(Label.TextProperty, new Binding("amount_discount", stringFormat: "{0:N3}"));
            grid.Children.Add(discountLabel);
            Grid.SetColumn(discountLabel, 9);            

            var taxLabel = new Label
            {
                Text = Item.Tax,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End
            };
            //taxLabel.SetBinding(Label.TextProperty, new Binding("price_tax", stringFormat: "{0:N3}"));
            grid.Children.Add(taxLabel);
            Grid.SetColumn(taxLabel, 10);

            var totalLabel = new Label
            {
                Text = Item.Total,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End
            };
            //totalLabel.SetBinding(Label.TextProperty, new Binding("price_total", stringFormat: "{0:N3}"));
            grid.Children.Add(totalLabel);
            Grid.SetColumn(totalLabel, 11);

            var cell = CreateCell(grid, padding: new Thickness(4), backgroundColor: Colors.Transparent);
            AddCell(cell, region: "left", row: 0, column: 0);
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            //var buttonGift = new Button
            //{
            //    HeightRequest = 23,
            //    WidthRequest = 23,
            //    BackgroundColor = Colors.Transparent,
            //    Text = "",
            //    TextColor = Colors.DodgerBlue,
            //    FontAttributes = FontAttributes.Bold,
            //    FontSize = 12,
            //    HorizontalOptions = LayoutOptions.Center,
            //    ImageSource = new FontImageSource
            //    {
            //        FontFamily = "FontAwesome5Solid",
            //        Color = Colors.DodgerBlue,
            //        Size = 10,
            //        FontAutoScalingEnabled = true,
            //        Glyph = "\uf06b"
            //    },
            //    Padding = new Thickness(3),
            //    Margin = new Thickness(2),
            //};

            //buttonGift.SetBinding(Button.IsVisibleProperty, new Binding("is_gift"));
            //var opacityConverter = new BoolToOpacityConverter();
            //var inverseBool = new InverseBooleanConverter();

            //buttonGift.SetBinding(VisualElement.OpacityProperty,
            //    new Binding("is_gift", converter: opacityConverter));

            //buttonGift.SetBinding(InputView.InputTransparentProperty,
            //    new Binding("is_gift", converter: inverseBool));

            // opcional: deshabilitar cuando no visible para seguridad
            //buttonGift.SetBinding(VisualElement.IsEnabledProperty,
            //    new Binding("is_gift", converter: opacityConverter));

            //var buttonEdit = new Button
            //{
            //    HeightRequest = 35,
            //    WidthRequest = 35,
            //    BackgroundColor = Colors.DodgerBlue,
            //    Text = "",
            //    TextColor = Colors.White,
            //    FontAttributes = FontAttributes.Bold,
            //    FontSize = 12,
            //    HorizontalOptions = LayoutOptions.Center,
            //    IsVisible = false,
            //    ImageSource = new FontImageSource
            //    {
            //        FontFamily = "FontAwesome5Solid",
            //        Color = Colors.White,
            //        Size = 15,
            //        FontAutoScalingEnabled = true,
            //        Glyph = "\uf303"
            //    },
            //    Padding = new Thickness(3),
            //    Margin = new Thickness(2),                
            //};

            //buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderLineRow))));
            //buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            //var buttonDelete = new Button
            //{                
            //    HeightRequest = 35,
            //    WidthRequest = 35,
            //    BackgroundColor = Colors.OrangeRed,
            //    Text = "",
            //    TextColor = Colors.White,
            //    FontAttributes = FontAttributes.Bold,
            //    FontSize = 12,
            //    HorizontalOptions = LayoutOptions.Center,
            //    IsVisible = true,
            //    ImageSource = new FontImageSource
            //    {
            //        FontFamily = "FontAwesome5Solid",
            //        Color = Colors.White,
            //        Size = 15,
            //        FontAutoScalingEnabled = true,
            //        Glyph = "\uf2ed"
            //    },
            //    Padding = new Thickness(3),
            //    Margin = new Thickness(2),
            //};

            //buttonDelete.SetBinding(Button.CommandProperty, new Binding("DeleteCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderLineRow))));
            //buttonDelete.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0),
                BackgroundColor = Colors.LightGray,
                MinimumWidthRequest = 70,
            };

            //stackLayout.Children.Add(buttonGift);
            //stackLayout.Children.Add(buttonEdit);
            //stackLayout.Children.Add(buttonDelete);

            toolGrid.Children.Add(stackLayout);
            Grid.SetRow(stackLayout, 0);
            Grid.SetRowSpan(stackLayout, 2);
            Grid.SetColumn(stackLayout, 4);
        }

    }
}
