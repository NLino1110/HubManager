using DMOrders.Controls.Base;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class SaleOrderRow : RowAdvance<sale_order>
    {
        public bool IsSynchronized => Item != null && !Item.is_synchronized;
        public bool IsNotSynchronized => Item != null && Item.is_synchronized;

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(SaleOrderRow), null);

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public static readonly BindableProperty DeleteCommandProperty =
            BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(SaleOrderRow), null);

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
                ColumnSpacing = 10,
                Padding = 4,
                HorizontalOptions = LayoutOptions.Fill,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            //grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star)});
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            var idLabel = new Label
            {
                Text = $"{Item.id}",
                FontSize = 12,
                TextColor = Colors.Green,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Children.Add(idLabel);
            Grid.SetColumn(idLabel, 0);

            var nameLabel = new Label
            {
                Text = Item.partner_display_name,
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                MaxLines = 2,
                LineBreakMode = LineBreakMode.TailTruncation,
                VerticalOptions = LayoutOptions.Center
            };

            grid.Children.Add(nameLabel);
            Grid.SetColumn(nameLabel, 1);

            var dateOrderLabel = new Label
            {
                Text = Item.date_order.ToString("dd/MM/yyyy HH:mm:ss"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
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
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Children.Add(dateSyncLabel);
            Grid.SetColumn(dateSyncLabel, 3);

            var totalLabel = new Label
            {
                Text = Item.amount_total.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Children.Add(totalLabel);
            Grid.SetColumn(totalLabel, 4);

            var syncLabel = new Label
            {
                Text = Item.is_synchronized.ToString(),
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
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
            //buttonEdit.SetBinding(Button.IsVisibleProperty, new Binding("IsSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));

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

            buttonDelete.SetBinding(Button.CommandProperty, new Binding("DeleteCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));
            buttonDelete.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));
            //buttonDelete.SetBinding(Button.IsVisibleProperty, new Binding("IsSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));

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

            var buttonEditDummy = new Button
            {
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.DarkGray,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                IsEnabled = false,
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

            //buttonEditDummy.SetBinding(Button.IsVisibleProperty, new Binding("IsSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));

            var buttonDeleteDummy = new Button
            {
                Command = EditCommand,
                CommandParameter = "",
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.DarkGray,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                IsEnabled = false,
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

            //buttonDeleteDummy.SetBinding(Button.IsVisibleProperty, new Binding("IsSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));

            var stackLayoutDummy = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0),
                BackgroundColor = Colors.Transparent
            };

            stackLayoutDummy.Children.Add(buttonEditDummy);
            stackLayoutDummy.Children.Add(buttonDeleteDummy);

            toolGrid.Children.Add(stackLayoutDummy);
            Grid.SetRow(stackLayoutDummy, 0);
            Grid.SetRowSpan(stackLayoutDummy, 2);
            Grid.SetColumn(stackLayoutDummy, 4);

            stackLayout.SetBinding(Button.IsVisibleProperty, new Binding("IsSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));
            stackLayoutDummy.SetBinding(Button.IsVisibleProperty, new Binding("IsNotSynchronized", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(SaleOrderRow))));            
        }
    }
}
