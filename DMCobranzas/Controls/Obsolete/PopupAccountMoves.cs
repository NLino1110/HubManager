using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Sample.Models;
using Microsoft.Maui.Controls;
using System.Drawing;
using Microsoft.Maui.Graphics;
using DMCobranzas.Models;
using System.Diagnostics;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using DMCobranzas.Settings.Sqlite;
using DMSA.Models.Odoo.General.Responses;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DMCobranzas.Settings.helpers;

namespace DMCobranzas.Controls
{
    public class PopupAccountMoves : Popup //, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        //public ICommand PerformSearch => new Command<string>((string query) =>
        //{
        //    SearchResults = DataService.GetSearchResults(query);
        //});

        //private List<string> searchResults = DataService.Fruits;
        //public List<string> SearchResults
        //{
        //    get
        //    {
        //        return searchResults;
        //    }
        //    set
        //    {
        //        searchResults = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        private AbsoluteLayout _layoutLoading {  get; set; }

        public res_company Company { get; set; }
        //Origen de datos
        public res_partner partner { get; set; }
        
        private SearchBar _searchBar { get; set; }
        
        private Microsoft.Maui.Controls.Switch _switchWithCredit;

        private StackLayout _stackLayoutTop;
        private Label _labelOverTitle;
        private Label _labelTitle;
        private Label _labelCount;

        private StackLayout _stackLayoutBottom;

        private Button _btnSave;
        private Button _btnCancel;

        private ScrollView _scrollView;
        private CollectionView _collectionViewSearch;

        private Microsoft.Maui.Graphics.Color _colorTop = Colors.GhostWhite;
        private Microsoft.Maui.Graphics.Color _colorBody = Colors.GhostWhite;
        private Microsoft.Maui.Graphics.Color _colorBottom = Colors.GhostWhite;
        private Microsoft.Maui.Graphics.Color _colorMain = Colors.GhostWhite;

        private bool _boolShowFooter = true;
        private bool _boolShowValues = true;

        public PopupAccountMoves(PopupSizeConstants popupSizeConstants)
        {            
            Size = popupSizeConstants.Medium;
            Color = _colorMain;
            
            // Agregar componentes al Grid
            var gridContent = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
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

            gridContent.BackgroundColor = Colors.GhostWhite;
            gridContent.VerticalOptions = LayoutOptions.Fill;

            gridContent.Padding = new Thickness(10,10,10,10);

            BuildTop(gridContent);

            BuildBody(gridContent);
            
            if(_boolShowFooter)
                BuildFooter(gridContent);

            Content = gridContent;

            PrepareForm();


        }

        async Task<int> LoadData()
        {
            IDispatcherTimer timer;

            timer = Dispatcher.CreateTimer();
            timer.IsRepeating = false;
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += async (s, e) =>
            {
                //SearchBar searchBar = (SearchBar) sender;

                //if (_searchBar.Text.Length < 2)
                //{
                //    return;
                //}

                _scrollView.IsVisible = false;
                await UITools.ShowLoading(_layoutLoading);

                AccountMoveDb accountMoveDb = new AccountMoveDb();
                var data_result = await accountMoveDb.GetItemsByPartnerForPaymentAsync(partner);
                //partner
                ObservableCollection<account_move> oc_data_result = new ObservableCollection<account_move>();
                oc_data_result = new ObservableCollection<account_move>(data_result);
                _collectionViewSearch.ItemsSource = oc_data_result;

                _labelCount.Text = "Total registros " + oc_data_result.Count().ToString();
                Debug.WriteLine(oc_data_result.Count());

                await UITools.HideLoading(_layoutLoading);
                _scrollView.IsVisible = true;
                timer.Stop();
            };

            timer.Start();

            return 1;
        }

        async void _searchBar_OnTextChanged(object sender, EventArgs e)
        {
            await LoadData();

            //SearchBar searchBar = (SearchBar)sender;

            //if (_searchBar.Text.Length < 2)
            //{
            //    return;
            //}

            //ResPartnerDb partnerBankDb = new ResPartnerDb();
            //ObservableCollection<res_partner> lpartners = new ObservableCollection<res_partner>();
            //lpartners = new ObservableCollection<res_partner>(await partnerBankDb.GetItemsBySearchAsync(Company.id, _searchBar.Text.ToUpper(), 25));
            //_collectionViewSearch.ItemsSource = lpartners;

            //_labelCount.Text = "Total registros " + lpartners.Count().ToString();
            //Debug.WriteLine(lpartners.Count());
        }

        //private async void _inputSearch_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //throw new NotImplementedException();

        //    if(_inputSearch.Text.Length < 3)
        //    {
        //        return;
        //    }

        //    ResPartnerDb partnerBankDb = new ResPartnerDb();
        //    ObservableCollection<res_partner> lpartners = new ObservableCollection<res_partner>();
        //    lpartners = new ObservableCollection<res_partner>(await partnerBankDb.GetItemsBySearchAsync(1, _inputSearch.Text.ToUpper()));
        //    _collectionViewSearch.ItemsSource = lpartners;

        //    Debug.WriteLine(lpartners.Count());

        //    //_pickerPartner.ItemDisplayBinding = new Binding("name");
        //    //_pickerPartner.SelectedIndex = 0;
        //    //labelArticulo.SetBinding(Label.TextProperty, new Binding(nameof(facturaItem.ARTICULO), source: this));
        //}

        private void PrepareForm()
        {
            IDispatcherTimer timer;

            timer = Dispatcher.CreateTimer();
            timer.IsRepeating = false;
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += async (s, e) =>
            {
                Debug.WriteLine("Cargando los datos...");

                if(Company==null || Company.id == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Facturas",
                                        $"Se requiere que se especifique la compañia para poder realizar la búsqueda de clientes.",
                                        "Continuar");
                    Close(null);
                    return;
                }

                if (partner == null)
                {
                    await App.Current.MainPage.DisplayAlert("Facturas",
                                        $"Se requiere que se especifique un cliente para poder mostrar los datos.",
                                        "Continuar");
                    Close(null);
                    return;
                }

                _labelOverTitle.Text = Company.name;

                await LoadData();

                timer.Stop();
            };
            timer.Start();
        }

        private void BuildTop(Grid gridContent)
        {
            var gridTop = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = _colorTop,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                }
            };

            _searchBar = new SearchBar
            {
                Placeholder = "",
                Margin = new Thickness(0,5,0,5),                
            };

            //_searchBar.TextChanged += _searchBar_OnTextChanged;
            _searchBar.SearchButtonPressed += _searchBar_OnTextChanged;

            var stackLayoutAllowOutPayment = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(15, 5, 15, 5),
            };
                        
            Label _labelWithCredit = new Label
            {
                Text = "Saldos > 0",
                Margin = new Thickness(0, 0, 5, 0),
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
            };

            _switchWithCredit = new Microsoft.Maui.Controls.Switch
            {
                //Text = "Aceptar términos y condiciones"
                Margin = new Thickness(15, 5, 15, 5),
                ThumbColor = Colors.White,
                OnColor = Colors.LimeGreen,
            };

            stackLayoutAllowOutPayment.Children.Add(_labelWithCredit);
            stackLayoutAllowOutPayment.Children.Add(_switchWithCredit);


            _labelOverTitle = new Label
            {
                Text = "Compañía",
                //Margin = new Thickness(15, 15, 0, 15),
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                TextColor = Colors.SeaGreen,
            };

            _labelTitle = new Label
            {
                Text = "Facturas de cliente",
                //Margin = new Thickness(15, 15, 0, 15),
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            };

            Button _btnClose = new Button
            {
                //Text = "",
                BackgroundColor = Colors.GhostWhite,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.DarkGray,
                    Size = 17,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf00d"
                },
                CornerRadius = 0,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0)
                //MinimumWidthRequest = 10,
                //MinimumHeightRequest = 10,
                //MaximumHeightRequest = 20,
                //MaximumWidthRequest = 20,
            };

            Button _btnBack = new Button
            {
                //Text = "",
                BackgroundColor = Colors.GhostWhite,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.DarkGray,
                    Size = 17,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf060"
                },
                CornerRadius = 0,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0)                
            };

            _btnBack.Clicked += OnBtnClose_Clicked;
            _btnClose.Clicked += OnBtnClose_Clicked;

            gridTop.Children.Add(_btnBack);
            Grid.SetRow(_btnBack, 0);
            Grid.SetColumn(_btnBack, 0);
            Grid.SetRowSpan(_btnBack, 2);

            gridTop.Children.Add(_labelTitle);
            Grid.SetRow(_labelTitle, 0);
            Grid.SetColumn(_labelTitle, 1);

            gridTop.Children.Add(_labelOverTitle);
            Grid.SetRow(_labelOverTitle, 1);
            Grid.SetColumn(_labelOverTitle, 1);

            //Grid.SetColumnSpan(_labelTitle, 3);

            gridTop.Children.Add(_searchBar);
            Grid.SetRow(_searchBar, 0);
            Grid.SetColumn(_searchBar, 2);
            Grid.SetColumnSpan(_searchBar, 2);
            Grid.SetRowSpan(_searchBar, 2);

            gridTop.Children.Add(stackLayoutAllowOutPayment);
            Grid.SetRow(stackLayoutAllowOutPayment, 0);
            Grid.SetColumn(stackLayoutAllowOutPayment, 4);
            Grid.SetRowSpan(stackLayoutAllowOutPayment, 2);
            //Grid.SetColumnSpan(_switchWithCredit, 2);

            /************************************/

            gridTop.Children.Add(_btnClose);
            Grid.SetRow(_btnClose, 0);
            Grid.SetColumn(_btnClose, 5);
            Grid.SetRowSpan(_btnClose, 2);

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

            stackLayoutTopInner.Children.Add(gridTop);

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
                }
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
                }
            };

            _labelCount = new Label
            {
                Text = "Total registros 0",
                //Margin = new Thickness(15, 15, 0, 15),
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                TextColor = Colors.DarkGray,
                HorizontalOptions= LayoutOptions.End,
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
                }
            };

            gridBottom.Children.Add(_btnSave);
            Grid.SetRow(_btnSave, 1);
            Grid.SetColumn(_btnSave, 0);

            gridBottom.Children.Add(_btnCancel);
            Grid.SetRow(_btnCancel, 1);
            Grid.SetColumn(_btnCancel, 1);

            gridBottom.Children.Add(_labelCount);
            Grid.SetRow(_labelCount, 1);
            Grid.SetColumn(_labelCount, 4);

            _stackLayoutBottom = new StackLayout
            {
                Children = { borderTop, gridBottom },
                Margin = new Thickness(0, 0, 0, 0),
                //HorizontalOptions = LayoutOptions.Fill,
            };

            gridContent.Children.Add(_stackLayoutBottom);
            Grid.SetRow(_stackLayoutBottom, 6);
            Grid.SetColumn(_stackLayoutBottom, 0);
            Grid.SetColumnSpan(_stackLayoutBottom, 3);
        }

        private Frame BuildHeaderTable()
        {
            Grid _gridHeader = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var _lblTitle_00 = new Label
            {
                Text = "Id",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.WhiteSmoke,
                FontAttributes = FontAttributes.Bold
            };

            var _lblTitle_01 = new Label
            {
                Text = "Nombre",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.WhiteSmoke,
                FontAttributes = FontAttributes.Bold
            };

            var _lblTitle_02 = new Label
            {
                Text = "Vat",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.WhiteSmoke,
                FontAttributes = FontAttributes.Bold
            };

            var _lblTitle_03 = new Label
            {
                Text = "Email",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.WhiteSmoke,
                FontAttributes = FontAttributes.Bold
            };

            //var importeLabel = new Label
            //{
            //    Text = "Importe",
            //    HorizontalOptions = LayoutOptions.Start,
            //    VerticalOptions = LayoutOptions.Center,
            //    TextColor = Colors.WhiteSmoke,
            //    FontAttributes = FontAttributes.Bold
            //};

            //var referenciaLabel = new Label
            //{
            //    Text = "Referencia",
            //    HorizontalOptions = LayoutOptions.Start,
            //    VerticalOptions = LayoutOptions.Center,
            //    TextColor = Colors.WhiteSmoke,
            //    FontAttributes = FontAttributes.Bold
            //};

            _gridHeader.Children.Add(_lblTitle_00);
            Grid.SetRow(_lblTitle_01, 0);
            Grid.SetColumn(_lblTitle_01, 0);

            _gridHeader.Children.Add(_lblTitle_01);
            Grid.SetRow(_lblTitle_01, 0);
            Grid.SetColumn(_lblTitle_01, 1);

            _gridHeader.Children.Add(_lblTitle_02);
            Grid.SetRow(_lblTitle_02, 0);
            Grid.SetColumn(_lblTitle_02, 2);

            _gridHeader.Children.Add(_lblTitle_03);
            Grid.SetRow(_lblTitle_03, 0);
            Grid.SetColumn(_lblTitle_03, 3);

            //_gridHeader.Children.Add(importeLabel);
            //Grid.SetRow(importeLabel, 0);
            //Grid.SetColumn(importeLabel, 4);

            //_gridHeader.Children.Add(referenciaLabel);
            //Grid.SetRow(referenciaLabel, 0);
            //Grid.SetColumn(referenciaLabel, 5);

            var frame = new Frame
            {
                BorderColor = Colors.LightGray,
                Padding = new Thickness(10),
                Margin = new Thickness(1),
                BackgroundColor = Colors.Gray,
                CornerRadius = 0,
                Content = _gridHeader
            };

            return frame;
        }

        private void BuildBody(Grid gridContent)
        {
            _layoutLoading = new AbsoluteLayout();
            _layoutLoading.HorizontalOptions = LayoutOptions.Center;
            _layoutLoading.VerticalOptions = LayoutOptions.Center;
            _layoutLoading.BackgroundColor = Colors.GhostWhite;

            CommandSelectListItem = new Command(SelectListItem);

            var stackLayoutBodyTop = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(15, 5, 15, 5),
            };            

            _collectionViewSearch = new CollectionView
            {
                ItemTemplate = new DataTemplate(() =>
                {
                    var swipeView = new SwipeView { IsClippedToBounds = true };

                    Button btnSelectSwipeWindows = new Button
                    {
                        CornerRadius = 0,
                        //ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 0),
                        //Command = new Command<object>((obj) =>
                        //{
                        //    // Lógica al hacer clic en el botón
                        //    Debug.WriteLine("Seleccionado");
                        //}),
                        Command = CommandSelectListItem,
                        //CommandParameter = new Binding("."), // Establece el objeto de datos como parámetro del comando
                        //MaximumWidthRequest = 120,
                        BackgroundColor = Colors.DeepSkyBlue,
                        Text = "_Select",
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 13,
                        HorizontalOptions = LayoutOptions.End,
                        ImageSource = new FontImageSource
                        {
                            FontFamily = "FontAwesome5Solid",
                            Color = Colors.White,
                            Size = 20,
                            FontAutoScalingEnabled = true,
                            Glyph = "\uf058"
                        }
                    };

                    Button btnForSwipe = new Button
                    {
                        CornerRadius = 0,                       
                        Command = CommandSelectListItem,                        
                        BackgroundColor = Colors.DeepSkyBlue,
                        Text = "_Select",
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 13,
                        HorizontalOptions = LayoutOptions.End,
                        WidthRequest = 100,
                        ImageSource = new FontImageSource
                        {
                            FontFamily = "FontAwesome5Solid",
                            Color = Colors.White,
                            Size = 20,
                            FontAutoScalingEnabled = true,
                            Glyph = "\uf058"
                        }
                    };

                    btnSelectSwipeWindows.SetBinding(Button.CommandParameterProperty, new Binding("."));
                    btnForSwipe.SetBinding(Button.CommandParameterProperty, new Binding("."));

                    swipeView.RightItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;

                    swipeView.RightItems.Add(
                            new SwipeItemView
                            {
                                Content = btnForSwipe
                            }
                        );

                    var scrollGridContent = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        BackgroundColor = Colors.WhiteSmoke,
                        RowDefinitions = new RowDefinitionCollection
                        {
                            new RowDefinition { Height = GridLength.Auto },
                            //new RowDefinition { Height = GridLength.Auto },
                            //new RowDefinition { Height = GridLength.Auto }
                        },
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = 60 }, //id
                            new ColumnDefinition { Width = 300 }, //name
                            new ColumnDefinition { Width = 130 }, //vat
                            new ColumnDefinition { Width = 210 }, //email
                            new ColumnDefinition { Width = 80 },
                            new ColumnDefinition { Width = 80 },
                            new ColumnDefinition { Width = 110 }
                        }
                    };

                    var stackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        BackgroundColor = Colors.Red,
                        IsVisible = true,
                        Padding = new Thickness(5,0,0,0)
                    };

                    var label0 = new Label
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        FontAttributes = FontAttributes.Bold,
                    };

                    //label1.SetBinding(Label.FormattedTextProperty, new Binding("vat", stringFormat: "{0}", source: "."));
                    label0.SetBinding(Label.TextProperty, new Binding("name"));

                    var label1 = new Label
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        Margin = new Thickness (5,0,15,0)
                    };

                    //label1.SetBinding(Label.FormattedTextProperty, new Binding("vat", stringFormat: "{0}", source: "."));
                    label1.SetBinding(Label.TextProperty, new Binding("invoice_date"));

                    var label2 = new Label
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center
                    };

                    //label2.SetBinding(Label.FormattedTextProperty, new Binding("name", stringFormat: "$ {0}", source: "."));
                    //label2.SetBinding(Label.FormattedTextProperty, new Binding("name", stringFormat: "$ {0}"));
                    label2.SetBinding(Label.TextProperty, new Binding("vat"));

                    var label3 = new Label
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center
                    };

                    label3.SetBinding(Label.TextProperty, new Binding("email"));

                    var label4 = new Label
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center
                    };

                    label4.SetBinding(Label.TextProperty, new Binding("amount_residual"));

                    var label5 = new Label
                    {
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center
                    };

                    label5.SetBinding(Label.TextProperty, new Binding("amount_total"));

                    scrollGridContent.Children.Add(label0);
                    Grid.SetRow(label0, 0);
                    Grid.SetColumn(label0, 0);

                    scrollGridContent.Children.Add(label1);
                    Grid.SetRow(label1, 0);
                    Grid.SetColumn(label1, 1);
                    //Grid.SetColumnSpan(label1, 2);

                    scrollGridContent.Children.Add(label2);
                    Grid.SetRow(label2, 0);
                    Grid.SetColumn(label2, 2);

                    scrollGridContent.Children.Add(label3);
                    Grid.SetRow(label3, 0);
                    Grid.SetColumn(label3, 3);

                    scrollGridContent.Children.Add(label4);
                    Grid.SetRow(label4, 0);
                    Grid.SetColumn(label4, 4);

                    scrollGridContent.Children.Add(label5);
                    Grid.SetRow(label5, 0);
                    Grid.SetColumn(label5, 5);

                    stackLayout.Children.Add(btnSelectSwipeWindows);

                    scrollGridContent.Add(stackLayout, 6, 0);
                    Grid.SetRow(stackLayout, 0);
                    Grid.SetColumn(stackLayout, 6);
                    //Grid.SetRowSpan(stackLayout, 3);

                    var frame = new Frame
                    {
                        BorderColor = Colors.LightGray,
                        Padding = new Thickness(10),
                        Margin = new Thickness(1),
                        BackgroundColor = Colors.WhiteSmoke,
                        CornerRadius = 0,
                        Content = scrollGridContent
                    };

                    swipeView.Content = frame;

                    return swipeView;
                })
            };

            //_collectionViewSearch.MinimumHeightRequest = 260;
            //_collectionViewSearch.BackgroundColor = Colors.Yellow;

            _scrollView = new ScrollView
            {
                //BackgroundColor = Colors.Red,
                VerticalOptions = LayoutOptions.Fill,
                //HorizontalOptions = LayoutOptions.Fill,
                MaximumHeightRequest = 330,
                HeightRequest = 330,
                Content = _collectionViewSearch
            };

            //Label lblInfo = new Label() { Text = "Cabecera" };
            Frame frmHeader = BuildHeaderTable();
            gridContent.Children.Add(frmHeader);
            Grid.SetRow(frmHeader, 2);
            Grid.SetColumn(frmHeader, 0);
            Grid.SetColumnSpan(frmHeader, 2);

            gridContent.Children.Add(_scrollView);
            Grid.SetRow(_scrollView, 3);
            Grid.SetColumn(_scrollView, 0);
            Grid.SetColumnSpan(_scrollView, 3);
            Grid.SetRowSpan(_scrollView, 3);

            gridContent.Children.Add(_layoutLoading);
            Grid.SetRow(_layoutLoading, 3);
            Grid.SetColumn(_layoutLoading, 0);
            //Grid.SetColumnSpan(_scrollView, 3);
            //Grid.SetRowSpan(_layoutLoading, 3); 
        }

        public ICommand CommandSelectListItem { get; set; }

        private async void SelectListItem(object objItem)
        {            
            if (objItem != null)
            {
                Close(objItem);
            }
            else
            {
                Debug.WriteLine("Error de objeto");
            }
        }

        private async void OnBtnSave_Clicked(object sender, EventArgs e)
        {
            ResPartnerDb partnerBankDb = new ResPartnerDb();
            res_partner res_Partner = await partnerBankDb.GetItemsAsync(1,1);
            // Lógica cuando se hace clic en el primer botón
            Close(res_Partner);
        }

        private void OnBtnCancel_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            Close(null);
        }

        private void OnBtnClose_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            Close(null);
        }
    }
}
