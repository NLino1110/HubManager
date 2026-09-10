using CommunityToolkit.Maui.Core.Platform;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls.Shapes;
using DMOrders.Controls.Tools;
using CommunityToolkit.Maui.Views;
using DMSA.Sync.Core.Controls;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectBase<T> : Popup<T>, INotifyPropertyChanged
    {
        protected static Grid scrollGridContent { get; set; }
        private AbsoluteLayout _layoutLoading { get; set; }

        //private SearchBar _searchBar { get; set; }
        private Entry _searchBar { get; set; }
        private Border _searchBarBorder { get; set; }

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
        private Button _btnBack;
        private StackLayout _stackLayoutLabels;
        private VerticalStackLayout _compactTopBar;
        private VerticalStackLayout _compactSearchSection;

        public ScrollView _scrollView;
        public CollectionView _collectionViewSearch;
        HeaderLikeTable contentViewHeader;
        private Label _labelGridLegend;

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

        protected Entry SearchEntry => _searchBar;

        protected void SyncTextForSearchFromEntry()
        {
            if (_searchBar != null)
                TextForSearch = (_searchBar.Text ?? string.Empty).Trim();
        }

        protected void RunSearch() =>
            _searchBar_BeginSearchBase(_searchBar, EventArgs.Empty);

        protected async Task ShowPopupAlertAsync(string title, string message)
        {
            var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
            if (page != null)
                await page.DisplayAlertAsync(title, message, "Aceptar");
        }

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
                typeof(PopupSelectBase<T>), //Clase contenedora
                true);

        public bool ShowTextSearch
        {
            get => (bool)GetValue(ShowTextSearchProperty);
            set => SetValue(ShowTextSearchProperty, value);
        }

        public static readonly BindableProperty ShowToolBoxProperty =
            BindableProperty.Create(nameof(ShowToolBox),
                typeof(bool),
                typeof(PopupSelectBase<T>),
                true);

        public bool ShowToolBox
        {
            get => (bool)GetValue(ShowToolBoxProperty);
            set => SetValue(ShowToolBoxProperty, value);
        }

        public static readonly BindableProperty ContentCustomToolBoxProperty =
            BindableProperty.Create(nameof(ContentCustomToolBox),
                typeof(ContentView),
                typeof(PopupSelectBase<T>));

        public ContentView ContentCustomToolBox
        {
            get => (ContentView)GetValue(ContentCustomToolBoxProperty);
            set => SetValue(ContentCustomToolBoxProperty, value);
        }

        public static readonly BindableProperty ContentToolBox1Property =
            BindableProperty.Create(nameof(ContentToolBox1),
                typeof(ContentView),
                typeof(PopupSelectBase<T>));

        public ContentView ContentToolBox1
        {
            get => (ContentView)GetValue(ContentToolBox1Property);
            set => SetValue(ContentToolBox1Property, value);
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
            DesiredSize = popupSizeConstants.Large;
            BackgroundColor = _colorMain;
            
            // Agregar componentes al Grid
            var gridContent = new Grid
            {                
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
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

            popupSizeChanged += OnPageSizeChanged;

            timer_eventController = Dispatcher.CreateTimer();
            timer_eventController.IsRepeating = true;
            timer_eventController.Interval = TimeSpan.FromMilliseconds(500);
            timer_eventController.Tick += async (s, e) =>
            {
                if (Parent != null)
                {
                    //if (((ContentPage)Parent).Height != lastParentHeight)
                    //{
                    //    if(lastParentHeight > 0)
                    //    {
                    //        //Lanzar evento de Giro
                    //        popupSizeChanged(this, EventArgs.Empty);
                    //    }

                    //    lastParentHeight = ((ContentPage)Parent).Height;
                    //    lastParentWidth = ((ContentPage)Parent).Width;
                    //}
                }
            };
            timer_eventController.Start();
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
            DesiredSize = popupSizeConstants.Large;

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

        public void SetGridLegend(string legend)
        {
            if (_labelGridLegend == null)
                return;

            if (string.IsNullOrWhiteSpace(legend))
            {
                _labelGridLegend.IsVisible = false;
                _labelGridLegend.Text = string.Empty;
                return;
            }

            _labelGridLegend.Text = legend;
            _labelGridLegend.IsVisible = true;
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

        protected static bool UseCompactPopupTopLayout()
        {
            return DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
        }

        private View BuildCompactTopBar()
        {
            _searchBar.MinimumWidthRequest = 0;
            _searchBarBorder.HorizontalOptions = LayoutOptions.Fill;
            _searchBarBorder.VerticalOptions = LayoutOptions.Start;
            _searchBarBorder.Margin = new Thickness(0);

            _btnBack.WidthRequest = 44;
            _btnBack.HeightRequest = 44;
            _btnBack.Padding = new Thickness(8);
            _btnBack.BackgroundColor = Colors.Transparent;

            _stackLayoutLabels.Margin = new Thickness(0);
            _labelTitle.FontSize = 15;
            _labelOverTitle.FontSize = 12;

            var headerRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                },
                ColumnSpacing = 6,
            };
            headerRow.Add(_btnBack, 0, 0);
            headerRow.Add(_stackLayoutLabels, 1, 0);

            _stackLayoutToolBox1.HorizontalOptions = LayoutOptions.Fill;
            _stackLayoutToolBox2.HorizontalOptions = LayoutOptions.Fill;
            _stackLayoutToolBox1.Margin = new Thickness(0);
            _stackLayoutToolBox2.Margin = new Thickness(0);

            _compactSearchSection = new VerticalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
            };
            _compactSearchSection.Children.Add(_searchBarBorder);

            _compactTopBar = new VerticalStackLayout
            {
                Spacing = 8,
                Padding = new Thickness(10, 12, 10, 8),
                BackgroundColor = _colorTop,
            };

            _compactTopBar.Children.Add(headerRow);
            _compactTopBar.Children.Add(_compactSearchSection);
            return _compactTopBar;
        }

        protected Button CreateSearchButton(Action onSearch)
        {
            var btnSearch = new Button
            {
                Text = "Buscar",
                BackgroundColor = Colors.SeaGreen,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0),
                MinimumHeightRequest = 44,
                HorizontalOptions = LayoutOptions.Fill,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 18,
                    Glyph = "\uf002"
                }
            };
            btnSearch.Clicked += (_, _) => onSearch();
            return btnSearch;
        }

        protected ContentView WrapSearchButton(Action onSearch) =>
            new ContentView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                Content = CreateSearchButton(onSearch)
            };

        private void ApplyMobileToolbarContent()
        {
            if (UseCompactPopupTopLayout() && _compactSearchSection != null && _compactTopBar != null)
            {
                while (_compactSearchSection.Children.Count > 1)
                    _compactSearchSection.Children.RemoveAt(_compactSearchSection.Children.Count - 1);

                while (_compactTopBar.Children.Count > 2)
                    _compactTopBar.Children.RemoveAt(_compactTopBar.Children.Count - 1);

                if (ContentCustomToolBox != null)
                {
                    StyleMobileSearchToolBox(ContentCustomToolBox);
                    _compactSearchSection.Children.Add(ContentCustomToolBox);
                }

                if (ContentToolBox1 != null)
                {
                    StyleMobileActionToolBox(ContentToolBox1);
                    _compactTopBar.Children.Add(ContentToolBox1);
                }

                return;
            }

            _stackLayoutToolBox2.Children.Clear();
            if (ContentCustomToolBox != null)
                _stackLayoutToolBox2.Children.Add(ContentCustomToolBox);

            _stackLayoutToolBox1.Children.Clear();
            if (ContentToolBox1 != null)
                _stackLayoutToolBox1.Children.Add(ContentToolBox1);

            _stackLayoutToolBox1.IsVisible = _stackLayoutToolBox1.Children.Count > 0;
            _stackLayoutToolBox2.IsVisible = _stackLayoutToolBox2.Children.Count > 0;
        }

        private static void StyleMobileSearchToolBox(ContentView contentView)
        {
            contentView.HorizontalOptions = LayoutOptions.Fill;
            contentView.VerticalOptions = LayoutOptions.Start;
            contentView.MinimumHeightRequest = 44;
            contentView.MaximumHeightRequest = 44;

            if (contentView.Content is Button searchButton)
                ConfigureMobileSearchButton(searchButton);
        }

        private static void StyleMobileActionToolBox(ContentView contentView)
        {
            contentView.HorizontalOptions = LayoutOptions.Fill;
            contentView.VerticalOptions = LayoutOptions.Start;

            if (contentView.Content is StackLayout horizontal &&
                horizontal.Orientation == StackOrientation.Horizontal)
            {
                var buttons = horizontal.Children.OfType<Button>().ToList();
                if (buttons.Count != 2)
                    return;

                horizontal.Children.Clear();
                horizontal.Spacing = 0;

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Star),
                    },
                    ColumnSpacing = 8,
                    HorizontalOptions = LayoutOptions.Fill,
                };

                ConfigureMobileActionButton(buttons[0]);
                ConfigureMobileActionButton(buttons[1]);
                grid.Add(buttons[0], 0, 0);
                grid.Add(buttons[1], 1, 0);
                horizontal.Children.Add(grid);
            }
        }

        private static void ConfigureMobileSearchButton(Button btn)
        {
            btn.HorizontalOptions = LayoutOptions.Fill;
            btn.VerticalOptions = LayoutOptions.Start;
            btn.MinimumHeightRequest = 44;
            btn.Margin = new Thickness(0);
        }

        private static void ConfigureMobileActionButton(Button btn)
        {
            btn.HorizontalOptions = LayoutOptions.Fill;
            btn.MinimumHeightRequest = 44;
            btn.Margin = new Thickness(0);
            btn.Padding = new Thickness(6, 8);
            btn.FontSize = 11;
            btn.LineBreakMode = LineBreakMode.WordWrap;
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

            //_searchBar = new SearchBar
            //{
            //    Placeholder = "",
            //    //Margin = new Thickness(2, 0, 1, 2),
            //    HorizontalOptions = LayoutOptions.Fill,
            //    VerticalOptions = LayoutOptions.Center,
            //    //MinimumWidthRequest = 250,
            //    //HeightRequest = 50,
            //    BackgroundColor = new Color(230,230,230),
            //    //MaxLength = 250,
            //    SearchIconColor = Colors.Transparent,                
            //};

            _searchBar = new Entry
            {
                Placeholder = "",
                //Margin = new Thickness(2, 0, 1, 2),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                MinimumWidthRequest = 270,
                //HeightRequest = 50,
                BackgroundColor = new Color(230, 230, 230),
                //MaxLength = 250,
                ReturnType = ReturnType.Search
            };

            //_searchBar.TextChanged += _searchBar_OnTextChanged;
            //_searchBar.SearchButtonPressed += _searchBar_BeginSearchBase;
            _searchBar.Completed += _searchBar_BeginSearchBase;

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
                        
            //_stackLayoutToolBox1.Children.Add(_switchWithCredit);

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

            _btnBack = new Button
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
                Margin = new Thickness(2, 0, 2, 0),
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };

            _stackLayoutLabels = new StackLayout
            {
                Margin = new Thickness(2, 0, 2, 0),
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
            };

            _stackLayoutLabels.Children.Add(_labelTitle);
            _stackLayoutLabels.Children.Add(_labelOverTitle);

            _searchBarBorder = new Border
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
                Content = _searchBar,
                IsVisible = ShowTextSearch
            };
                        
            var compactTop = UseCompactPopupTopLayout();
            View topBarContent;

            if (compactTop)
            {
                topBarContent = BuildCompactTopBar();
            }
            else
            {
                _stackLayoutButtonTop.Children.Add(_btnBack);
                FlexLayout.SetOrder(_stackLayoutButtonTop, 1);
                flexTop.Children.Add(_stackLayoutButtonTop);

                flexTop.Children.Add(_stackLayoutLabels);
                FlexLayout.SetOrder(_stackLayoutLabels, 1);

                FlexLayout.SetOrder(_searchBarBorder, 3);

                bool isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;
                if (isWindows)
                {
                    flexTop.Margin = new Thickness(0, 25, 0, 0);
                    FlexLayout.SetGrow(_searchBarBorder, 1);
                    FlexLayout.SetAlignSelf(_searchBarBorder, FlexAlignSelf.Center);
                }

                FlexLayout.SetShrink(_searchBarBorder, 0);
                flexTop.Children.Add(_searchBarBorder);
                flexTop.Children.Add(_stackLayoutToolBox1);
                FlexLayout.SetOrder(_stackLayoutToolBox1, 3);
                flexTop.Children.Add(_stackLayoutToolBox2);
                FlexLayout.SetOrder(_stackLayoutToolBox2, 4);

                var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
                _btnClose.IsVisible = orientation != DisplayOrientation.Portrait;
                if (_btnClose.IsVisible)
                    FlexLayout.SetOrder(_btnClose, 5);

                topBarContent = flexTop;
            }

            var stackLayoutTopInner = new StackLayout
            {
                Margin = new Thickness(0, 0, 0, 0)
            };

            var borderBottom = new BoxView
            {
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill,
                Color = new Microsoft.Maui.Graphics.Color(100, 100, 100, 50)
            };

            stackLayoutTopInner.Children.Add(topBarContent);

            _stackLayoutTop = new StackLayout
            {
                Children = { stackLayoutTopInner, borderBottom },
                Margin = new Thickness(0),
                BackgroundColor = _colorTop,
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
            Grid.SetRow(_stackLayoutBottom, 7);
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

            _collectionViewSearch.BackgroundColor = Colors.WhiteSmoke; //Color.FromUint(0xFF778899);

            _scrollView = new ScrollView
            {
                Content = _collectionViewSearch
            };

            //Label lblInfo = new Label() { Text = "Cabecera" };
            contentViewHeader = new HeaderLikeTable();
            contentViewHeader.Columns = "-";
            if (UseCompactPopupTopLayout())
                contentViewHeader.Margin = new Thickness(0, 2, 0, 0);

            gridContent.Children.Add(contentViewHeader);
            Grid.SetRow(contentViewHeader, UseCompactPopupTopLayout() ? 1 : 2);
            Grid.SetColumn(contentViewHeader, 0);
            Grid.SetColumnSpan(contentViewHeader, 2);

            _labelGridLegend = new Label
            {
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.DimGray,
                Margin = new Thickness(10, 0, 10, 4),
                IsVisible = false,
                LineBreakMode = LineBreakMode.WordWrap
            };

            gridContent.Children.Add(_labelGridLegend);
            Grid.SetRow(_labelGridLegend, 3);
            Grid.SetColumn(_labelGridLegend, 0);
            Grid.SetColumnSpan(_labelGridLegend, 3);

            gridContent.Children.Add(_scrollView);
            Grid.SetRow(_scrollView, 4);
            Grid.SetColumn(_scrollView, 0);
            Grid.SetColumnSpan(_scrollView, 3);
            //Grid.SetRowSpan(_scrollView, 3);

            gridContent.Children.Add(_layoutLoading);
            Grid.SetRow(_layoutLoading, 4);
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
            if (objItem is T item) // aquí validas y conviertes
            {
                await CloseAsync(item);
            }
            else
            {
                Debug.WriteLine("Error: el objeto no es del tipo esperado.");
            }
        }

        private async void OnBtnSave_Clicked(object sender, EventArgs e)
        {
            await CloseAsync(default(T));
        }

        private void OnBtnCancel_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            CloseAsync(default(T));
        }

        private void OnBtnClose_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            CloseAsync(default(T));
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
                        if (_searchBarBorder != null)
                            _searchBarBorder.IsVisible = ShowTextSearch;
                    }
                    break;
                case "ShowToolBox":
                    {
                        _stackLayoutToolBox1.IsVisible = ShowToolBox;
                    }
                    break;
                case "ContentCustomToolBox":
                case "ContentToolBox1":
                    ApplyMobileToolbarContent();
                    break;
            }
        }
    }
}
