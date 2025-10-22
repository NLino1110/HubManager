using DMOrders.Controls.Base;
using DMSA.Models.Odoo.Native;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public class MarcaRow : RowAdvance<product_marca>
    {
        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(MarcaRow), null);

        public MarcaRow()
        {
            
        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            base.BuildLeftGridContent(leftGrid);

            if (Item == null)
            {
                return;
            }

            var nameLabel = new Label
            {
                Text = Item.name,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black
            };

            var codeLabel = new Label
            {
                Text = $"Código: {Item.id}",
                FontSize = 12,
                TextColor = Colors.Green
            };

            var contentStack = new StackLayout
            {
                BackgroundColor = Colors.Transparent,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
                Padding = 4,
                Children = { nameLabel, codeLabel }
            };

            Button btnSelect = new Button
            {
                Command = PopupSelectMarca.CommandSelectListItem,
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.DodgerBlue,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,                
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf058"
                },
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            //btnSelect.SetBinding(Button.CommandProperty, new Binding()
            //{
            //    Path = "CommandSelectListItem111",
            //    Source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(PopupSelectBrand))
            //});

            btnSelect.SetBinding(Button.CommandParameterProperty, new Binding("."));
            contentStack.Children.Add(btnSelect);

            //var cell = CreateCell(contentStack, padding: new Thickness(4), backgroundColor: Colors.Transparent);
            //AddCell(cell, region: "left", row: 0, column: 0);

            leftGrid.Children.Add(contentStack);
        }
    }
}
