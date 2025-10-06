using DMOrders.Controls.Base;
using DMSA.Models.Odoo.Native;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ProductRow : RowAdvance<product_product>
    {
        public static readonly BindableProperty ShowSelectButtonProperty =
            BindableProperty.Create(nameof(ShowSelectButton), typeof(bool), typeof(ProductRow), false);

        public bool ShowSelectButton
        {
            get => (bool)GetValue(ShowSelectButtonProperty);
            set => SetValue(ShowSelectButtonProperty, value);
        }

        public static readonly BindableProperty ActionButtonProperty =
            BindableProperty.Create(nameof(ActionButton), typeof(ICommand), typeof(ProductRow), null);

        public ICommand ActionButton
        {
            get => (ICommand)GetValue(ActionButtonProperty);
            set => SetValue(ActionButtonProperty, value);
        }

        // ---------- cache de vistas (se crean una sola vez) ----------
        bool _built;
        Label _nameLabel, _codeLabel, _priceLabel, _priceBaseLabel, _stockLabel, _unitLabel;
        Button _btnSelect;

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            // ¡No llames a base.BuildLeftGridContent! (esa limpia el grid y provoca rebuild)
            if (_built)
                return;

            // Layout compacto: menos vistas y sin anidar stacks innecesarios
            var infoGrid = new Grid
            {
                Padding = new Thickness(4),
                RowSpacing = 2,
                ColumnSpacing = 12,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // name
                    new RowDefinition { Height = GridLength.Auto }, // code
                    new RowDefinition { Height = GridLength.Auto }, // price / stock
                    new RowDefinition { Height = GridLength.Auto }, // priceBase / unit
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };

            _nameLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Colors.Black,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2,
                MinimumHeightRequest = 40,
                InputTransparent = true
            };
            _codeLabel = new Label { FontSize = 12, TextColor = Colors.Gray, InputTransparent = true };
            _priceLabel = new Label { FontSize = 12, TextColor = Colors.DarkGreen, InputTransparent = true };
            _priceBaseLabel = new Label { FontSize = 12, TextColor = Colors.DarkGreen, InputTransparent = true };
            _stockLabel = new Label { FontSize = 12, TextColor = Colors.DarkGreen, HorizontalTextAlignment = TextAlignment.End, InputTransparent = true };
            _unitLabel = new Label { FontSize = 12, TextColor = Colors.DarkGreen, HorizontalTextAlignment = TextAlignment.End, InputTransparent = true };

            // Colocar en la grilla
            infoGrid.Add(_nameLabel, 0, 0);
            Grid.SetColumnSpan(_nameLabel, 2);

            infoGrid.Add(_codeLabel, 0, 1);
            Grid.SetColumnSpan(_codeLabel, 2);

            infoGrid.Add(_priceLabel, 0, 2);
            infoGrid.Add(_stockLabel, 1, 2);

            infoGrid.Add(_priceBaseLabel, 0, 3);
            infoGrid.Add(_unitLabel, 1, 3);

            

            // Enlaza labels a las propiedades del Item (así no tienes que “repintar” manual)
            _nameLabel.SetBinding(Label.TextProperty, new Binding("Item.name", source: this));
            _codeLabel.SetBinding(Label.TextProperty, new Binding("Item.default_code", source: this, stringFormat: "Código: {0}"));
            _priceLabel.SetBinding(Label.TextProperty, new Binding("Item.list_price", source: this, stringFormat: "Precio: {0:C}"));
            _priceBaseLabel.SetBinding(Label.TextProperty, new Binding("Item.list_price", source: this, stringFormat: "PVP Base: {0:C}"));
            _stockLabel.SetBinding(Label.TextProperty, new Binding("Item.qty_available", source: this, stringFormat: "Stock: {0}"));
            _unitLabel.SetBinding(Label.TextProperty, new Binding("Item._uom_id", source: this, stringFormat: "Unidad: {0}"));

            // Contenedor (dos filas: info + botón)
            var container = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            container.Add(infoGrid, 0, 0);
                        
            _btnSelect = new Button
            {
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
                Padding = new Thickness(3)
            };

            // 🔗 ENLACES (se actualizan cuando el DataTemplate resuelve los bindings)
            _btnSelect.SetBinding(Button.CommandProperty, new Binding(nameof(ActionButton), source: this));
            _btnSelect.SetBinding(Button.CommandParameterProperty, new Binding(nameof(Item), source: this));
            _btnSelect.SetBinding(IsVisibleProperty, new Binding(nameof(ShowSelectButton), source: this));

            container.Add(_btnSelect, 0, 1);            

            // Agrega una vez al grid izquierdo
            leftGrid.Children.Add(container);
            _built = true;
        }
    }
}
