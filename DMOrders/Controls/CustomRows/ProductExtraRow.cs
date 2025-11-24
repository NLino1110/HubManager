using DMOrders.Controls.Base;
using DMSA.Models.Odoo.Native;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ProductExtraRow : RowAdvance<product_product>
    {
        public static readonly BindableProperty ShowSelectButtonProperty =
            BindableProperty.Create(nameof(ShowSelectButton), typeof(bool), typeof(ProductExtraRow), false);

        public bool ShowSelectButton
        {
            get => (bool)GetValue(ShowSelectButtonProperty);
            set => SetValue(ShowSelectButtonProperty, value);
        }

        public static readonly BindableProperty ActionButtonProperty =
            BindableProperty.Create(nameof(ActionButton), typeof(ICommand), typeof(ProductExtraRow), null);

        public ICommand ActionButton
        {
            get => (ICommand)GetValue(ActionButtonProperty);
            set => SetValue(ActionButtonProperty, value);
        }

        // ---------- cache de vistas (se crean una sola vez) ----------
        bool _built;
        Label _nameLabel, 
            _codeLabel, 
            _brand,
            _brandLine,
            _state,
            _unitLabel;
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
                    new RowDefinition { Height = GridLength.Auto }, // name / unit /stock / price / priceBase / (ori/com)
                    new RowDefinition { Height = GridLength.Star }, // code                    
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },                    
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
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
            _brand = new Label { FontSize = 12, TextColor = Colors.Gray, InputTransparent = true };
            _brandLine = new Label { FontSize = 12, TextColor = Colors.Gray, InputTransparent = true };
            _state = new Label { FontSize = 12, TextColor = Colors.Gray, InputTransparent = true };            
            _unitLabel = new Label { FontSize = 12, TextColor = Colors.DarkGreen, InputTransparent = true, VerticalTextAlignment = TextAlignment.Center };
            _state = new Label { FontSize = 12, TextColor = Colors.DarkGreen, InputTransparent = true, VerticalTextAlignment = TextAlignment.Center };

            // Colocar en la grilla
            infoGrid.Add(_nameLabel, 0, 0);
            infoGrid.Add(_codeLabel, 0, 1);
            infoGrid.Add(_unitLabel, 1, 0);
            Grid.SetRowSpan(_unitLabel, 2);

            infoGrid.Add(_brand, 2, 0);
            Grid.SetRowSpan(_brand, 2);
            infoGrid.Add(_brandLine, 3, 0);
            Grid.SetRowSpan(_brandLine, 2);
            infoGrid.Add(_state, 4, 0);
            Grid.SetRowSpan(_state, 2);

            // Enlaza labels a las propiedades del Item (así no tienes que “repintar” manual)
            _nameLabel.SetBinding(Label.TextProperty, new Binding("Item.name", source: this));
            _codeLabel.SetBinding(Label.TextProperty, new Binding("Item.default_code", source: this, stringFormat: "Código: {0}"));
            _brand.SetBinding(Label.TextProperty, new Binding("Item.list_price", source: this, stringFormat: "Precio: {0:C4}"));
            _brandLine.SetBinding(Label.TextProperty, new Binding("Item.qty_available", source: this, stringFormat: "Stock: {0}"));
            _unitLabel.SetBinding(Label.TextProperty, new Binding("Item._uom_id", source: this, stringFormat: "Unidad: {0}"));

            // Contenedor (dos filas: info + botón)
            var container = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    //new RowDefinition { Height = GridLength.Auto }
                }
            };

            container.Add(infoGrid, 0, 0);
                        
            _btnSelect = new Button
            {
                HeightRequest = 35,
                BackgroundColor = Colors.DodgerBlue,
                Text = "1",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                HorizontalOptions = LayoutOptions.Fill,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf067"
                },
                Padding = new Thickness(10,0,10,0)
            };

            // 🔗 ENLACES (se actualizan cuando el DataTemplate resuelve los bindings)
            _btnSelect.SetBinding(Button.CommandProperty, new Binding(nameof(ActionButton), source: this));
            _btnSelect.SetBinding(Button.CommandParameterProperty, new Binding(nameof(Item), source: this));
            _btnSelect.SetBinding(IsVisibleProperty, new Binding(nameof(ShowSelectButton), source: this));

            container.Add(_btnSelect, 1, 0);            

            // Agrega una vez al grid izquierdo
            leftGrid.Children.Add(container);
            _built = true;
        }
    }
}
