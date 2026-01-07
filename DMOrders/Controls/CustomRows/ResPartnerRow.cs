using DMOrders.Controls.Base;
using DMSA.Models.Odoo.Native;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [Obsolete]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ResPartnerRow : RowAdvance<res_partner>
    {
        // BP para el comando de edición
        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ResPartnerRow));

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        // cache de vistas (se crean 1 sola vez)
        bool _built;
        Label _id, _vat, _name, _email, _phone, _visits, _city;
        Button _editBtn;

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            // Evita reconstrucciones
            if (_built)
                return;

            // ====== Layout: columnas fijas y planas (1 sola vez) ======
            leftGrid.RowDefinitions.Clear();
            leftGrid.ColumnDefinitions.Clear();

            // Ajusta anchos según tu diseño real
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0: Id
            //leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) }); // 1: separador
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 2: Name/VAT
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // 3: Email
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 4: Phone
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 5: Visits/Active
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 6: City

            // ====== Vistas (InputTransparent reduce hit-testing) ======
            _id = new Label { FontSize = 10, TextColor = Colors.Black, Padding = 3, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
            _name = new Label { FontSize = 13, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, InputTransparent = true };
            _vat = new Label { FontSize = 12, TextColor = Colors.Gray, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, InputTransparent = true };
            _email = new Label { FontSize = 12, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, InputTransparent = true };
            _phone = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkSlateGray, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, InputTransparent = true };
            _visits = new Label { FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Colors.OrangeRed, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, InputTransparent = true };
            _city = new Label { FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, InputTransparent = true };

            // ====== Bindings baratos a Item.* (source: this) ======
            _id.SetBinding(Label.TextProperty, new Binding("Item.id", source: this));
            _name.SetBinding(Label.TextProperty, new Binding("Item.name", source: this));
            _vat.SetBinding(Label.TextProperty, new Binding("Item.vat", source: this));
            _email.SetBinding(Label.TextProperty, new Binding("Item.email", source: this));
            _phone.SetBinding(Label.TextProperty, new Binding("Item.phone", source: this));
            _visits.SetBinding(Label.TextProperty, new Binding("Item.active", source: this, stringFormat: "{0}")); // ajusta si no es boolean
            _city.SetBinding(Label.TextProperty, new Binding("Item.city", source: this));

            // Bloque Name/VAT para dos líneas (ligero)
            var nameVat = new VerticalStackLayout
            {
                Spacing = 0,
                Children = { _name, _vat }
            };

            // ====== Posicionamiento (una sola vez) ======
            Grid.SetColumn(_id, 0); leftGrid.Children.Add(_id);
            // col 1 es separador visual (sin vista)
            Grid.SetColumn(nameVat, 1); leftGrid.Children.Add(nameVat);
            Grid.SetColumn(_email, 2); leftGrid.Children.Add(_email);
            Grid.SetColumn(_phone, 3); leftGrid.Children.Add(_phone);
            Grid.SetColumn(_visits, 4); leftGrid.Children.Add(_visits);
            Grid.SetColumn(_city, 5); leftGrid.Children.Add(_city);

            // (Opcional) fila con altura fija para acelerar medición
            this.HeightRequest = 72; // ajusta a tu diseño

            _built = true;
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            if (_editBtn != null)
                return;

            _editBtn = new Button
            {
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.DodgerBlue,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                Padding = 3,
                Margin = 2,
                // RECOMENDACIÓN: reutiliza un FontImageSource desde Resources si el icono es fijo
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf303"
                }
            };

            // Bindings sin FindAncestor (más rápido y estable)
            _editBtn.SetBinding(Button.CommandProperty, new Binding(nameof(EditCommand), source: this));
            _editBtn.SetBinding(Button.CommandParameterProperty, new Binding(nameof(Item), source: this));

            // ToolGrid del Row base tiene 1 fila/1 columna → sin SetColumn/RowSpan fantasmas
            toolGrid.Children.Add(_editBtn);
        }
    }
}
