using CobranzasDMSA_Odoo.Models;
using CommunityToolkit.Maui.Behaviors;
using DMOrders.Controls.Base;
using DMOrders.Pages.Fragments.Orders.modals;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public class ProductRow : RowAdvance<product_product>
    {
        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ActivityRow), null);

        public ProductRow()
        {
            //WidthRequest = 160;
            //HeightRequest = 120;
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
                TextColor = Colors.Black,
                MinimumHeightRequest = 40,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2
            };

            var codeLabel = new Label
            {
                Text = $"Código: {Item.default_code}",
                FontSize = 12,
                TextColor = Colors.Gray
            };

            var priceLabel = new Label
            {
                Text = $"Precio: {Item.list_price:C}",
                FontSize = 12,
                TextColor = Colors.DarkGreen
            };

            var priceBaseLabel = new Label
            {
                Text = $"PVP Base: {Item.list_price:C}",
                FontSize = 12,
                TextColor = Colors.DarkGreen
            };

            var groupStack1 = new VerticalStackLayout
            {                
                HorizontalOptions = LayoutOptions.Start,                
                Children = { priceLabel, priceBaseLabel }
            };


            var stockLabel = new Label
            {
                Text = $"Stock: {Item.qty_available}",
                FontSize = 12,
                TextColor = Colors.DarkGreen
            };

            var unitLabel = new Label
            {
                Text = $"Unidad: {Item._uom_id}",
                FontSize = 12,
                TextColor = Colors.DarkGreen
            };

            var groupStack2 = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(30, 0, 0, 0),
                Children = { stockLabel, unitLabel }
            };

            var groupStack3 = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                Padding = 4,
                Children = { groupStack1, groupStack2 }
            };

            var contentStack = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
                Padding = 4,                
                Children = { nameLabel, codeLabel, groupStack3 }
            };

            Button btnSelect = new Button
            {
                Command = CatalogViewer.CommandSelectListItem,
                HeightRequest = 35,                
                BackgroundColor = Colors.DodgerBlue,
                Text = "Seleccionar",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Fill,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf058"
                },
                Padding = new Thickness(3),
            };

            btnSelect.SetBinding(Button.CommandParameterProperty, new Binding("."));
            contentStack.Children.Add(btnSelect);

            var cell = CreateCell(contentStack, padding: new Thickness(4), backgroundColor: Colors.Transparent);
            AddCell(cell, region: "left", row: 0, column: 0);
        }
    }

    //public class ProductRow2 : BaseRow<product_product>
    //{
    //    Label labelId { get; set; }
    //    Label labelCode { get; set; }
    //    Label labelName { get; set; }
    //    Label labelUnit { get; set; }
    //    Label labelBrand { get; set; }
    //    Label labelCategory { get; set; }
    //    //Label labelStartDate { get; set; }
    //    //Label labelEndDate { get; set; }
    //    Label labelStatus { get; set; }

    //    public ICommand EditCommand
    //    {
    //        get => (ICommand)GetValue(EditCommandProperty);
    //        set => SetValue(EditCommandProperty, value);
    //    }

    //    public static readonly BindableProperty EditCommandProperty =
    //        BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ActivityRow), null);

    //    public ProductRow()
    //    {            
            
    //    }

    //    protected override void BuildLeftGridContent(Grid leftGrid)
    //    {
    //        labelId = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black, FontSize = 10, BackgroundColor = Colors.Transparent, Padding = new Thickness(3), Margin = new Thickness(0) };
    //        labelCode = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black, FontSize = 10, BackgroundColor = Colors.Transparent, Padding = new Thickness(3), Margin = new Thickness(0) };
    //        labelName = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black, FontSize = 10, BackgroundColor = Colors.Transparent, Padding = new Thickness(3), Margin = new Thickness(0) };
    //        labelUnit = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
    //        labelBrand = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
    //        labelCategory = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };            
    //        labelStatus = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, FontSize = 10 };
            
    //        labelId.SetBinding(Label.TextProperty, new Binding(nameof(Item.id), source: Item));
    //        labelCode.SetBinding(Label.TextProperty, new Binding(nameof(Item.code), source: Item));
    //        labelName.SetBinding(Label.TextProperty, new Binding(nameof(Item.name), source: Item));
    //        labelUnit.SetBinding(Label.TextProperty, new Binding(nameof(Item.base_unit_count), source: Item));
    //        labelBrand.SetBinding(Label.TextProperty, new Binding(nameof(Item._product_brand_id), source: Item));
    //        labelCategory.SetBinding(Label.TextProperty, new Binding(nameof(Item._categ_id), source: Item));            
    //        labelStatus.SetBinding(Label.TextProperty, new Binding(nameof(Item.active), source: Item));

    //        var cellGrid = new Grid
    //        {
    //            HorizontalOptions = LayoutOptions.Fill,
    //            BackgroundColor = Colors.Transparent,
    //            Padding = new Thickness(0),
    //            ColumnSpacing = 0,
    //            RowSpacing = 0,
    //            RowDefinitions =
    //            {
    //                new RowDefinition { Height = GridLength.Star },
    //                new RowDefinition { Height = GridLength.Auto }
    //            },
    //            ColumnDefinitions =
    //            {
    //                new ColumnDefinition { Width = GridLength.Star },                    
    //            }
    //        };

    //        cellGrid.Children.Add(labelCode);
    //        cellGrid.Children.Add(labelName);
    //        Grid.SetRow(labelName, 1);

    //        AddCell(CreateCell(labelId), "left", 0, 0);
    //        AddCell(CreateCell(cellGrid), "left", 0, 2);
    //        AddCell(CreateCell(labelUnit), "left", 0, 3);
    //        AddCell(CreateCell(labelBrand), "left", 0, 4);
    //        AddCell(CreateCell(labelCategory), "left", 0, 5);
    //        AddCell(CreateCell(labelStatus), "left", 0, 6);
    //    }

    //    protected override void BuildToolGridContent(Grid toolGrid)
    //    {
    //        var buttonEdit = new Button
    //        {
    //            HeightRequest = 35,
    //            WidthRequest = 35,
    //            BackgroundColor = Colors.DodgerBlue,
    //            Text = "",
    //            TextColor = Colors.White,
    //            FontAttributes = FontAttributes.Bold,
    //            FontSize = 12,
    //            HorizontalOptions = LayoutOptions.Center,
    //            IsVisible = true,
    //            ImageSource = new FontImageSource
    //            {
    //                FontFamily = "FontAwesome5Solid",
    //                Color = Colors.White,
    //                Size = 15,
    //                FontAutoScalingEnabled = true,
    //                Glyph = "\uf303"
    //            },
    //            Padding = new Thickness(3),
    //            Margin = new Thickness(2),
    //        };

    //        buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(ActivityRow))));
    //        buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

    //        var buttonDelete = new Button
    //        {
    //            //Command = EditCommand,
    //            CommandParameter = "",
    //            HeightRequest = 35,
    //            WidthRequest = 35,
    //            BackgroundColor = Colors.OrangeRed,
    //            Text = "",
    //            TextColor = Colors.White,
    //            FontAttributes = FontAttributes.Bold,
    //            FontSize = 12,
    //            HorizontalOptions = LayoutOptions.Center,
    //            IsVisible = true,
    //            ImageSource = new FontImageSource
    //            {
    //                FontFamily = "FontAwesome5Solid",
    //                Color = Colors.White,
    //                Size = 15,
    //                FontAutoScalingEnabled = true,
    //                Glyph = "\uf2ed"
    //            },
    //            Padding = new Thickness(3),
    //            Margin = new Thickness(2),
    //        };

    //        var stackLayout = new StackLayout
    //        {
    //            Orientation = StackOrientation.Horizontal,
    //            Margin = new Thickness(0),
    //            BackgroundColor = Colors.Transparent
    //        };

    //        stackLayout.Children.Add(buttonEdit);
    //        stackLayout.Children.Add(buttonDelete);

    //        toolGrid.Children.Add(stackLayout);
    //        Grid.SetRow(stackLayout, 0);
    //        Grid.SetRowSpan(stackLayout, 2);
    //        Grid.SetColumn(stackLayout, 4);
    //    }
    //}
}
