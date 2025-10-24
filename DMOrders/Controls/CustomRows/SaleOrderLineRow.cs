using DMOrders.Controls.Base;
using DMSA.Models.Odoo.Native;
using SkiaSharp;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public class SaleOrderLineRow : RowAdvance<sale_order_line>
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

        public SaleOrderLineRow()
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
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto},
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
                    new RowDefinition { Height = GridLength.Star },
                },
            };

            var idLabel = new Label
            {
                Text = $"{Item.ordinal}",
                FontSize = 12,
                TextColor = Colors.Green,
                HorizontalOptions = LayoutOptions.Start
            };
            grid.Children.Add(idLabel);
            Grid.SetColumn(idLabel, 0);
            Grid.SetRowSpan(idLabel, 2);

            var codeLabel = new Label
            {
                Text = Item.product_code,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Green,
                HorizontalOptions = LayoutOptions.Start
            };
            grid.Children.Add(codeLabel);
            Grid.SetRow(codeLabel, 0);
            Grid.SetColumn(codeLabel, 1);

            var nameLabel = new Label
            {
                Text = Item.product_display,
                FontAttributes = FontAttributes.None,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill
            };

            grid.Children.Add(nameLabel);
            Grid.SetRow(nameLabel, 1);
            Grid.SetColumn(nameLabel, 1);
            Grid.SetColumnSpan(nameLabel, 6);

            var productUnd = new Label
            {
                Text = Item.uom_category_display,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill
            };
            grid.Children.Add(productUnd);
            Grid.SetRow(productUnd, 0);
            Grid.SetColumn(productUnd, 2);

            var qty_real = new Label
            {
                //Text = Item.qty_to_deliver.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill
            };

            qty_real.SetBinding(Label.TextProperty, new Binding("product_uom_qty_real"));

            grid.Children.Add(qty_real);            
            Grid.SetColumn(qty_real, 3);

            var qty = new Label
            {
                //Text = Item.qty_to_deliver.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill
            };

            qty.SetBinding(Label.TextProperty, new Binding("product_uom_qty"));

            grid.Children.Add(qty);
            Grid.SetColumn(qty, 4);

            var priceLabel = new Label
            {
                Text = Item.price_unit.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(priceLabel);
            Grid.SetColumn(priceLabel, 5);
            
            var subtotalLabel = new Label
            {
                Text = Item.price_subtotal.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(subtotalLabel);
            Grid.SetColumn(subtotalLabel, 6);                    

            var discountPercentLabel = new Label
            {
                Text = Item.discount.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(discountPercentLabel);
            Grid.SetColumn(discountPercentLabel, 7);            

            var discountLabel = new Label
            {
                Text = Item.discount.ToString(), //CALCULAR PORCENTAJE
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(discountLabel);
            Grid.SetColumn(discountLabel, 8);            

            var taxLabel = new Label
            {
                Text = Item.price_tax.ToString(), //CALCULAR PORCENTAJE
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(taxLabel);
            Grid.SetColumn(taxLabel, 9);            

            var cell = CreateCell(grid, padding: new Thickness(4), backgroundColor: Colors.Transparent);
            AddCell(cell, region: "left", row: 0, column: 0);
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            var buttonEdit = new Button
            {
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.DodgerBlue,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = true,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf303"
                },
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderLineRow))));
            buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            var buttonDelete = new Button
            {                
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.OrangeRed,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = true,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf2ed"
                },
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            buttonDelete.SetBinding(Button.CommandProperty, new Binding("DeleteCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderLineRow))));
            buttonDelete.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0),
                BackgroundColor = Colors.Transparent
            };

            stackLayout.Children.Add(buttonEdit);
            stackLayout.Children.Add(buttonDelete);

            toolGrid.Children.Add(stackLayout);
            Grid.SetRow(stackLayout, 0);
            Grid.SetRowSpan(stackLayout, 2);
            Grid.SetColumn(stackLayout, 4);
        }

    }
}
