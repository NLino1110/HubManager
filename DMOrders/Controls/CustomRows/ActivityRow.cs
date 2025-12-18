using CommunityToolkit.Maui.Behaviors;
using DMOrders.Controls.Base;
using DMOrders.Converters;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ActivityRow : RowAdvance<ProjectTask>
    {
        private bool _built;

        Label labelId { get; set; }
        Label labelName { get; set; }
        Label labelSellerName { get; set; }
        Label labelPlanningDate { get; set; }
        Label labelWriteDate { get; set; }
        Label labelState { get; set; }

        Button buttonEdit;
        Button buttonDelete;

        // Notificador para detectar cambios en Item
        INotifyPropertyChanged? _itemNotifier;

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ActivityRow), null);

        public ActivityRow()
        {

        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            if (_built) return;

            // Fijar altura de fila para que todo tenga el mismo alto que el botón
            this.HeightRequest = 28;

            // Ajuste: primera columna (ID) fija y última columna para herramientas fija
            LeftGrid.ColumnDefinitions = new ColumnDefinitionCollection()
            {
                new ColumnDefinition { Width = new GridLength(42) }, // ID fijo para evitar desplazar columnas
                new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 28 }, // fijado a 28, coincide con DataGrid.xaml
            };

            // ID centrado
            labelId = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.Black,
                FontSize = 12,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.NoWrap,
                MaxLines = 1,
                HeightRequest = 28
            };

            // Vendedor: alineado centrado en la celda, truncado en una línea (este es el que se muestra en la columna VENDEDOR)
            labelSellerName = new Label
            {
                HorizontalOptions = LayoutOptions.Center,                // centrado horizontalmente en su celda
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.None,
                FontSize = 11,
                BackgroundColor = Colors.Transparent,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                HorizontalTextAlignment = TextAlignment.Center,         // texto centrado
                Margin = new Thickness(6, 0, 6, 0),
                HeightRequest = 28
            };

            // Nombre de la actividad (si se necesita más adelante) - no se añade a la columna VENDEDOR
            labelName = new Label
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.None,
                FontSize = 12,
                BackgroundColor = Colors.Transparent,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                HorizontalTextAlignment = TextAlignment.Start,
                Margin = new Thickness(6, 0, 6, 0),
                HeightRequest = 28,
                IsVisible = false
            };

            // Fechas y estado: sin negrita
            labelPlanningDate = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.None, TextColor = Colors.Black, FontSize = 12, LineBreakMode = LineBreakMode.NoWrap, MaxLines = 1, HeightRequest = 28 };
            labelWriteDate = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.None, TextColor = Colors.Black, FontSize = 12, LineBreakMode = LineBreakMode.NoWrap, MaxLines = 1, HeightRequest = 28 };
            labelState = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.None, TextColor = Colors.Black, FontSize = 12, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, HeightRequest = 28 };

            // Bindings: rutas simples; BindingContext de las labels se asigna en UpdateItemSubscription()
            labelId.SetBinding(Label.TextProperty, new Binding("id"));
            labelSellerName.SetBinding(Label.TextProperty, new Binding("display_username"));
            labelName.SetBinding(Label.TextProperty, new Binding("name"));
            labelPlanningDate.SetBinding(Label.TextProperty, new Binding("date_assign"));
            labelWriteDate.SetBinding(Label.TextProperty, new Binding("date_synchronized", converter: new DateToDashConverter()));

            // Añadir celdas en el orden y columna correcta (VENDEDOR en la segunda columna).
            AddCell(CreateCell(labelId), "left", 0, 0);
            AddCell(CreateCell(labelSellerName), "left", 0, 1);
            AddCell(CreateCell(labelPlanningDate), "left", 0, 2);
            AddCell(CreateCell(labelWriteDate), "left", 0, 3);
            AddCell(CreateCell(labelState), "left", 0, 4);

            _built = true;
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            if (buttonEdit != null) return;

            // Botón pequeño y contenedor ajustado a 28px para mantener alineación
            buttonEdit = new Button
            {
                HeightRequest = 28,
                WidthRequest = 28,
                BackgroundColor = Colors.DodgerBlue,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.None,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = true,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                CornerRadius = 4,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 14,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf303"
                }
            };

            // Enlazar al EditCommand del ancestro DataGrid para que reciba el comando correcto
            buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(DMOrders.Pages.Fragments.Activities.DataGrid))));
            // Pasamos el modelo (Item) como parámetro
            buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            buttonDelete = new Button
            {
                Command = EditCommand,
                CommandParameter = "",
                HeightRequest = 28,
                WidthRequest = 28,
                BackgroundColor = Colors.OrangeRed,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.None,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                CornerRadius = 4,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 14,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf2ed"
                }
            };

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                BackgroundColor = Colors.Transparent,
                WidthRequest = 28,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 0
            };

            stackLayout.Children.Add(buttonEdit);
            stackLayout.Children.Add(buttonDelete);

            toolGrid.Children.Add(stackLayout);
            Grid.SetRow(stackLayout, 0);
            Grid.SetRowSpan(stackLayout, 2);
            Grid.SetColumn(stackLayout, 5); // columna fija pequeña en DataGrid.xaml

            // Inicializar suscripción y visibilidad
            UpdateItemSubscription();
            UpdateEditButtonVisibility();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            UpdateItemSubscription();
            UpdateEditButtonVisibility();

            // Actualizar estado visual
            UpdateStateDisplay();
        }

        void UpdateItemSubscription()
        {
            if (_itemNotifier != null)
            {
                try
                {
                    _itemNotifier.PropertyChanged -= OnItemPropertyChanged;
                }
                catch { }
                _itemNotifier = null;
            }

            // Asegurar que las labels se enlazan al Item actual (BindingContext)
            labelId.BindingContext = Item;
            labelName.BindingContext = Item;
            labelSellerName.BindingContext = Item;
            labelPlanningDate.BindingContext = Item;
            labelWriteDate.BindingContext = Item;
            labelState.BindingContext = Item;

            if (Item is INotifyPropertyChanged npc)
            {
                _itemNotifier = npc;
                _itemNotifier.PropertyChanged += OnItemPropertyChanged;
            }
        }

        void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // cuando cambian estas propiedades, actualizamos estado y visibilidad
            if (string.IsNullOrEmpty(e?.PropertyName) || e.PropertyName.Equals("is_synchronized", System.StringComparison.OrdinalIgnoreCase) || e.PropertyName.Equals("state", System.StringComparison.OrdinalIgnoreCase) || e.PropertyName.Equals("state_view", System.StringComparison.OrdinalIgnoreCase))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateEditButtonVisibility();
                    UpdateStateDisplay();
                });
            }
        }

        void UpdateEditButtonVisibility()
        {
            if (buttonEdit == null) return;

            bool visible = true;

            if (Item != null)
            {
                try
                {
                    var prop = Item.GetType().GetProperty("is_synchronized", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        var val = prop.GetValue(Item);
                        if (val == null)
                        {
                            visible = true; // sin valor => permitir editar
                        }
                        else if (val is int i)
                        {
                            visible = i == 0; // 0 => editable, distinto a 0 => sincronizado => ocultar
                        }
                        else if (val is long l)
                        {
                            visible = l == 0;
                        }
                        else if (val is bool b)
                        {
                            // si se guarda como bool: true significa sincronizado => ocultar
                            visible = !b;
                        }
                        else if (int.TryParse(val.ToString(), out int parsed))
                        {
                            visible = parsed == 0;
                        }
                        else
                        {
                            // fallback: mantener visible
                            visible = true;
                        }
                    }
                }
                catch
                {
                    visible = true;
                }
            }

            buttonEdit.IsVisible = visible;
        }

        // Nueva función que calcula y aplica texto + color del estado
        void UpdateStateDisplay()
        {
            try
            {
                if (labelState == null) return;
                if (Item == null)
                {
                    labelState.Text = string.Empty;
                    labelState.TextColor = Colors.DarkGray;
                    return;
                }

                string stateView = null;
                string rawState = null;
                bool? isSynchronized = null;

                if (Item is ProjectTask pt)
                {
                    stateView = pt.state_view;
                    rawState = pt.state;
                    isSynchronized = pt.is_synchronized;
                }
                else
                {
                    var t = Item.GetType();
                    var pStateView = t.GetProperty("state_view", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var pState = t.GetProperty("state", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var pIsSync = t.GetProperty("is_synchronized", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (pStateView != null) stateView = pStateView.GetValue(Item)?.ToString();
                    if (pState != null) rawState = pState.GetValue(Item)?.ToString();
                    if (pIsSync != null)
                    {
                        var v = pIsSync.GetValue(Item);
                        if (v is bool b) isSynchronized = b;
                        else if (v is int i) isSynchronized = i != 0;
                        else if (v is long l) isSynchronized = l != 0;
                    }
                }

                // If both state_view and rawState are empty, fall back to isSynchronized if available
                if (string.IsNullOrWhiteSpace(stateView) && string.IsNullOrWhiteSpace(rawState))
                {
                    if (isSynchronized.HasValue)
                    {
                        stateView = isSynchronized.Value ? "SINCRONIZADO" : "ACTIVO";
                    }
                }

                if (string.IsNullOrWhiteSpace(stateView) && !string.IsNullOrWhiteSpace(rawState))
                {
                    var rs = rawState.Trim().ToLowerInvariant();
                    if (rs == "draft")
                        stateView = (isSynchronized == true) ? "SINCRONIZADO" : "ACTIVO";
                    else if (rs == "sent")
                        stateView = "SINCRONIZADO";
                    else if (rs == "done")
                        stateView = "TERMINADO";
                    else if (rs == "cancel")
                        stateView = "CANCELADO";
                    else
                        stateView = rawState;
                }

                var converter = new StateToLabelConverter();
                var display = converter.Convert(stateView ?? string.Empty, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString() ?? string.Empty;

                labelState.Text = display;

                switch (display.ToUpperInvariant())
                {
                    case "ACTIVO":
                        labelState.TextColor = Colors.Green;
                        break;
                    case "SINCRONIZADA":
                    case "SINCRONIZADO":
                        labelState.TextColor = Colors.DodgerBlue;
                        break;
                    default:
                        labelState.TextColor = Colors.DarkGray;
                        break;
                }

                if (string.IsNullOrWhiteSpace(display))
                {
                    Debug.WriteLine($"ActivityRow: estado vacío para item id={(Item.GetType().GetProperty("id")?.GetValue(Item) ?? "-")}, rawState={rawState}, isSynchronized={isSynchronized}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateStateDisplay error: {ex.Message}");
            }
        }
    }
}
