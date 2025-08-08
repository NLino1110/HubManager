using DMOrdersUI.Controls.Base;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrdersUI.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class SaleOrderRow : RowAdvance<sale_order>
    {
        public bool IsSynchronized => Item != null && !Item.is_synchronized;

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(SaleOrderRow), null);

        public SaleOrderRow()
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
            };

            // Definir las columnas (puedes usar GridLength.Star para que se repartan equitativamente)
            for (int i = 0; i < 6; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            var idLabel = new Label
            {
                Text = $"Código: {Item.id}",
                FontSize = 12,
                TextColor = Colors.Green,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(idLabel);
            Grid.SetColumn(idLabel, 0);

            var nameLabel = new Label
            {
                Text = Item.partner_display,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(nameLabel);
            Grid.SetColumn(nameLabel, 1);

            var dateOrderLabel = new Label
            {
                Text = Item.date_order.ToString("dd/MM/yyyy"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(dateOrderLabel);
            Grid.SetColumn(dateOrderLabel, 2);

            string date_synchronized = "-";

            if(Item.date_synchronized != DateTime.MinValue)
            {
                date_synchronized = Item.date_synchronized.ToString("dd/MM/yyyy");
            }

            var dateSyncLabel = new Label
            {
                Text = date_synchronized,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(dateSyncLabel);
            Grid.SetColumn(dateSyncLabel, 3);

            var totalLabel = new Label
            {
                Text = Item.amount_total.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(totalLabel);
            Grid.SetColumn(totalLabel, 4);

            var syncLabel = new Label
            {
                Text = Item.is_synchronized.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Children.Add(syncLabel);
            Grid.SetColumn(syncLabel, 5);

            //var btnSelect = new Button
            //{
            //    Command = PopupSelectBrand.CommandSelectListItem,
            //    HeightRequest = 35,
            //    WidthRequest = 35,
            //    BackgroundColor = Colors.DodgerBlue,
            //    Text = "",
            //    TextColor = Colors.White,
            //    FontAttributes = FontAttributes.Bold,
            //    FontSize = 12,
            //    HorizontalOptions = LayoutOptions.Center,
            //    ImageSource = new FontImageSource
            //    {
            //        FontFamily = "FontAwesome5Solid",
            //        Color = Colors.White,
            //        Size = 15,
            //        FontAutoScalingEnabled = true,
            //        Glyph = "\uf058"
            //    },
            //    Padding = new Thickness(3),
            //    Margin = new Thickness(2),
            //};

            //btnSelect.SetBinding(Button.CommandParameterProperty, new Binding("."));
            //grid.Children.Add(btnSelect);
            //Grid.SetColumn(btnSelect, 6);

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

            buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));
            buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));
            buttonEdit.SetBinding(Button.IsVisibleProperty, new Binding("IsSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));

            var buttonDelete = new Button
            {
                Command = EditCommand,
                CommandParameter = "",
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

            buttonDelete.SetBinding(Button.IsVisibleProperty, new Binding("IsSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));

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
