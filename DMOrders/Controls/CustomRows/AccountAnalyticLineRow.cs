using DMOrders.Controls.Base;
using DMOrders.Converters;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Collections;
using System.Reflection;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class AccountAnalyticLineRow : RowAdvance<AccountAnalyticLine>
    {
        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(AccountAnalyticLineRow));

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty DeleteCommandProperty =
            BindableProperty.Create(
                nameof(DeleteCommand),
                typeof(ICommand),
                typeof(AccountAnalyticLineRow),
                null,
                propertyChanged: OnDeleteCommandChanged);

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        static void OnDeleteCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is AccountAnalyticLineRow row && row._btnDelete != null)
            {
                // actualizar comando directo en el botón para asegurar comportamiento inmediato
                row._btnDelete.Command = newValue as ICommand;
                row._btnDelete.CommandParameter = row.Item;
            }
        }

        bool _built;
        Label _labelId, _labelCompany, _labelReason, _labelPartner, _labelStartDate, _labelEndDate, _labelStandby;
        Button _btnEdit, _btnDelete;

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            if (_built) return;

            leftGrid.Padding = new Thickness(0);
            leftGrid.Margin = new Thickness(0);
            leftGrid.ColumnSpacing = 4; // reducido para acercar Empresa y Motivo
            leftGrid.HorizontalOptions = LayoutOptions.Fill;
            leftGrid.VerticalOptions = LayoutOptions.Center;

            leftGrid.RowDefinitions.Clear();
            leftGrid.ColumnDefinitions.Clear();

            // Columnas: id | sep | empresa | motivo | cliente | h.ini | h.fin | standby | herramientas
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(42) });                // 0: id
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });                 // 1: separador (ligeramente reducido)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) }); // 2: empresa (ligeramente reducido)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // 3: motivo (reducido)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // 4: cliente (ampliado)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });                // 5: hora inicio (fijo)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });                // 6: hora fin (fijo)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });                // 7: standby (fijo)
            leftGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });                // 8: herramientas (fijo ancho ajustado)

            leftGrid.SetBinding(BindingContextProperty, new Binding(nameof(Item), source: this));

            _labelId = new Label
            {
                FontSize = 12,
                TextColor = Colors.Black,
                Padding = new Thickness(2, 0),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                InputTransparent = true
            };

            _labelCompany = new Label
            {
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                HorizontalTextAlignment = TextAlignment.Start
            };

            _labelReason = new Label
            {
                FontSize = 12,
                TextColor = Colors.Gray,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                HorizontalTextAlignment = TextAlignment.Start
            };

            _labelPartner = new Label
            {
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                HorizontalTextAlignment = TextAlignment.Start
            };

            _labelStartDate = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Gray,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                WidthRequest = 72,
                HorizontalOptions = LayoutOptions.Center
            };

            _labelEndDate = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Gray,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                WidthRequest = 72,
                HorizontalOptions = LayoutOptions.Center
            };

            _labelStandby = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Green,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                WidthRequest = 72,
                HorizontalOptions = LayoutOptions.Center
            };

            _labelId.SetBinding(Label.TextProperty, new Binding("id"));
            _labelCompany.SetBinding(Label.TextProperty, new Binding("res_company_display"));
            _labelReason.SetBinding(Label.TextProperty, new Binding("motivo_display"));
            _labelPartner.SetBinding(Label.TextProperty, new Binding("res_partner_display"));
            _labelStartDate.SetBinding(Label.TextProperty, new Binding("hour_start", converter: new HourDecimalToTimeSpanConverter(), stringFormat: "{0:hh\\:mm}"));
            _labelEndDate.SetBinding(Label.TextProperty, new Binding("hour_end", converter: new HourDecimalToTimeSpanConverter(), stringFormat: "{0:hh\\:mm}"));
            _labelStandby.SetBinding(Label.TextProperty, new Binding("duration", converter: new HourDecimalToTimeSpanConverter(), stringFormat: "{0:hh\\:mm}"));

            // Añadir controles en columnas exactas (coincidir con Details.xaml)
            Grid.SetColumn(_labelId, 0); leftGrid.Children.Add(_labelId);
            Grid.SetColumn(_labelCompany, 2); leftGrid.Children.Add(_labelCompany);
            Grid.SetColumn(_labelReason, 3); leftGrid.Children.Add(_labelReason);

            // Cliente debe estar en la columna del header "CLIENTE" (ej.: columna 4)
            Grid.SetColumn(_labelPartner, 4); leftGrid.Children.Add(_labelPartner);

            // Hora inicio (H. INI) en la columna del header "H. INI" (ej.: columna 5)
            Grid.SetColumn(_labelStartDate, 5); leftGrid.Children.Add(_labelStartDate);

            // Hora fin (H. FIN) en la columna del header "H. FIN" (ej.: columna 6)
            Grid.SetColumn(_labelEndDate, 6); leftGrid.Children.Add(_labelEndDate);

            // Standby en la columna del header "STANDBY" (ej.: columna 7)
            Grid.SetColumn(_labelStandby, 7); leftGrid.Children.Add(_labelStandby);

            // BOTONES: reducir tamaño para caber en la columna y evitar solapamiento
            _btnEdit = new Button
            {
                HeightRequest = 28,
                WidthRequest = 28,
                BackgroundColor = Colors.DodgerBlue,
                Text = "",
                TextColor = Colors.White,
                CornerRadius = 4,
                Padding = 0,
                Margin = new Thickness(2, 0),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Glyph = "\uf303",
                    Color = Colors.White,
                    Size = 12
                },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            _btnEdit.SetBinding(Button.CommandProperty, new Binding(nameof(EditCommand), source: this));
            _btnEdit.SetBinding(Button.CommandParameterProperty, new Binding(nameof(Item), source: this));

            _btnDelete = new Button
            {
                HeightRequest = 28,
                WidthRequest = 28,
                BackgroundColor = Colors.OrangeRed,
                Text = "",
                TextColor = Colors.White,
                CornerRadius = 4,
                Padding = 0,
                Margin = new Thickness(2, 0),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Glyph = "\uf2ed",
                    Color = Colors.White,
                    Size = 12
                },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            // Mantener enlace a la propiedad DeleteCommand del control (si el consumidor lo establece)
            _btnDelete.SetBinding(Button.CommandProperty, new Binding(nameof(DeleteCommand), source: this));
            _btnDelete.SetBinding(Button.CommandParameterProperty, new Binding(nameof(Item), source: this));

            // Manejo adicional: si no hay DeleteCommand asignado al control, buscarlo en la página o en su BindingContext
            _btnDelete.Clicked += (s, e) =>
            {
                // Si el botón ya tiene un Command asignado, no ejecutar manualmente para evitar duplicados.
                if (_btnDelete.Command != null)
                {
                    Debug.WriteLine("[AccountAnalyticLineRow] Button.Command present; skipping manual execution to avoid duplicate.");
                    return;
                }

                ICommand cmd = DeleteCommand;
                object param = Item;

                Debug.WriteLine($"[AccountAnalyticLineRow] Delete clicked. Button.Command is {(_btnDelete.Command == null ? "null" : "set")}, Control.DeleteCommand is {(DeleteCommand == null ? "null" : "set")}. Item id={(Item?.id.ToString() ?? "<null>")}");

                if (cmd == null)
                {
                    // buscar ancestro ContentPage recorriendo padres
                    Element parent = this;
                    int depth = 0;
                    while (parent != null && !(parent is ContentPage) && depth < 30)
                    {
                        parent = parent.Parent;
                        depth++;
                    }

                    var page = parent as ContentPage;
                    Debug.WriteLine($"[AccountAnalyticLineRow] Ancestor ContentPage found: {(page != null)} at depth {depth}");

                    if (page is not null)
                    {
                        // intentar propiedad pública DeleteCommand en la página
                        var pageProp = page.GetType().GetProperty("DeleteCommand");
                        if (pageProp != null)
                        {
                            cmd = pageProp.GetValue(page) as ICommand;
                            Debug.WriteLine($"[AccountAnalyticLineRow] Page.DeleteCommand found: {(cmd != null)}");
                        }

                        // si no encontrado en la página, intentar en BindingContext (ViewModel)
                        if (cmd == null && page.BindingContext != null)
                        {
                            var vmProp = page.BindingContext.GetType().GetProperty("DeleteCommand");
                            if (vmProp != null)
                            {
                                cmd = vmProp.GetValue(page.BindingContext) as ICommand;
                                Debug.WriteLine($"[AccountAnalyticLineRow] VM.DeleteCommand found on page.BindingContext: {(cmd != null)}");
                            }
                        }

                        // Si aún no hay comando, intentar eliminar directamente de Activities del BindingContext de la página
                        if (cmd == null)
                        {
                            TryRemoveItemFromBindingContextActivities(page, param);
                            return; // already removed in-memory -> stop
                        }
                    }
                }

                if (cmd != null)
                {
                    try
                    {
                        if (cmd.CanExecute(param))
                        {
                            cmd.Execute(param);
                            Debug.WriteLine("[AccountAnalyticLineRow] DeleteCommand executed.");
                        }
                        else
                        {
                            Debug.WriteLine("[AccountAnalyticLineRow] DeleteCommand.CanExecute returned false.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AccountAnalyticLineRow] Exception executing DeleteCommand: {ex.Message}");
                    }
                }
                else
                {
                    Debug.WriteLine("[AccountAnalyticLineRow] No DeleteCommand found to execute.");
                }
            };

            var tools = new HorizontalStackLayout
            {
                Spacing = 4,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                WidthRequest = 64,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { _btnEdit, _btnDelete }
            };

            Grid.SetColumn(tools, 8);
            leftGrid.Children.Add(tools);

            this.HeightRequest = 58;
            _built = true;
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            // Intencionalmente vacío: las herramientas se colocan en leftGrid.
            return;
        }

        void TryRemoveItemFromBindingContextActivities(ContentPage page, object param)
        {
            try
            {
                if (page?.BindingContext == null)
                {
                    Debug.WriteLine("[AccountAnalyticLineRow] page.BindingContext is null, cannot remove");
                    return;
                }

                var vm = page.BindingContext;
                var activitiesProp = vm.GetType().GetProperty("Activities", BindingFlags.Public | BindingFlags.Instance);
                if (activitiesProp == null)
                {
                    Debug.WriteLine("[AccountAnalyticLineRow] ViewModel has no 'Activities' property");
                    return;
                }

                var activitiesObj = activitiesProp.GetValue(vm);
                if (activitiesObj is not IList list)
                {
                    Debug.WriteLine("[AccountAnalyticLineRow] 'Activities' is not an IList");
                    return;
                }

                if (param != null && list.Contains(param))
                {
                    list.Remove(param);
                    Debug.WriteLine("[AccountAnalyticLineRow] Removed item by reference from Activities");
                    return;
                }

                // intentar eliminación por propiedad 'id'
                var idProp = param?.GetType().GetProperty("id");
                if (idProp != null)
                {
                    var idVal = idProp.GetValue(param);
                    object toRemove = null;
                    foreach (var it in list)
                    {
                        var itIdProp = it?.GetType().GetProperty("id");
                        if (itIdProp == null) continue;
                        var itId = itIdProp.GetValue(it);
                        if (idVal != null && idVal.Equals(itId))
                        {
                            toRemove = it;
                            break;
                        }
                    }
                    if (toRemove != null)
                    {
                        list.Remove(toRemove);
                        Debug.WriteLine("[AccountAnalyticLineRow] Removed item by id from Activities");
                        return;
                    }
                }

                Debug.WriteLine("[AccountAnalyticLineRow] No matching item found to remove");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccountAnalyticLineRow] Error removing item from Activities: {ex.Message}");
            }
        }
    }
}
