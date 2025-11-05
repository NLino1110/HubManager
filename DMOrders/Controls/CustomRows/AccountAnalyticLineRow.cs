using DMOrders.Controls.Base;
using DMOrders.Converters;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{    
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class AccountAnalyticLineRow : RowAdvance<AccountAnalyticLine>
    {
        // (Opcional pero útil) Command para el botón de herramientas
        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(AccountAnalyticLine));

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        // --- cache / flag ---
        bool _built;
        Label _labelId, _labelCompany, _labelReason, _labelPartner, _labelStartDate, _labelEndDate, _labelStandby;
        Button _btnEdit, _btnDelete;

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            // ⚡ evita reconstrucciones innecesarias del árbol visual
            if (_built) return;
            leftGrid.ColumnSpacing = 10; 
            leftGrid.RowDefinitions.Clear();
            leftGrid.ColumnDefinitions.Clear();

            // Columnas coherentes con cómo colocas los controles (0..6), con un separador en 1
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });     // 0: id
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });   // 1: separador
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });     // 2: company/reason (bloque)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });     // 3: partner
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });     // 4: start
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });     // 5: end
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });     // 6: standby

            // Hereda Item como BC del grid (bindings simples abajo)
            leftGrid.SetBinding(BindingContextProperty, new Binding(nameof(Item), source: this));

            // Labels livianos (sin hit-testing) y con truncado
            _labelId = new Label { FontSize = 10, TextColor = Colors.Black, Padding = 3, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
            _labelCompany = new Label { FontSize = 13, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
            _labelReason = new Label { FontSize = 12, TextColor = Colors.Gray, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
            _labelPartner = new Label { FontSize = 12, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
            _labelStartDate = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.Gray, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
            _labelEndDate = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.Gray, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
            _labelStandby = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };

            // Bindings simples (ajusta los nombres si tu modelo difiere)
            _labelId.SetBinding(Label.TextProperty, new Binding("id"));
            _labelCompany.SetBinding(Label.TextProperty, new Binding("res_company_display"));
            _labelReason.SetBinding(Label.TextProperty, new Binding("motivo_display")); // ← si tu modelo no tiene esto, deja "res_company_display"
            _labelPartner.SetBinding(Label.TextProperty, new Binding("res_partner_display"));
            _labelStartDate.SetBinding(Label.TextProperty, new Binding("hour_start", converter: new HourDecimalToTimeSpanConverter(), stringFormat: "{0:HH\\:mm}"));
            _labelEndDate.SetBinding(Label.TextProperty, new Binding("hour_end", converter: new HourDecimalToTimeSpanConverter(), stringFormat: "{0:HH\\:mm}"));
            _labelStandby.SetBinding(Label.TextProperty, new Binding("duration", converter: new HourDecimalToTimeSpanConverter(), stringFormat: "{0:hh\\:mm}"));

            // Bloque de 2 líneas: company / reason
            var companyReason = new VerticalStackLayout
            {
                Spacing = 0,
                Children = { _labelCompany, _labelReason },
                VerticalOptions = LayoutOptions.Center
            };

            // Colocar en columnas (una sola vez)
            Grid.SetColumn(_labelId, 0); leftGrid.Children.Add(_labelId);
            Grid.SetColumn(companyReason, 2); leftGrid.Children.Add(companyReason);
            Grid.SetColumn(_labelPartner, 3); leftGrid.Children.Add(_labelPartner);
            Grid.SetColumn(_labelStartDate, 4); leftGrid.Children.Add(_labelStartDate);
            Grid.SetColumn(_labelEndDate, 5); leftGrid.Children.Add(_labelEndDate);
            Grid.SetColumn(_labelStandby, 6); leftGrid.Children.Add(_labelStandby);

            // Altura fija opcional para acelerar medición con MeasureFirstItem
            this.HeightRequest = 56; // ajusta a tu diseño

            _built = true;
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            if (_btnEdit != null) return;

            _btnEdit = new Button
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
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf303"
                }
            };
            // Bindings sin FindAncestor (más rápidos/robustos)
            _btnEdit.SetBinding(Button.CommandProperty, new Binding(nameof(EditCommand), source: this));
            _btnEdit.SetBinding(Button.CommandParameterProperty, new Binding(nameof(Item), source: this));

            _btnDelete = new Button
            {
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.OrangeRed,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                Padding = 3,
                Margin = 2,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf2ed"
                }
            };
            // Si más adelante expones DeleteCommand, cámbialo aquí:
            //_btnDelete.SetBinding(Button.CommandProperty, new Binding(nameof(DeleteCommand), source: this));

            var tools = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Colors.Transparent,
                Children = { _btnEdit, _btnDelete }
            };

            // ToolGrid suele ser 1x1 en tu base; no forces SetColumn/RowSpan fuera de rango
            toolGrid.Children.Add(tools);
        }
    }
}
