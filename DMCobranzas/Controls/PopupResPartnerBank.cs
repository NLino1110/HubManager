using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Sample.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using DMCobranzas.Models;
using System.Diagnostics;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using DMSA.Models.Odoo.General.Responses;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;
using System.ComponentModel;
using DMCobranzas.Services.Database.Sqlite;

namespace DMCobranzas.Controls
{
    public class use_bank_type
    {
        public int id { get; set; }
        public string name { get; set;}
        public string description { get; set; }
    }

    public class type_account
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class currency_struct
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class PopupResPartnerBank : Popup<res_partner_bank>, INotifyPropertyChanged
    {
        //Origen de datos
        public res_partner partner { get; set; }

        private res_partner_bank new_Partner_Bank {  get; set; }

        private Picker _pickerPartner;
        private Picker _pickerTypeAccount;
        private Entry _inputAccNumber;
        private Entry _inputAccHolderName;
        private Picker _pickerUseBankType;
        private Picker _pickerBank;
        private Picker _pickerCurrency;
                
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
        public PopupResPartnerBank(PopupSizeConstants popupSizeConstants)
        {
            popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            DesiredSize = popupSizeConstants.Large;

            ///Nueva linea para la nueva version de CommunityToolKit
            /// se requiere para el fondo
            
            BackgroundColor = Colors.GhostWhite;
            Title = "NUEVA CUENTA BANCARIA";
            
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

            popupSizeChanged += OnPageSizeChanged;

            timer_eventController = Dispatcher.CreateTimer();
            timer_eventController.IsRepeating = true;
            timer_eventController.Interval = TimeSpan.FromMilliseconds(500);
            timer_eventController.Tick += async (s, e) =>
            {
                //if (Parent != null)
                //{
                //    if (((ContentPage)Parent).Height != lastParentHeight)
                //    {
                //        if (lastParentHeight > 0)
                //        {
                //            //Lanzar evento de Giro
                //            popupSizeChanged(this, EventArgs.Empty);
                //        }

                //        lastParentHeight = ((ContentPage)Parent).Height;
                //        lastParentWidth = ((ContentPage)Parent).Width;
                //    }
                //}
            };
            timer_eventController.Start();
        }

        private void BuildBody(Grid gridContent)
        {          
            // Inicializar componentes
            _pickerPartner = new Picker
            {
                Title = "Titular",
                Margin = new Thickness(5, 5, 15, 5),
                //ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0)
            };

            _pickerTypeAccount = new Picker
            {
                Title = "Tipo de Cuenta",
                Margin = new Thickness(5, 5, 15, 5)
            };

            _pickerUseBankType = new Picker
            {
                Title = "Uso cuenta bancaria",
                Margin = new Thickness(5, 5, 15, 5)
            };

            _pickerBank = new Picker
            {
                Title = "Banco",
                Margin = new Thickness(5, 5, 15, 5)
            };

            _pickerCurrency = new Picker()
            {                
                Margin = new Thickness(5, 5, 15, 5),
                VerticalOptions = LayoutOptions.Center,
                //Title = "Moneda"
            };

            //Enviar dinero
            Label _labelAllowOutPayment = new Label
            {
                Text = "Enviar dinero",
                Margin = new Thickness(0, 0, 5, 0),
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
            };

            _switchAllowOutPayment = new Microsoft.Maui.Controls.Switch
            {
                //Text = "Aceptar términos y condiciones"   
                //Margin = new Thickness(15, 5, 15, 5),
                ThumbColor = Colors.White,
                OnColor = Colors.LimeGreen,
            };

            var stackLayoutAllowOutPayment = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 5, 15, 5),
            };

            stackLayoutAllowOutPayment.Children.Add(_labelAllowOutPayment);
            stackLayoutAllowOutPayment.Children.Add(_switchAllowOutPayment);

            _inputAccNumber = new Entry
            {
                Placeholder = "Número de cuenta",
                Margin = new Thickness(5, 5, 0, 5),
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                MaximumWidthRequest = 200,
                MinimumWidthRequest = 200,
            };

            _inputAccHolderName = new Entry
            {
                Placeholder = "Nombre del titular de la cuenta",
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 5, 0, 5),
                MaximumWidthRequest = 250,
                MinimumWidthRequest = 250,
            };

            FlexLayout flexLayout = new FlexLayout
            {
                //Direction = FlexDirection.Row,
                //JustifyContent = FlexJustify.SpaceBetween,
                //AlignItems = FlexAlignItems.Center,
                //AlignContent = FlexAlignContent.Center,
                Wrap = FlexWrap.Wrap,
                Margin = new Thickness(15),
            };

            // Agregar elementos al FlexLayout
            flexLayout.Children.Add(_pickerPartner);
            flexLayout.Children.Add(_pickerTypeAccount);
            flexLayout.Children.Add(_inputAccNumber);
            flexLayout.Children.Add(_inputAccHolderName);
            flexLayout.Children.Add(_pickerUseBankType);
            flexLayout.Children.Add(_pickerBank);                        
            flexLayout.Children.Add(_pickerCurrency);
            flexLayout.Children.Add(stackLayoutAllowOutPayment);

            gridContent.Children.Add(flexLayout);
            Grid.SetRow(flexLayout, 1);
            Grid.SetRowSpan(flexLayout, 3);
            Grid.SetColumn(flexLayout, 0);
            Grid.SetColumnSpan(flexLayout, 2);
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
                    Size = 17,
                    FontAutoScalingEnabled = true,
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
                Margin = new Thickness(0, 0, 10, 0),
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

                //partner = new res_partner {
                //    email = "ronald.chonillo@gmail.com",
                //    id = 1,
                //    name = "Ronald Chonillo",
                //    vat = "0919826958"
                //};

                if ( partner != null )
                {
                    ObservableCollection<res_partner> lpartners = new ObservableCollection<res_partner>();
                    lpartners.Add(partner);
                    _pickerPartner.ItemsSource = lpartners;                    
                    _pickerPartner.ItemDisplayBinding = new Binding("name");
                    
                    _pickerPartner.SelectedIndex = 0;
                    //labelArticulo.SetBinding(Label.TextProperty, new Binding(nameof(facturaItem.ARTICULO), source: this));
                }
                else
                {
                    //Si no se ha enviado el partner de origen no se permitirá el ingreso del dato
                    await App.Current.MainPage.DisplayAlert("Nueva cuenta",
                                        $"Se requiere que se especifique el cliente para poder crear nueva cuenta bancaria",
                                        "Continuar");
                    await CloseAsync();
                    return;
                }

                ObservableCollection<use_bank_type> luse_bank_type = new ObservableCollection<use_bank_type>();
                luse_bank_type.Add(new use_bank_type() {
                    id = 1,
                    name = "company",
                    description = "Compañia"
                });

                luse_bank_type.Add(new use_bank_type()
                {
                    id = 2,
                    name = "employee",
                    description = "Empleado"
                });

                luse_bank_type.Add(new use_bank_type()
                {
                    id = 3,
                    name = "other",
                    description = "Otro"
                });

                _pickerUseBankType.ItemsSource = luse_bank_type;
                _pickerUseBankType.ItemDisplayBinding = new Binding("description");
                _pickerUseBankType.SelectedIndex = 0;


                ObservableCollection<type_account> ltype_account = new ObservableCollection<type_account>();
                ltype_account.Add(new type_account()
                {
                    id = 1,
                    name = "savings",
                    description = "Ahorros"
                });

                ltype_account.Add(new type_account()
                {
                    id = 2,
                    name = "current",
                    description = "Corriente"
                });

                ltype_account.Add(new type_account()
                {
                    id = 3,
                    name = "virtual",
                    description = "Virtual"
                });

                _pickerTypeAccount.ItemsSource = ltype_account;
                _pickerTypeAccount.ItemDisplayBinding = new Binding("description");
                _pickerTypeAccount.SelectedIndex = 0;

                ObservableCollection<currency_struct> lcurrency_struct = new ObservableCollection<currency_struct>();
                lcurrency_struct.Add(new currency_struct()
                {
                    id = 1,
                    name = "USD",
                    description = "USD"
                });

                _pickerCurrency.ItemsSource = lcurrency_struct;
                _pickerCurrency.ItemDisplayBinding = new Binding("description");
                _pickerCurrency.SelectedIndex = 0;

                ObservableCollection<Bank_Id> l_banks = new ObservableCollection<Bank_Id>();                
                BankDb bankDb = new BankDb();
                l_banks = new ObservableCollection<Bank_Id>( (await bankDb.GetItemsAsync()).OrderBy(i=>i.name) );

                _pickerBank.ItemsSource = l_banks;
                _pickerBank.ItemDisplayBinding = new Binding("name");
                _pickerBank.SelectedIndex = 0;

                string holder_name = ((res_partner)_pickerPartner.SelectedItem).name;
                _inputAccHolderName.Text = holder_name;

                timer.Stop();
            };
            timer.Start();
        }

        //internal static async Task<bool> ShowAlertAsync(string title, string message, string confirm, string close)
        //{
        //    var window = UIApplication.SharedApplication.Delegate.GetWindow();
        //    if (window.RootViewController.PresentedViewController is null)
        //    {
        //        return await ShowAlertFromPageAsync(title, message, confirm, close);
        //    }

        //    var result = new TaskCompletionSource<bool>();
        //    var confirmAlertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

        //    confirmAlertController.AddAction(UIAlertAction.Create(confirm, UIAlertActionStyle.Default, alert => result.SetResult(true)));
        //    confirmAlertController.AddAction(UIAlertAction.Create(close, UIAlertActionStyle.Cancel, alert => result.SetResult(false)));

        //    window.RootViewController.PresentedViewController.PresentViewController(confirmAlertController, true, null);

        //    return await result.Task;
        //}

        private async void OnBtnSave_Clicked(object sender, EventArgs e)
        {
            if(_inputAccNumber.Text == null || _inputAccNumber.Text == "")
            {
                await App.Current.MainPage.DisplayAlert("Cuenta requerida", "Llene el campo de número de cuenta", "Cerrar");
                return;
            }

            if (_inputAccHolderName.Text == null || _inputAccHolderName.Text == "")
            {
                await App.Current.MainPage.DisplayAlert("Titular requerido", "Llene el campo de nombre de titular", "Cerrar");
                return;
            }

            new_Partner_Bank = new res_partner_bank();
            new_Partner_Bank.PartnerId = ((res_partner) _pickerPartner.SelectedItem).id;
            new_Partner_Bank.type_account = ((type_account) _pickerTypeAccount.SelectedItem).name;
            new_Partner_Bank.acc_number = _inputAccNumber.Text != null ? _inputAccNumber.Text : "";
            new_Partner_Bank.acc_holder_name = _inputAccHolderName.Text !=null ? _inputAccHolderName.Text : "";
            new_Partner_Bank.use_bank_type = ((use_bank_type) _pickerUseBankType.SelectedItem).name;
            new_Partner_Bank.BankId = ((Bank_Id) _pickerBank.SelectedItem).id;
            new_Partner_Bank.bank_name = ((Bank_Id)_pickerBank.SelectedItem).name;
            new_Partner_Bank.CurrencyId = ((currency_struct) _pickerCurrency.SelectedItem).id;
            new_Partner_Bank.allow_out_payment = _switchAllowOutPayment.IsToggled;

            PartnerBankDb partnerBankDb = new PartnerBankDb();
            var existsPrevious = await partnerBankDb.GetMatch(new_Partner_Bank);

            if(existsPrevious != null)
            {
                BankDb bankDb = new BankDb();
                var bankItem = await bankDb.GetItem(existsPrevious.BankId);
                string bankName = "";
                
                if(bankItem!=null)
                {
                    bankName = bankItem.name;
                }

                await App.Current.MainPage.DisplayAlert("Cuenta existente",
                    $"Cuenta ya existente, no se puede guardar, {bankName}, {existsPrevious.display}", "Cerrar");

                return;
            }

            string BankName = ((Bank_Id)_pickerBank.SelectedItem).name;

            try
            {
                bool answer = await App.Current.MainPage.DisplayAlert("Nueva cuenta", 
                    $"Desea continuar guardando la nueva cuenta #{new_Partner_Bank.acc_number} para el banco {BankName}?", 
                    "Continuar", "Cerrar");
                if (!answer)
                {
                    return;
                }
            }
            catch (Exception ex)
            {

            }

            // Lógica cuando se hace clic en el primer botón
            await CloseAsync(new_Partner_Bank);
        }

        private async void OnBtnCancel_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            await CloseAsync();
        }

        private async void OnBtnClose_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            await CloseAsync();
        }
    }
}
