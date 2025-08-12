//namespace DMOrders;

//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.Formats.Tar;
//using Maui.BindableProperty.Generator.Core;
//using Microsoft.Maui.Controls.Shapes;

//public partial class Tab : View
//{
//    public BoxView activeLine = new BoxView
//    {
//        HeightRequest = 2,
//        BackgroundColor = Colors.Blue,
//        HorizontalOptions = LayoutOptions.FillAndExpand,
//        IsVisible = false
//    };

//    [AutoBindable(DefaultValue = "true")]
//    private bool isActive;

//    [AutoBindable]
//    private ImageSource? icon;

//    [AutoBindable]
//    private string title = string.Empty;

//    [AutoBindable(DefaultValue = "new ContentView()")]
//    private IView content = new ContentView();

//    public IView BuildTabHeader(Color borderColor)
//    {
//        var tabHeader = new VerticalStackLayout()
//        {
//            BackgroundColor = Colors.Transparent,
//            VerticalOptions = LayoutOptions.FillAndExpand,            
//        };

//        var tabContent = new StackLayout
//        {            
//            Orientation = StackOrientation.Horizontal,
//            HorizontalOptions = LayoutOptions.FillAndExpand,
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            //Spacing = 5
//        };       

//        tabContent.Children.Add(new Image() { Source = Icon, 
//            WidthRequest = 20, 
//            HeightRequest = 20,
//            Margin = new Thickness(0,0,3,0),
//            VerticalOptions = LayoutOptions.Center 
//        });
//        tabContent.Children.Add(new Label() { 
//            Text = Title.ToUpper(), 
//            FontSize = 12, 
//            FontFamily = "Consoles", 
//            FontAttributes = FontAttributes.Bold, 
//            VerticalOptions = LayoutOptions.Center,            
//        });

//        tabHeader.Children.Add(activeLine);

//        Border border = new Border
//        {
//            Content = tabContent,
//            Stroke = borderColor,
//            StrokeThickness = 0.3,
//            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6, 6, 0, 0) },
//            Padding = new Thickness(5, 10, 5, 5),
//            Background = Colors.Transparent,
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            HeightRequest = 55 //TODO: Alto estatico ya que no se ajusta automaticamente
//        };

//        tabHeader.Children.Add(border);

//        // Agregar borde en la parte inferior
//        //tabHeader.Children.Add(new BoxView()
//        //{
//        //    HeightRequest = 1,
//        //    BackgroundColor = borderColor,
//        //    HorizontalOptions = LayoutOptions.Fill
//        //});
                
//        return tabHeader;
//    }
//}

//public partial class TabView : VerticalStackLayout
//{
//    public event EventHandler<int>? ActiveTabChanged;

//    [AutoBindable(DefaultValue = "new System.Collections.ObjectModel.ObservableCollection<Tab>()", OnChanged = "OnTabsChanged")]
//    private ObservableCollection<Tab> tabs = new();

//    [AutoBindable(DefaultValue = "-1", OnChanged = "OnActiveTabIndexChanged")]
//    private int activeTabIndex;

//    [AutoBindable(DefaultValue = "Colors.Yellow")]
//    private Color tabBackgroundColor;

//    [AutoBindable(DefaultValue = "Colors.Orange")]
//    private Color activeTabBackgroundColor;

//    [AutoBindable(DefaultValue = "LayoutOptions.Center")] // Posición de los tabs (izquierda, centro, derecha)
//    private LayoutOptions tabAlignment;

//    [AutoBindable(DefaultValue = "StackOrientation.Vertical")] // Icono arriba o a la izquierda del texto
//    private StackOrientation tabContentOrientation;

//    private readonly Dictionary<int, VerticalStackLayout> _tabHeaders = new();

//    void OnTabsChanged()
//    {
//        Children.Clear();
//        //Children.Add(BuildLogoAndTitle());
//        Children.Add(BuildTabs());

//        OnActiveTabIndexChanged();
//        ActiveTabIndex = Tabs.Count > 0 ? 0 : -1;
//    }

//    public TabView()
//    {
//        Loaded += TabView_Loaded;
//    }

//    private void TabView_Loaded(object? sender, EventArgs e)
//    {
//        OnTabsChanged();
//    }

//    void OnActiveTabIndexChanged()
//    {
//        foreach (var kvp in _tabHeaders)
//        {
//            var index = kvp.Key;
//            var tabHeader = kvp.Value;

//            if (index == ActiveTabIndex)
//            {
//                AnimateTabSelection(tabHeader);
//                tabHeader.BackgroundColor = ActiveTabBackgroundColor;
//            }
//            else
//            {
//                tabHeader.BackgroundColor = TabBackgroundColor;
//                tabHeader.Scale = 1;
//            }
//        }

//        var activeTab = GetActiveTab();
//        if (activeTab is null)
//        {
//            return;
//        }

//        if (Children.Count == 1)
//        {
//            Children.Add(activeTab);
//        }
//        else
//        {
//            Children[1] = activeTab;
//        }

//        ActiveTabChanged?.Invoke(this, ActiveTabIndex);

//    }

//    IView BuildLogoAndTitle()
//    {
//        var logoAndTitle = new HorizontalStackLayout
//        {
//            HorizontalOptions = LayoutOptions.Center,
//            Spacing = 3,
//            Padding = new Thickness(2, 2, 2, 2),
//            Margin = new Thickness(10, 0, 20, 0)
//        };

//        var logoImage = new Image
//        {
//            Source = "dotnet_bot.png",
//            WidthRequest = 45,
//            HeightRequest = 45
//        };

//        var textContainer = new VerticalStackLayout
//        {
//            VerticalOptions = LayoutOptions.Center
//        };

//        var titleLabel = new Label
//        {
//            Text = "Empresa Empresa Empresa S.A.",
//            FontSize = 15,
//            FontAttributes = FontAttributes.Bold
//        };

//        var subTitleLabel = new Label
//        {
//            Text = "Agencia",
//            FontSize = 12,
//            TextColor = Colors.Gray
//        };

//        textContainer.Children.Add(titleLabel);
//        textContainer.Children.Add(subTitleLabel);

//        logoAndTitle.Children.Add(logoImage);
//        logoAndTitle.Children.Add(textContainer);

//        titleLabel.Text = App.Session.res_Company.name;
//        subTitleLabel.Text = App.Session.res_Store.name;

//        return logoAndTitle;
//    }

//    IView BuildTabs()
//    {
//        var view = new HorizontalStackLayout()
//        {
//            HorizontalOptions = LayoutOptions.FillAndExpand,
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            Spacing = 0,
//        };

//        _tabHeaders.Clear();
//        view.Children.Add(BuildLogoAndTitle());

//        for (var index = 0; index < Tabs.Count; index++)
//        {
//            var tab = Tabs[index];
//            var index1 = index;
            
//            var tabHeader = tab.BuildTabHeader(Colors.Gray);
            
//            var tapGesture = new TapGestureRecognizer()
//            {
//                Command = new Command(() => ActiveTabIndex = index1)
//            };

//            if (tabHeader is Layout layout)
//            {
//                layout.GestureRecognizers.Add(tapGesture);
//            }

//            _tabHeaders[index] = (VerticalStackLayout)tabHeader;

//            //Grid.SetRowSpan(border, 2);
//            view.Children.Add(tabHeader);
//        }

//        var view_main = new HorizontalStackLayout()
//        {
//            HorizontalOptions = LayoutOptions.FillAndExpand,
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            Spacing = 3,
//            WidthRequest = double.NaN
//        };

//        Border border = new Border
//        {
//            Content = view,
//            Stroke = Colors.Gray,
//            StrokeThickness = 0.5,
//            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 0, 0) },
//            Padding = new Thickness(0, 0, 0, 0),
//            Background = Colors.Transparent,
//            VerticalOptions = LayoutOptions.FillAndExpand,
//            HorizontalOptions = LayoutOptions.FillAndExpand,
//            WidthRequest = double.NaN
//        };
                
//        view_main.Children.Add(border);

//        return view_main;
//    }

//    IView? GetActiveTab()
//    {
//        if (Tabs.Count < ActiveTabIndex || ActiveTabIndex < 0)
//        {
//            return null;
//        }

//        foreach(var tabItem in Tabs)
//        {
//            tabItem.activeLine.IsVisible = false;
//        }

//        var activeTab = Tabs[ActiveTabIndex];

//        activeTab.activeLine.IsVisible = true;

//        return activeTab.Content;
//    }

//    private async void AnimateTabSelection(VerticalStackLayout tabHeader)
//    {
//        await tabHeader.ScaleTo(1.1, 100, Easing.CubicInOut);
//        await tabHeader.ScaleTo(1, 100, Easing.CubicInOut);
//    }
//}
