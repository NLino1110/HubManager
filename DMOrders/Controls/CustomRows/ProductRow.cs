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
            //BackgroundColor = Colors.AliceBlue;
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
}
