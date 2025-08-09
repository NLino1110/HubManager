using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Core.Platform;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Layouts;
using DMCobranzas.Settings.helpers;
using Microsoft.Maui.Controls;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;

namespace DMCobranzas.Controls
{
    public class PopupSelectBase : Popup, INotifyPropertyChanged
    {
        protected static Grid scrollGridContent { get; set; }
        private AbsoluteLayout _layoutLoading { get; set; }

        private SearchBar _searchBar { get; set; }

        public Microsoft.Maui.Controls.Switch _switchWithCredit;

        public StackLayout _stackLayoutTop;
        public Label _labelOverTitle;
        public Label _labelTitle;
        public Label _labelCount;

        private StackLayout _stackLayoutToolBox1;
        private StackLayout _stackLayoutToolBox2;
        public StackLayout _stackLayoutBottom;

        public Button _btnSave;
        public Button _btnCancel;
        public Button _btnClose;

        public ScrollView _scrollView;
        public CollectionView _collectionViewSearch;
        HeaderLikeTable contentViewHeader;

        public Color _colorTop = Colors.GhostWhite;
        public Color _colorBody = Colors.GhostWhite;
        public Color _colorBottom = Colors.GhostWhite;
        public Color _colorMain = Colors.GhostWhite;

        public bool _boolShowButtonsFooter { get; set; } = false;
        public bool _boolShowValues { get; set; } = true;

        IDispatcherTimer timer_eventController;

        double lastParentHeight = 0;
        double lastParentWidth = 0;
        public EventHandler popupSizeChanged {  get; set; }

        //Eventos para clases heredadas
        public EventHandler _LaunchSearchEvent { get; set; }
        public EventHandler _CustomSaveClickEvent { get; set; }
        public EventHandler _CustomCloseClickEvent { get; set; }

        public EventHandler _OnAppearing { get; set; }

        //Propiedades para clases heredadas
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public static string DataField {  get; set; } = "id, name";
        public string TextForSearch { get; set; }

        //public bool ShowTextSearch { get; set; } = true;

        public bool _renderCustomDataTemplate = false;

        //public static readonly BindableProperty CustomDataTemplateProperty =
        //    BindableProperty.Create(nameof(CustomDataTemplate),
        //        typeof(ContentView), //Clase de datos auxiliar
        //        typeof(PopupSelectBase), //Clase contenedora
        //        null);

        //public ContentView CustomDataTemplate
        //{
        //    get => (ContentView)GetValue(CustomDataTemplateProperty);
        //    set => SetValue(CustomDataTemplateProperty, value);
        //}

        //private Func<ContentView> _CustomDataTemplateFunc { get; set; }

        public static readonly BindableProperty ShowTextSearchProperty =
            BindableProperty.Create(nameof(ShowTextSearch),
                typeof(bool), //Clase de datos auxiliar
                typeof(PopupSelectBase), //Clase contenedora
                true);

        public bool ShowTextSearch
        {
            get => (bool)GetValue(ShowTextSearchProperty);
            set => SetValue(ShowTextSearchProperty, value);
        }

        public static readonly BindableProperty ShowToolBoxProperty =
            BindableProperty.Create(nameof(ShowToolBox),
                typeof(bool),
                typeof(PopupSelectBase),
                true);

        public bool ShowToolBox
        {
            get => (bool)GetValue(ShowToolBoxProperty);
            set => SetValue(ShowToolBoxProperty, value);
        }

        public static readonly BindableProperty ContentCustomToolBoxProperty =
            BindableProperty.Create(nameof(ContentCustomToolBox),
                typeof(ContentView),
                typeof(PopupSelectBase));

        public ContentView ContentCustomToolBox
        {
            get => (ContentView)GetValue(ContentCustomToolBoxProperty);
            set => SetValue(ContentCustomToolBoxProperty, value);
        }

        public PopupSelectBase(PopupSizeConstants popupSizeConstants)
        {
            PopupSelectBaseBuild(popupSizeConstants);
        }

        public PopupSelectBase(PopupSizeConstants popupSizeConstants, 
            bool renderCustomDataTemplate)
            //ContentView cvCustomDataTemplate,
            //Func<ContentView> _CustomDataTemplateFuncParam)
        {
            _renderCustomDataTemplate = renderCustomDataTemplate;
            //CustomDataTemplate = cvCustomDataTemplate;
            //_CustomDataTemplateFunc = _CustomDataTemplateFuncParam;
            PopupSelectBaseBuild(popupSizeConstants);
        }

        public void PopupSelectBaseBuild(PopupSizeConstants popupSizeConstants) //, bool renderCustomDataTemplate)
        {
            popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            Size = popupSizeConstants.Large;
            Color = _colorMain;
            
            // Agregar componentes al Grid
            var gridContent = new Grid
            {                
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star },
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
            gridContent.HorizontalOptions = LayoutOptions.Fill;

            gridContent.Padding = new Thickness(2,2,2,2);
                      
            BuildTop(gridContent);

            BuildBody(gridContent);
            
            BuildFooter(gridContent);

            Content = gridContent;

            PrepareForm();

            //IDispatcherTimer timer;

            popupSizeChanged += OnPageSizeChanged;

            timer_eventController = Dispatcher.CreateTimer();
            timer_eventController.IsRepeating = true;
            timer_eventController.Interval = TimeSpan.FromMilliseconds(500);
            timer_eventController.Tick += async (s, e) =>
            {
                if (Parent != null)
                {
                    if (((ContentPage)Parent).Height != lastParentHeight)
                    {
                        if(lastParentHeight > 0)
                        {
                            //Lanzar evento de Giro
                            popupSizeChanged(this, EventArgs.Empty);
                        }

                        lastParentHeight = ((ContentPage)Parent).Height;
                        lastParentWidth = ((ContentPage)Parent).Width;
                    }
                }
            };
            timer_eventController.Start();
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
            Size = popupSizeConstants.Large;

            var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
            if(orientation == DisplayOrientation.Portrait)
            {
                //FlexLayout.SetOrder(_btnClose, 2);
                _btnClose.IsVisible = false;
            }
            else
            {
                FlexLayout.SetOrder(_btnClose, 4);
                _btnClose.IsVisible = true;
            }
            
            //var width = this.Width;
            //var height = this.Height;
            // Realiza acciones basadas en el cambio de tamaño de la página aquí
            // Puedes abrir o cerrar el Popup u hacer otros ajustes según sea necesario
            //Debug.Write("Girando!");
        }

        public void SetTitle(string NewTitle)
        {
            _labelTitle.Text = NewTitle;
        }

        public void SetSubtitle(string NewSubtitle)
        {
            _labelOverTitle.Text = NewSubtitle;
        }

        public void SetGridTitles(string Titles)
        {
            contentViewHeader.Columns = Titles;
        }

        public virtual void SetDataFields(string Fields)
        {            
            //Debug.WriteLine("CARGANDO CAMPOS!");
            scrollGridContent.Children.Clear();
            //scrollGridContent.HorizontalOptions = LayoutOptions.Fill;
            //scrollGridContent.BackgroundColor = Colors.WhiteSmoke;
            
            var fieldsItems = Fields.Split(",");

            for (int i = 0; i < fieldsItems.Length; i++)
            {
                var field = fieldsItems[i];
                AddDataColumn(scrollGridContent, i, field);
            }

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Colors.Transparent,
                IsVisible = true,
                Padding = new Thickness(5, 0, 0, 0)
            };

            Button btnSelectSwipeWindows = new Button
            {
                CornerRadius = 0,                
                Command = CommandSelectListItem,
                BackgroundColor = Colors.DeepSkyBlue,
                Text = "",
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

            btnSelectSwipeWindows.SetBinding(Button.CommandParameterProperty, new Binding("."));

            stackLayout.Children.Add(btnSelectSwipeWindows);

            scrollGridContent.Add(stackLayout, 6, 0);
            Grid.SetRow(stackLayout, 0);
            Grid.SetColumn(stackLayout, 6);
        }        

        async void _searchBar_BeginSearchBase(object sender, EventArgs e)
        {
            //Debug.WriteLine("_searchBar_BeginSearchBase");
            await KeyboardExtensions.HideKeyboardAsync(_searchBar, new CancellationToken());
            TextForSearch = _searchBar.Text;
            _LaunchSearchEvent(this, EventArgs.Empty);
        }

        public async Task SetWorkingStatus()
        {
            _searchBar.IsEnabled = false;
            _scrollView.IsVisible = false;
            await UITools.ShowLoading(_layoutLoading);
            _labelCount.Text = "Total registros 0";
            Debug.WriteLine("SetWorkingStatus");
        }

        public async Task SetDoneStatus()
        {
            _searchBar.IsEnabled = true;
            await UITools.HideLoading(_layoutLoading);
            _scrollView.IsVisible = true;

            if (_collectionViewSearch.ItemsSource != null)
            {
                var listItems = _collectionViewSearch.ItemsSource.Cast<object>();
                _labelCount.Text = "Total registros " + listItems.Count().ToString();
            }

            Debug.WriteLine("SetDoneStatus");
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

                _OnAppearing(this, EventArgs.Empty);

                timer.Stop();
            };
            timer.Start();
        }

        private void BuildTop(Grid gridContent)
        {
            var flexTop = new FlexLayout
            {
                //Direction = FlexDirection.Row,
                JustifyContent = FlexJustify.SpaceBetween,
                //HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = _colorTop,
                Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
                //HeightRequest = 100,
            };
            
            _searchBar = new SearchBar
            {
                Placeholder = "",
                //Margin = new Thickness(2, 0, 1, 2),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                //MinimumWidthRequest = 250,
                //HeightRequest = 50,
                BackgroundColor = new Color(230,230,230),
                //MaxLength = 250,
            };

            //_searchBar.TextChanged += _searchBar_OnTextChanged;
            _searchBar.SearchButtonPressed += _searchBar_BeginSearchBase;

            //Debug.WriteLine("BuildTopSmall");

            _stackLayoutToolBox1 = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(5, 0, 0, 0),
                //BackgroundColor = Colors.Green
            };

            _stackLayoutToolBox2 = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 0, 0, 0),
                //BackgroundColor = Colors.GhostWhite
            };

            Label _labelWithCredit = new Label
            {
                Text = "Saldos > 0",
                Margin = new Thickness(0, 0, 0, 0),
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
            };

            _switchWithCredit = new Microsoft.Maui.Controls.Switch
            {
                //Text = "Aceptar términos y condiciones"
                Margin = new Thickness(5, 0, 0, 3),                
                ThumbColor = Colors.White,
                OnColor = Colors.LimeGreen,
                VerticalOptions = LayoutOptions.Center,
            };

            _stackLayoutToolBox1.Children.Add(_labelWithCredit);
            _stackLayoutToolBox1.Children.Add(_switchWithCredit);

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
                Text = Title,
                //Margin = new Thickness(15, 15, 0, 15),
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
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
                //Text = "Regresar",
                BackgroundColor = Colors.WhiteSmoke,
                Margin = new Thickness(0),
                FontAttributes = FontAttributes.Bold,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.DimGray,
                    Size = 23,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf060"
                },
                CornerRadius = 0,
                //ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 0),                
                //HeightRequest = 40
                VerticalOptions = LayoutOptions.Center,
            };

            _btnBack.Clicked += OnBtnClose_Clicked;
            _btnClose.Clicked += OnBtnClose_Clicked;
            //_btnCloseSmall.Clicked += OnBtnClose_Clicked;

            var _stackLayoutButtonTop = new StackLayout
            {
                Margin = new Thickness(2,0,2,0),
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                //MinimumHeightRequest = 35,
                BackgroundColor = Colors.Red
            };

            _stackLayoutButtonTop.Children.Add(_btnBack);
            
            //FlexLayout.SetGrow(_stackLayoutButtonTop, 1);
            FlexLayout.SetOrder(_stackLayoutButtonTop, 1);
            //FlexLayout.SetAlignSelf(_btnBack, FlexAlignSelf.Center);
            flexTop.Children.Add(_stackLayoutButtonTop);
            
            var _stackLayoutLabels = new StackLayout
            {
                Margin = new Thickness(2, 0, 2, 0),
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                //MinimumHeightRequest = 40,
                //BackgroundColor = Colors.Blue
            };

            //FlexLayout.SetGrow(_stackLayoutLabels, 1);
            _stackLayoutLabels.Children.Add(_labelTitle);
            _stackLayoutLabels.Children.Add(_labelOverTitle);            

            flexTop.Children.Add(_stackLayoutLabels);
            //FlexLayout.SetGrow(_stackLayoutLabels, 1);
            FlexLayout.SetOrder(_stackLayoutLabels, 1);
            //Botón de cerrar en pantalla pequeña
            //flexTop.Children.Add(_btnCloseSmall);

            Border border = new Border
            {
                Stroke = Color.FromArgb("#C49B33"),
                //Background = Color.FromArgb("#2B0B98"),
                //Background = Colors.HotPink,
                StrokeThickness = 1,
                Padding = new Thickness(0, 0),
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(2, 2, 3, 2),
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5, 5, 5, 5)
                },
                Content = _searchBar
            };
                        
            FlexLayout.SetOrder(border, 3);
            
            bool isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

            if (isWindows)
            {
                flexTop.Margin = new Thickness(0, 25, 0, 0);
                FlexLayout.SetGrow(border, 1);
                FlexLayout.SetAlignSelf(border, FlexAlignSelf.Center);
            }
            
            FlexLayout.SetShrink(border, 0);
            flexTop.Children.Add(border);

            /************************************/
            flexTop.Children.Add(_stackLayoutToolBox1);
            FlexLayout.SetOrder(_stackLayoutToolBox1, 3);

            flexTop.Children.Add(_stackLayoutToolBox2);
            FlexLayout.SetOrder(_stackLayoutToolBox2, 4);
            /************************************/

            //flexTop.Children.Add(_btnClose);
            var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
            if (orientation == DisplayOrientation.Portrait)
            {
                //FlexLayout.SetOrder(_btnClose, 2);
                _btnClose.IsVisible = false;
            }
            else
            {
                FlexLayout.SetOrder(_btnClose, 5);
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
                Margin = new Thickness(0, 0, 0, 0),
                BackgroundColor = Colors.Khaki,
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

            _labelCount = new Label
            {
                Text = "Total registros 0",
                //Margin = new Thickness(15, 15, 0, 15),
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                TextColor = Colors.DarkGray,
                HorizontalOptions= LayoutOptions.End,
                Margin = new Thickness(0, 0, 5, 0),
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

            gridBottom.Children.Add(_labelCount);
            Grid.SetRow(_labelCount, 1);
            Grid.SetColumn(_labelCount, 4);

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

        public virtual void AddDataColumn(Grid scrollGridContent, int ColIndex, string ColName)
        {
            var lblColumnItem = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold,
            };
            
            lblColumnItem.SetBinding(Label.TextProperty, new Binding(ColName));

            var label1 = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 0, 15, 0)
            };

            scrollGridContent.Children.Add(lblColumnItem);
            Grid.SetRow(lblColumnItem, 0);
            Grid.SetColumn(lblColumnItem, ColIndex);
        }
        
        private CollectionView builCollectionViewModern()
        {
            return new CollectionView
            {
                //Hay que tener claro que cada vez que se crea un item se vuelve a renderizar
                // y se vuelven a cargar los child dentro del ItemTemplate
                ItemTemplate = new DataTemplate(() =>
                {
                    var swipeView = new SwipeView { IsClippedToBounds = true };

                    Button btnForSwipe = new Button
                    {
                        CornerRadius = 0,
                        Command = CommandSelectListItem,
                        BackgroundColor = Colors.DeepSkyBlue,
                        Text = "",
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

                    //btnSelectSwipeWindows.SetBinding(Button.CommandParameterProperty, new Binding("."));
                    btnForSwipe.SetBinding(Button.CommandParameterProperty, new Binding("."));

                    swipeView.RightItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;

                    swipeView.RightItems.Add(
                            new SwipeItemView
                            {
                                Content = btnForSwipe
                            }
                        );

                    scrollGridContent = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        BackgroundColor = Colors.WhiteSmoke,
                        RowDefinitions = new RowDefinitionCollection
                        {
                            new RowDefinition { Height = GridLength.Auto },
                        },
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Star }, //0
                            new ColumnDefinition { Width = GridLength.Star }, //1
                            new ColumnDefinition { Width = GridLength.Star }, //2
                            new ColumnDefinition { Width = GridLength.Star }, //3
                            new ColumnDefinition { Width = GridLength.Star }, //4
                            new ColumnDefinition { Width = GridLength.Star }, //5
                            new ColumnDefinition { Width = GridLength.Star }  //6
                        }
                    };

                    SetDataFields(DataField);

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
                    //swipeView.Open(OpenSwipeItem.RightItems, false);
                    //swipeView.Open(OpenSwipeItem.LeftItems, false);

                    return swipeView;
                })
            };
        }
        
        public virtual CollectionView builCollectionViewCustom()
        {
            //Se requiere implementar en la clase que hereda
            return new CollectionView
            {
                //Hay que tener claro que cada vez que se crea un item se vuelve a renderizar
                // y se vuelven a cargar los child dentro del ItemTemplate
                //ItemTemplate = new DataTemplate(() => CustomDataTemplate)
            };
        }

        private void BuildBody(Grid gridContent)
        {
            _layoutLoading = new AbsoluteLayout();
            _layoutLoading.HorizontalOptions = LayoutOptions.Center;
            _layoutLoading.VerticalOptions = LayoutOptions.Center;
            _layoutLoading.BackgroundColor = Colors.GhostWhite;

            CommandSelectListItem = new Command(SelectListItem);

            if (_renderCustomDataTemplate)
            {
                //if (CustomDataTemplate == null)
                //{
                //    CustomDataTemplate = _CustomDataTemplateFunc.Invoke();
                //}
                
                _collectionViewSearch = builCollectionViewCustom();
            }
            else
            {
                _collectionViewSearch = builCollectionViewModern();
            }

            _collectionViewSearch.BackgroundColor = Colors.DarkGray; //Color.FromUint(0xFF778899);

            _scrollView = new ScrollView
            {
                Content = _collectionViewSearch
            };

            //Label lblInfo = new Label() { Text = "Cabecera" };
            contentViewHeader = new HeaderLikeTable();
            contentViewHeader.Columns = "Col1, Col2";

            gridContent.Children.Add(contentViewHeader);
            Grid.SetRow(contentViewHeader, 2);
            Grid.SetColumn(contentViewHeader, 0);
            Grid.SetColumnSpan(contentViewHeader, 2);

            gridContent.Children.Add(_scrollView);
            Grid.SetRow(_scrollView, 3);
            Grid.SetColumn(_scrollView, 0);
            Grid.SetColumnSpan(_scrollView, 3);
            //Grid.SetRowSpan(_scrollView, 3);

            gridContent.Children.Add(_layoutLoading);
            Grid.SetRow(_layoutLoading, 3);
            Grid.SetColumn(_layoutLoading, 0);
            Grid.SetColumnSpan(_layoutLoading, 2);
            //Grid.SetRowSpan(_layoutLoading, 3);

            //_collectionViewSearch.ChildAdded += (sender, e) =>
            //{
            //    if (e.Element is SwipeView swipeView)
            //    {
            //        // Aquí puedes acceder al SwipeView y realizar operaciones
            //        // Por ejemplo, abrir el SwipeView automáticamente
            //        Debug.WriteLine(swipeView.RightItems.Count.ToString());
                    
            //        swipeView.Open(OpenSwipeItem.RightItems, true);                    
            //    }
            //};
        }

        public static ICommand CommandSelectListItem { get; set; }

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
            Close(null);
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

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            Console.WriteLine(propertyName);

            switch(propertyName)
            {
                case "ShowTextSearch":
                    {
                        _searchBar.IsVisible = ShowTextSearch;
                    }
                    break;
                case "ShowToolBox":
                    {
                        _stackLayoutToolBox1.IsVisible = ShowToolBox;
                    }
                    break;
                case "ContentCustomToolBox":
                    {
                        _stackLayoutToolBox2.Children.Add(ContentCustomToolBox);
                    }
                    break;
            }
        }
    }
}
