using BeebTech.Controls.UI;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tareas;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using InputKit.Shared.Validations;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DMOrders.Controls
{
    [Obsolete("Se esta migrando a Popup.AccountAnalyticLine")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class PopupAccountAnalyticLine : Popup<AccountAnalyticLine>, INotifyPropertyChanged
    {

        public ProjectTask projectTask { get; set; }

        public AccountAnalyticLine analyticLine {  get; set; }

        private Picker _pickerPlanningSlot;

        private DropdownField _pickerPlanningReason;
        private DropdownField _pickerCompany;
        private TimePicker _timePickerStart;
        private TimePicker _timePickerEnd;

        
        //private Picker _pickerCompany;
        private TextField _inputResPartner;
        private TextField _inputReview;
        
        //private Picker _pickerBank;
        //private Picker _pickerCurrency;
                
        private Microsoft.Maui.Controls.Switch _switchAllowOutPayment;

        private StackLayout _stackLayoutTop;
        private Label _labelTitle;
        private StackLayout _stackLayoutBottom;

        private Button _btnSave;
        private Button _btnCancel;
        public Button _btnClose;

        private Button _btnSaveTop;
        private Button _btnCancelTop;

        public Color _colorTop = Colors.GhostWhite;
        public Color _colorBody = Colors.GhostWhite;
        public Color _colorBottom = Colors.GhostWhite;
        public Color _colorMain = Colors.GhostWhite;

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(); // 🔹 Esto notifica al Binding
                }
            }
        }

        IDispatcherTimer timer_eventController;

        double lastParentHeight = 0;
        double lastParentWidth = 0;
        public EventHandler popupSizeChanged { get; set; }

        public bool _boolShowButtonsFooter { get; set; } = true;

        public bool isWindows { get; set; } = false;
        private res_partner Sel_Res_Partner { get; set; }
        public PopupAccountAnalyticLine(PopupSizeConstants popupSizeConstants, ProjectTask _projectTask, AccountAnalyticLine _analyticLine)
        {
            projectTask = _projectTask;
            analyticLine = _analyticLine;

            isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

            popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            DesiredSize = new Size(100,100); // popupSizeConstants.Tiny;
            
            WidthRequest = 600;
            HeightRequest = 400;

            BackgroundColor = Colors.GhostWhite;
            
            Title = "NUEVA ACTIVIDAD";
            Padding = new Thickness(0);
            Margin = new Thickness(0);

            // Agregar manipuladores de eventos para los botones
            //_btnSave.Clicked += OnBtnSave_Clicked;
            //_btnCancel.Clicked += OnBtnCancel_Clicked;

            // Agregar componentes al Grid
            var gridContent = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.GhostWhite,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            //mainGrid.BackgroundColor = Colors.GhostWhite;            
            gridContent.VerticalOptions = LayoutOptions.Fill;

            gridContent.Padding = new Thickness(10,10,10,10);

            BuildTop(gridContent);

            BuildBody(gridContent);

            //BuildFooter(gridContent);

            Content = gridContent;

            PrepareForm();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            //base.OnSizeAllocated(width, height);
            //base.OnSizeAllocated(500, 500);
        }

        private void BuildBody(Grid gridContent)
        {
            _pickerPlanningSlot = new Picker
            {
                Title = "Principal",
                Margin = new Thickness(5, 2, 15, 2),
                IsVisible = false
            };

            _pickerPlanningReason = new DropdownField
            {
                Title = "Motivo",
                Margin = new Thickness(5, 2, 15, 2)
            };

            _pickerPlanningReason.Validations.Add(new RequiredValidation()); 

            _pickerCompany = new DropdownField
            {
                Title = "Compañia",
                Margin = new Thickness(5, 2, 15, 2)
            };

            _timePickerStart = new TimePicker
            {
            
                Margin = new Thickness(5, 2, 15, 2),
                Time = DateTime.Now.TimeOfDay,
                Format = "HH:mm",
                
                
            };

            _timePickerEnd = new TimePicker
            {
                
                Margin = new Thickness(5, 2, 15, 2),
                Time = DateTime.Now.TimeOfDay.Add(new TimeSpan(1, 0, 0)),
                Format = "HH:mm",
                
            };

            //_pickerBank = new Picker
            //{
            //    Title = "Banco",
            //    Margin = new Thickness(5, 5, 15, 5)
            //};

            //_pickerCurrency = new Picker()
            //{                
            //    Margin = new Thickness(5, 5, 15, 5),
            //    VerticalOptions = LayoutOptions.Center,
            //    //Title = "Moneda"
            //};

            //Enviar dinero
            Label _labelAllowOutPayment = new Label
            {
                Text = "-",
                Margin = new Thickness(0, 0, 5, 0),
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false
            };

            _switchAllowOutPayment = new Microsoft.Maui.Controls.Switch
            {                
                ThumbColor = Colors.White,
                OnColor = Colors.LimeGreen,
                IsVisible = false
            };

            var stackLayoutAllowOutPayment = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 5, 15, 5),
            };

            stackLayoutAllowOutPayment.Children.Add(_labelAllowOutPayment);
            stackLayoutAllowOutPayment.Children.Add(_switchAllowOutPayment);

            _inputResPartner = new TextField
            {
                Title = "Cliente",
                Text = "Seleccione un cliente...",
                Margin = new Thickness(5, 2, 15, 2),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill,
                IsReadOnly = true
            };

            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                AutomationId = "tapEntryResPartner"
            };

            tapGesture.Tapped += OnEntryTapped;
            _inputResPartner.Content.GestureRecognizers.Add(tapGesture);
            //_inputResPartner.GestureRecognizers.Add(tapGesture);

            _inputReview = new TextField
            {
                Title = "Observaciones",
                Margin = new Thickness(5, 2, 15, 2),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill                
            };

            FlexLayout flexLayout = new FlexLayout
            {
                Direction = FlexDirection.Column,
                //JustifyContent = FlexJustify.SpaceBetween,
                //AlignItems = FlexAlignItems.Center,
                //AlignContent = FlexAlignContent.Center,                
                Wrap = FlexWrap.NoWrap,                
                Margin = new Thickness(5, 0, 0, 5),
            };

            // Agregar elementos al FlexLayout
            flexLayout.Children.Add(_pickerPlanningSlot);
            flexLayout.Children.Add(_pickerPlanningReason);
            //flexLayout.Children.Add(_pickerCompany);            
            flexLayout.Children.Add(_inputResPartner);
            flexLayout.Children.Add(_inputReview);
            flexLayout.Children.Add(_timePickerStart);
            flexLayout.Children.Add(_timePickerEnd);

            //flexLayout.Children.Add(_pickerBank);                        
            //flexLayout.Children.Add(_pickerCurrency);
            flexLayout.Children.Add(stackLayoutAllowOutPayment);

            gridContent.Children.Add(flexLayout);
            Grid.SetRow(flexLayout, 1);
            Grid.SetRowSpan(flexLayout, 3);
            Grid.SetColumn(flexLayout, 0);
            Grid.SetColumnSpan(flexLayout, 2);
        }

        void OnEntryTapped(object sender, EventArgs e)
        {
            Console.WriteLine("Entry tapped!");
            // Código a ejecutar cuando se "tapea" en el Entry.
            HandleReturnResultPopupButtonClicked(sender, e);            
        }

        async void HandleReturnResultPopupButtonClicked(object sender, EventArgs e)
        {
            var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
            popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            //DesiredSize = popupSizeConstants.Medium;

            var returnResultPopup = new PopupSelectPartnerSingle(popupSizeConstants);
            
            returnResultPopup.Company = new res_company()
            {
                id = 1,
                name = "Macronegocios",
            };

            //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
            returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

            //TODO: Buscar modo de parametrizar 
            //if (!isWindows)
            //    returnResultPopup.DesiredSize = popupSizeConstants.Large;

            var result = await PopupExtensions.ShowPopupAsync<res_partner>(App.Current.MainPage, returnResultPopup);
            //var result = await this.ShowPopupAsync(returnResultPopup);

            if (result.Result != null)
            {
                Sel_Res_Partner = (res_partner) result.Result;
                _inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
                //_res_partnerItem = resPartner;
            }
        }

        private void BuildTop(Grid gridContent)
        {
            var flexTop = new FlexLayout
            {
                JustifyContent = FlexJustify.SpaceBetween,                
                BackgroundColor = _colorTop,
                Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,                
            };

            _btnSaveTop = new Button
            {
                Text = "  Guardar",
                TextColor = Colors.GhostWhite,
                BackgroundColor = Colors.SeaGreen,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(0,0,15,0),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf0c7"
                },                
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0),
                VerticalOptions = LayoutOptions.Start,
            };
                        

            // Agregar manipuladores de eventos para los botones
            _btnSaveTop.Clicked += OnBtnSave_Clicked;
            
            _labelTitle = new Label
            {
                //Text = Title,
                //Margin = new Thickness(15, 15, 0, 15),
                TextColor = Colors.DarkGray,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
            };

            _labelTitle.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

            _btnClose = new Button
            {
                //Text = "",
                BackgroundColor = Colors.GhostWhite,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 0, 0),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.DarkGray,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf00d"
                },
                CornerRadius = 0,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0),
                VerticalOptions = LayoutOptions.Start,
            };

            Button _btnBack = new Button
            {
                //Text = "",
                BackgroundColor = Colors.GhostWhite,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 0, 0, 0),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.DarkGray,
                    Size = 20,
                    FontAutoScalingEnabled = false,
                    Glyph = "\uf060"
                },
                CornerRadius = 0,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0),
                VerticalOptions = LayoutOptions.Start,
            };

            _btnBack.Clicked += OnBtnClose_Clicked;
            _btnClose.Clicked += OnBtnClose_Clicked;
            //_btnCloseSmall.Clicked += OnBtnClose_Clicked;

            flexTop.Children.Add(_btnBack);

            var _stackLayoutLabels = new StackLayout
            {
                Margin = new Thickness(0, 0, 0, 0),
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
            };

            //FlexLayout.SetGrow(_stackLayoutLabels, 1);
            _stackLayoutLabels.Children.Add(_labelTitle);           

            flexTop.Children.Add(_stackLayoutLabels);
            FlexLayout.SetOrder(_stackLayoutLabels, 1);
            //Botón de cerrar en pantalla pequeña
            //flexTop.Children.Add(_btnCloseSmall);

            var _stackLayoutTools = new StackLayout
            {
                Margin = new Thickness(0, 0, 10, 10),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
            };

            _stackLayoutTools.Children.Add(_btnSaveTop);
            _stackLayoutTools.Children.Add(_btnClose);

            flexTop.Children.Add(_stackLayoutTools);
            FlexLayout.SetOrder(_stackLayoutTools, 5);

            //flexTop.Children.Add(_btnClose);
            var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
            if (orientation == DisplayOrientation.Portrait)
            {
                //FlexLayout.SetOrder(_btnClose, 2);
                _btnClose.IsVisible = false;
            }
            else
            {
                //FlexLayout.SetOrder(_btnClose, 5);
                _btnClose.IsVisible = true;
            }

            var stackLayoutTopInner = new StackLayout
            {
                Margin = new Thickness(0, 0, 0, 0)
            };

            var borderBottom = new BoxView
            {
                //BackgroundColor = Colors.SlateGray, // Color del borde
                HeightRequest = 1, // Grosor del borde
                HorizontalOptions = LayoutOptions.Fill,
                Color = new Microsoft.Maui.Graphics.Color(100, 100, 100, 50)
            };

            stackLayoutTopInner.Children.Add(flexTop);

            _stackLayoutTop = new StackLayout
            {
                Children = { stackLayoutTopInner, borderBottom },
                Margin = new Thickness(0, 0, 0, 0)
            };

            gridContent.Children.Add(_stackLayoutTop);
            Grid.SetRow(_stackLayoutTop, 0);
            Grid.SetColumn(_stackLayoutTop, 0);
            Grid.SetColumnSpan(_stackLayoutTop, 2);
        }

        private void BuildFooter(Grid gridContent)
        {
            _btnSave = new Button
            {
                Text = "Guardar",
                BackgroundColor = Colors.SeaGreen,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf0c7"
                },
                IsVisible = _boolShowButtonsFooter
            };

            _btnCancel = new Button
            {
                Text = "Cancelar",
                BackgroundColor = Colors.IndianRed,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf00d"
                },
                IsVisible = _boolShowButtonsFooter
            };

            // Agregar manipuladores de eventos para los botones
            _btnSave.Clicked += OnBtnSave_Clicked;
            _btnCancel.Clicked += OnBtnCancel_Clicked;

            var borderTop = new BoxView
            {
                //BackgroundColor = Colors.SlateGray, // Color del borde
                HeightRequest = 1, // Grosor del borde
                HorizontalOptions = LayoutOptions.Fill,
                Color = new Microsoft.Maui.Graphics.Color(100, 100, 100, 50)
            };

            var gridBottom = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = _colorBottom,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
            };

            gridBottom.Children.Add(_btnSave);
            Grid.SetRow(_btnSave, 1);
            Grid.SetColumn(_btnSave, 0);

            gridBottom.Children.Add(_btnCancel);
            Grid.SetRow(_btnCancel, 1);
            Grid.SetColumn(_btnCancel, 1);

            _stackLayoutBottom = new StackLayout
            {
                Children = { borderTop, gridBottom },
                Margin = new Thickness(0, 0, 0, 0),
                //HorizontalOptions = LayoutOptions.Fill,
                MinimumHeightRequest = 50,
            };

            gridContent.Children.Add(_stackLayoutBottom);
            Grid.SetRow(_stackLayoutBottom, 6);
            Grid.SetColumn(_stackLayoutBottom, 0);
            Grid.SetColumnSpan(_stackLayoutBottom, 3);
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
            //DesiredSize = popupSizeConstants.Large;

            var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
            if (orientation == DisplayOrientation.Portrait)
            {                
                _btnClose.IsVisible = false;
            }
            else
            {
                FlexLayout.SetOrder(_btnClose, 4);
                _btnClose.IsVisible = true;
            }
        }

        private void PrepareForm()
        {
            Task.Run(async () =>
            {
                try
                {
                    Debug.WriteLine("Cargando los datos...");

                    if (projectTask != null)
                    {
                        ObservableCollection<ProjectTask> lplanning = new ObservableCollection<ProjectTask>();
                        lplanning.Add(projectTask);
                        _pickerPlanningSlot.ItemsSource = lplanning;
                        //_pickerPartner.ItemDisplayBinding = new Binding("name");

                        _pickerPlanningSlot.SelectedIndex = 0;
                        //labelArticulo.SetBinding(Label.TextProperty, new Binding(nameof(facturaItem.ARTICULO), source: this));
                    }
                    else
                    {
                        //Si no se ha enviado el partner de origen no se permitirá el ingreso del dato
                        await App.Current.MainPage.DisplayAlert("Nueva actividad",
                                            $"Se requiere que se especifique la actividad principal.",
                                            "Continuar");
                        await CloseAsync(default(AccountAnalyticLine));

                        return;
                    }

                    ObservableCollection<MotivoActividadDiaria> lplanning_reason = new ObservableCollection<MotivoActividadDiaria>();

                    MotivoActividadDiariaDb motivoActividadDiariaDb = new MotivoActividadDiariaDb(App.Session.odooConnection.DbNameSqlite);
                    lplanning_reason = new ObservableCollection<MotivoActividadDiaria>((await motivoActividadDiariaDb.GetItemsAsync()).OrderBy(i => i.name));

                    _pickerPlanningReason.ItemsSource = lplanning_reason;
                    _pickerPlanningReason.ItemDisplayBinding = new Binding("name");
                    //_pickerPlanningReason.SelectedItem = 0;

                    ObservableCollection<res_company> lcompany = new ObservableCollection<res_company>();
                    CompanyDb companyDb = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
                    lcompany = new ObservableCollection<res_company>((await companyDb.GetItemsAsync()).OrderBy(i => i.name));

                    //_pickerCompany.ItemsSource = lcompany;
                    //_pickerCompany.ItemDisplayBinding = new Binding("name");

                    if (analyticLine != null)
                    {
                        Title = "EDITANDO ACTIVIDAD";
                        _inputReview.Text = analyticLine.name;

                        ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);

                        if (analyticLine.partner_id != null)
                        {
                            Sel_Res_Partner = await resPartnerDb.GetItemsAsync(analyticLine.company_id, analyticLine.partner_id.Value);
                            if (Sel_Res_Partner != null)
                            {
                                _inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
                            }
                        }

                        //_pickerCompany.SelectedItem = lcompany.Where(i => i.id == analyticLine.company_id).FirstOrDefault();

                        var motivo_selected = lplanning_reason.Where(i => i.id == analyticLine.motivo).FirstOrDefault();
                        if (motivo_selected != null)
                        {
                            _pickerPlanningReason.SelectedItem = motivo_selected;
                        }
                        var time_start = TimeSpan.FromHours((double)analyticLine.hour_start);                        
                        _timePickerStart.Time = time_start;
                        //_timePickerStart.TimePickerView.SetValue(TimePickerField.TimeProperty, time_start);
                        var time_end = TimeSpan.FromHours((double)analyticLine.hour_end);
                        _timePickerEnd.Time = time_end;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en PrepareForm: {ex}");
                }
            });
        }

        private async void OnBtnSave_Clicked(object sender, EventArgs e)
        {
            //if(_inputResPartner.Text == null || _inputResPartner.Text == "")
            //{
            //    //await App.Current.MainPage.DisplayAlert("Cuenta requerida", "Llene el campo de cliente", "Cerrar");

            //    var messageView = new VerticalStackLayout
            //    {
            //        Margin = new Thickness(15),
            //        Children =
            //        {
            //            new Label
            //            {
            //                Text = "Llene el campo de cliente",
            //                FontSize = 15,
            //                FontAttributes = FontAttributes.None,
            //                HorizontalOptions = LayoutOptions.Center
            //            }
            //        }
            //    };

            //    //await ServicesExposer.DialogService.DisplayViewAsync("Cliente requerido", messageView);
            //    //var leave = await this.DisplayAlert("Atención", "Cliente requerido", "Si", "No");
            //    Debug.WriteLine("Cliente requerido");
            //    return;
            //}

            if (_inputReview.Text == null || _inputReview.Text == "")
            {
                //await App.Current.MainPage.DisplayAlert("Review requerido", "Llene el campo de observaciones", "Cerrar");
                //await ServicesExposer.DialogService.ConfirmAsync("Review requerido", "Llene el campo de observaciones", "Ok");

                //var messageView = new VerticalStackLayout
                //{
                //    Margin = new Thickness(15),
                //    Children =
                //    {
                //        new Label
                //        {
                //            Text = "Llene el campo de observaciones",
                //            FontSize = 15,
                //            FontAttributes = FontAttributes.None,
                //            HorizontalOptions = LayoutOptions.Center
                //        }
                //    }
                //};

                _inputReview.Focus();
                //await ServicesExposer.DialogService.DisplayViewAsync("Review requerido", messageView);
                await Toast.Make("Llene el campo de observaciones").Show();                
                return;
            }

            var motivo = (MotivoActividadDiaria) _pickerPlanningReason.SelectedItem;

            var time_start = _timePickerStart.Time.Value;
            double hour_start = time_start.Hours + (time_start.Minutes / 60.0) + (time_start.Seconds / 3600.0);

            var time_end = _timePickerEnd.Time.Value;
            double hour_end = time_end.Hours + (time_end.Minutes / 60.0) + (time_end.Seconds / 3600.0);

            if(hour_start >= hour_end)
            {                
                await Toast.Make("La hora final debe ser mayor a la hora de inicio").Show();
                return;
            }

            try
            {
                bool isNew = true;
                string title_save = "Guardar nueva actividad";
                string message_save = $"¿Desea guardar la nueva actividad para el cliente {_inputResPartner.Text}?";
                
                if (analyticLine != null)
                {
                    isNew = false;                    

                    title_save = "Modificar actividad";
                    message_save = $"¿Desea guardar los cambios de la actividad para el cliente {_inputResPartner.Text}?";
                }
                else
                {
                    analyticLine = new AccountAnalyticLine();
                }

                bool answer = await App.Current.MainPage.DisplayAlert(title_save,
                        message_save,
                        "Continuar", "Cerrar");

                if (!answer)
                {
                    return;
                }
                
                //new_PlanningSlot.id = 1;
                //analyticLine.name = $"VISITA {Sel_Res_Partner.name}";
                analyticLine.name = _inputReview.Text;
                if (Sel_Res_Partner != null)
                {
                    analyticLine.partner_id = Sel_Res_Partner.id;
                }

                analyticLine.company_id = App.Session.res_Company.id;
                analyticLine.project_id = projectTask.project_id_;
                analyticLine.task_id = projectTask.id;
                analyticLine.date = DateTime.Now;
                analyticLine.motivo = motivo.id;
                
                analyticLine.hour_start = (decimal)hour_start;
                analyticLine.hour_end = (decimal)hour_end;
                analyticLine.duration = analyticLine.hour_end - analyticLine.hour_start;

                AccountAnalyticLineDb accountAnalyticLineDb = new AccountAnalyticLineDb(App.Session.odooConnection.DbNameSqlite);
                if (isNew)
                {
                    await accountAnalyticLineDb.InsertAsync(analyticLine);
                }
                else
                {
                    await accountAnalyticLineDb.UpdateAsync(analyticLine);
                }

                await Toast.Make("Actividad guardada correctamente " + analyticLine.id.ToString()).Show();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error al guardar actividad: " + ex.Message).Show();
            }

            await CloseAsync(analyticLine);
        }

        private async void OnBtnCancel_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            await CloseAsync(default(AccountAnalyticLine));
        }

        private async void OnBtnClose_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            await CloseAsync(default(AccountAnalyticLine));
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
