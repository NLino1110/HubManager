using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Helpers;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using InputKit.Shared.Validations;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UraniumUI.Dialogs;
using UraniumUI.Icons.MaterialIcons;
using UraniumUI.Material.Controls;

namespace DMOrders.Controls
{    
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public class PopupMailActivityPlanTemplate : Popup<MailActivityPlanTemplate>, INotifyPropertyChanged
    {
        public IDialogService DialogService { get; private set; }

        public MailActivityPlan activityPlan { get; set; }

        private MailActivityPlanTemplate new_ActivityPlanTemplate {  get; set; }

        private Picker _pickerPlanningSlot;

        private DropdownField _pickerPlanningReason;
        private DropdownField _pickerCompany;
        private TimePickerField _timePickerStart;
        private TimePickerField _timePickerEnd;
        
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

        public string Title { get; set; }
        IDispatcherTimer timer_eventController;

        double lastParentHeight = 0;
        double lastParentWidth = 0;
        public EventHandler popupSizeChanged { get; set; }

        public bool _boolShowButtonsFooter { get; set; } = true;

        public bool isWindows { get; set; } = false;

        public PopupMailActivityPlanTemplate(PopupSizeConstants popupSizeConstants)
        {
            isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

            popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            DesiredSize = popupSizeConstants.Medium;
            
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

            //popupSizeChanged += OnPageSizeChanged;

            //timer_eventController = Dispatcher.CreateTimer();
            //timer_eventController.IsRepeating = true;
            //timer_eventController.Interval = TimeSpan.FromMilliseconds(500);
            //timer_eventController.Tick += async (s, e) =>
            //{
            //    //if (Parent != null)
            //    //{
            //    //    if (((ContentPage)Parent).Height != lastParentHeight)
            //    //    {
            //    //        if (lastParentHeight > 0)
            //    //        {
            //    //            //Lanzar evento de Giro
            //    //            popupSizeChanged(this, EventArgs.Empty);
            //    //        }

            //    //        lastParentHeight = ((ContentPage)Parent).Height;
            //    //        lastParentWidth = ((ContentPage)Parent).Width;
            //    //    }
            //    //}
            //};
            //timer_eventController.Start();
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

            _timePickerStart = new TimePickerField
            {
                Title = "Hora Inicio",
                Margin = new Thickness(5, 2, 15, 2),
                Time = DateTime.Now.TimeOfDay,
                Icon = new FontImageSource
                {
                    FontFamily = "MaterialSharp",
                    Color = Colors.Black,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = MaterialSharp.Timer
                },
            };

            _timePickerEnd = new TimePickerField
            {
                Title = "Hora Fin",
                Margin = new Thickness(5, 2, 15, 2),
                Time = DateTime.Now.TimeOfDay.Add(new TimeSpan(1, 0, 0)),
                Icon = new FontImageSource
                {
                    FontFamily = "MaterialSharp",
                    Color = Colors.Black,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = MaterialSharp.Timer
                },
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
                Text = "<NO SELECCIONADO>",
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
            flexLayout.Children.Add(_pickerCompany);            
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
            var Sel_Res_Partner = new res_partner();

            var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
            popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            DesiredSize = popupSizeConstants.Medium;

            var returnResultPopup = new PopupSelectPartner(popupSizeConstants);
            
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
                //Text = "",
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
                Text = Title,
                //Margin = new Thickness(15, 15, 0, 15),
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
            };

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
            DesiredSize = popupSizeConstants.Large;

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
            IDispatcherTimer timer;

            timer = Dispatcher.CreateTimer();
            timer.IsRepeating = false;
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += async (s, e) =>
            {
                Debug.WriteLine("Cargando los datos...");

                if ( activityPlan != null )
                {
                    ObservableCollection<MailActivityPlan> lplanning = new ObservableCollection<MailActivityPlan>();
                    lplanning.Add(activityPlan);
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
                    await CloseAsync(default(MailActivityPlanTemplate));
                    
                    return;
                }

                ObservableCollection<PlanningReason> lplanning_reason = new ObservableCollection<PlanningReason>();
                lplanning_reason.Add(new PlanningReason() {
                    id = 1,
                    name = "company",
                    description = "Compañia"
                });

                lplanning_reason.Add(new PlanningReason()
                {
                    id = 2,
                    name = "employee",
                    description = "Empleado"
                });

                lplanning_reason.Add(new PlanningReason()
                {
                    id = 3,
                    name = "other",
                    description = "Otro"
                });

                _pickerPlanningReason.ItemsSource = lplanning_reason;
                _pickerPlanningReason.ItemDisplayBinding = new Binding("description");
                //_pickerPlanningReason.SelectedItem = 0;

                ObservableCollection<res_company> lcompany = new ObservableCollection<res_company>();
                lcompany.Add(new res_company()
                {
                    id = 1,
                    name = "Macronegocios",
                    //description = "Ahorros"
                });

                lcompany.Add(new res_company()
                {
                    id = 2,
                    name = "DMUJERES",
                    //description = "Corriente"
                });

                _pickerCompany.ItemsSource = lcompany;
                _pickerCompany.ItemDisplayBinding = new Binding("name");
                //_pickerCompany.SelectedItem = 0;

                //ObservableCollection<currency_struct> lcurrency_struct = new ObservableCollection<currency_struct>();
                //lcurrency_struct.Add(new currency_struct()
                //{
                //    id = 1,
                //    name = "USD",
                //    description = "USD"
                //});

                //_pickerCurrency.ItemsSource = lcurrency_struct;
                //_pickerCurrency.ItemDisplayBinding = new Binding("description");
                //_pickerCurrency.SelectedIndex = 0;

                //ObservableCollection<Bank_Id> l_banks = new ObservableCollection<Bank_Id>();                
                //BankDb bankDb = new BankDb();
                //l_banks = new ObservableCollection<Bank_Id>( (await bankDb.GetItemsAsync()).OrderBy(i=>i.name) );

                //_pickerBank.ItemsSource = l_banks;
                //_pickerBank.ItemDisplayBinding = new Binding("name");
                //_pickerBank.SelectedIndex = 0;

                //string holder_name = ((res_partner)_pickerPartner.SelectedItem).name;
                //_inputAccHolderName.Text = holder_name;

                timer.Stop();
            };
            timer.Start();
        }

        private async void OnBtnSave_Clicked(object sender, EventArgs e)
        {
            if(_inputResPartner.Text == null || _inputResPartner.Text == "")
            {
                //await App.Current.MainPage.DisplayAlert("Cuenta requerida", "Llene el campo de cliente", "Cerrar");

                var messageView = new VerticalStackLayout
                {
                    Margin = new Thickness(15),
                    Children =
                    {
                        new Label
                        {
                            Text = "Llene el campo de cliente",
                            FontSize = 15,
                            FontAttributes = FontAttributes.None,
                            HorizontalOptions = LayoutOptions.Center
                        }
                    }
                };

                //await ServicesExposer.DialogService.DisplayViewAsync("Cliente requerido", messageView);
                //var leave = await this.DisplayAlert("Atención", "Cliente requerido", "Si", "No");
                Debug.WriteLine("Cliente requerido");
                return;
            }

            if (_inputReview.Text == null || _inputReview.Text == "")
            {
                //await App.Current.MainPage.DisplayAlert("Review requerido", "Llene el campo de observaciones", "Cerrar");
                //await ServicesExposer.DialogService.ConfirmAsync("Review requerido", "Llene el campo de observaciones", "Ok");

                var messageView = new VerticalStackLayout
                {
                    Margin = new Thickness(15),
                    Children =
                    {
                        new Label
                        {
                            Text = "Llene el campo de observaciones",
                            FontSize = 15,
                            FontAttributes = FontAttributes.None,
                            HorizontalOptions = LayoutOptions.Center
                        }
                    }
                };

                //await ServicesExposer.DialogService.DisplayViewAsync("Review requerido", messageView);
                Debug.WriteLine("Review requerido");
                return;
            }

            new_ActivityPlanTemplate = new MailActivityPlanTemplate();
            new_ActivityPlanTemplate.plan_id = activityPlan;
            //new_ActivityHeader.type_account = ((type_account) _pickerCompany.SelectedItem).name;
            //new_ActivityHeader.acc_number = _inputAccNumber.Text != null ? _inputAccNumber.Text : "";
            //new_ActivityHeader.acc_holder_name = _inputAccHolderName.Text !=null ? _inputAccHolderName.Text : "";
            //new_ActivityHeader.use_bank_type = ((use_bank_type) _pickerPlanningReason.SelectedItem).name;
            //new_ActivityHeader.BankId = ((Bank_Id) _pickerBank.SelectedItem).id;
            //new_ActivityHeader.bank_name = ((Bank_Id)_pickerBank.SelectedItem).name;
            //new_ActivityHeader.CurrencyId = ((currency_struct) _pickerCurrency.SelectedItem).id;
            //new_ActivityHeader.allow_out_payment = _switchAllowOutPayment.IsToggled;

            //PartnerBankDb partnerBankDb = new PartnerBankDb();
            //var existsPrevious = await partnerBankDb.GetMatch(new_Partner_Bank);

            //if(existsPrevious != null)
            //{
            //    BankDb bankDb = new BankDb();
            //    var bankItem = await bankDb.GetItem(existsPrevious.BankId);
            //    string bankName = "";

            //    if(bankItem!=null)
            //    {
            //        bankName = bankItem.name;
            //    }

            //    await App.Current.MainPage.DisplayAlert("Cuenta existente",
            //        $"Cuenta ya existente, no se puede guardar, {bankName}, {existsPrevious.display}", "Cerrar");

            //    return;
            //}

            //string BankName = ((Bank_Id)_pickerBank.SelectedItem).name;

            try
            {   

                bool answer = await App.Current.MainPage.DisplayAlert("Nueva Actividad",
                    $"Desea continuar guardando la nueva cuenta #{new_ActivityPlanTemplate.plan_id.id} para el cliente {_inputResPartner.Text}?",
                    "Continuar", "Cerrar");

                if (!answer)
                {
                    return;
                }
            }
            catch (Exception ex)
            {

            }

            await CloseAsync(new_ActivityPlanTemplate);
        }

        private async void OnBtnCancel_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            await CloseAsync(default(MailActivityPlanTemplate));
        }

        private async void OnBtnClose_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            await CloseAsync(default(MailActivityPlanTemplate));
        }
    }
}
