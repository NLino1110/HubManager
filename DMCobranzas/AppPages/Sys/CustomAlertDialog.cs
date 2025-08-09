using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Sample.Models;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;


namespace DMCobranzas.AppPages.Sys
{
    public partial class CustomAlertDialog : Popup, INotifyPropertyChanged
    {
        protected static Grid scrollGridContent { get; set; }

        public bool _boolShowButtonsFooter { get; set; } = false;

        public Color _colorTop = Colors.GhostWhite;
        public Color _colorBody = Colors.GhostWhite;
        public Color _colorBottom = Colors.GhostWhite;
        public Color _colorMain = Colors.GhostWhite;

        public StackLayout _stackLayoutBottom;
        public StackLayout _stackLayoutTop;
        private AbsoluteLayout _layoutLoading { get; set; }

        public Button _btnSave;
        public Button _btnCancel;
        public Button _btnClose;

        private Image imageAlert { get; set; }
        private Label subTitleLabel {  get; set; }
        private Label titleLabel { get; set; }

        public static readonly BindableProperty TitleBoxProperty = 
            BindableProperty.Create(nameof(TitleBox),
                typeof(string),
                typeof(PasswordPromptPage));

        public string TitleBox
        {
            get => (string) GetValue(TitleBoxProperty);
            set => SetValue(TitleBoxProperty, value);
        }

        public static readonly BindableProperty SubTitleBoxProperty =
            BindableProperty.Create(nameof(SubTitleBox),
                typeof(string),
                typeof(PasswordPromptPage));

        public string SubTitleBox
        {
            get => (string)GetValue(SubTitleBoxProperty);
            set => SetValue(SubTitleBoxProperty, value);
        }

        public CustomAlertDialog()
        {
            InitializeComponent();
        }
        
        public void InitializeComponent()
        {
            //CanBeDismissedByTappingOutsideOfPopup = false;
            //PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
            //popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            Size = new Size(400,160); // popupSizeConstants.SmallWide;
            Color = Colors.White;

            imageAlert = new Image
            {
                Source = "warning.png",
                WidthRequest = 50,
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(3),
                VerticalOptions = LayoutOptions.Center
            };

            titleLabel = new Label
            {
                BackgroundColor = Colors.Transparent,
                Text = TitleBox,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(5,0,5,0),
            };

            subTitleLabel = new Label
            {
                Text = SubTitleBox,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            
            var acceptButton = new Button
            {
                BackgroundColor = Colors.SeaGreen,
                Margin = new Thickness(1),
                Text = "OK",
                WidthRequest = 100,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            acceptButton.Clicked += (sender, e) => {
                //taskCompletionSource.SetResult(passwordEntry.Text);
                Close(null);
            };

            var cancelButton = new Button
            {
                BackgroundColor = Colors.Red,
                Margin = new Thickness(1),
                Text = "Cancelar",
                WidthRequest = 100,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = false
            };

            cancelButton.Clicked += (sender, e) => {
                //taskCompletionSource.SetResult(null);
                Close(null);
            };

            StackLayout stackLayout = new StackLayout
            {
                MinimumHeightRequest = Size.Height,
                MinimumWidthRequest= Size.Width,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(0),
                Children = {
                    titleLabel,
                    new StackLayout
                    {
                        Margin = new Thickness(10,10,0,0),
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        Children = { imageAlert, subTitleLabel, }
                    },
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Children = { acceptButton, cancelButton }
                    }
                },
                BackgroundColor = Colors.Transparent
            };

            Frame frameContent = new Frame
            {
                BackgroundColor = Colors.WhiteSmoke,
                Content = stackLayout,                
                BorderColor = Colors.Gray,
                CornerRadius = 10,
                Padding = new Thickness(10),
                HasShadow = true
            };
            
            Content = frameContent;
            Color = Colors.Transparent;
            //passwordEntry.Focus();            
        }

        private void BuildBody(Grid gridContent)
        {
            //_layoutLoading = new AbsoluteLayout();
            //_layoutLoading.HorizontalOptions = LayoutOptions.Center;
            //_layoutLoading.VerticalOptions = LayoutOptions.Center;
            //_layoutLoading.BackgroundColor = Colors.GhostWhite;

            //_scrollView = new ScrollView
            //{
            //    Content = _collectionViewSearch
            //};

            //gridContent.Children.Add(_scrollView);
            //Grid.SetRow(_scrollView, 3);
            //Grid.SetColumn(_scrollView, 0);
            //Grid.SetColumnSpan(_scrollView, 3);
            
            //gridContent.Children.Add(_layoutLoading);
            //Grid.SetRow(_layoutLoading, 3);
            //Grid.SetColumn(_layoutLoading, 0);
            //Grid.SetColumnSpan(_layoutLoading, 2);
        }

        private void BuildTopSmall(Grid gridContent)
        {
            var flexTop = new FlexLayout
            {
                //Direction = FlexDirection.Row,
                JustifyContent = FlexJustify.SpaceBetween,
                //HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = _colorTop,
                Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
                //JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start                
            };

            titleLabel = new Label
            {
                Text = TitleBox,
                //Margin = new Thickness(15, 15, 0, 15),
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                TextColor = Colors.SeaGreen,
            };

            subTitleLabel = new Label
            {
                Text = SubTitleBox,
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
                HorizontalOptions = LayoutOptions.Fill
            };

            //FlexLayout.SetGrow(_stackLayoutLabels, 1);
            _stackLayoutLabels.Children.Add(titleLabel);
            _stackLayoutLabels.Children.Add(subTitleLabel);

            flexTop.Children.Add(_stackLayoutLabels);
            FlexLayout.SetOrder(_stackLayoutLabels, 1);
            
            //Border border = new Border
            //{
            //    Stroke = Color.FromArgb("#C49B33"),                
            //    StrokeThickness = 1,
            //    Padding = new Thickness(0, 0),                
            //    Margin = new Thickness(2, 0, 1, 2),
            //    StrokeShape = new RoundRectangle
            //    {
            //        CornerRadius = new CornerRadius(5, 5, 5, 5)
            //    },
            //    Content = _searchBar
            //};

            //FlexLayout.SetGrow(border, 1);
            //FlexLayout.SetOrder(border, 3);
            //FlexLayout.SetAlignSelf(border, FlexAlignSelf.Center);
            //flexTop.Children.Add(border);

            flexTop.Children.Add(_btnClose);
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

        private async void OnBtnSave_Clicked(object sender, EventArgs e)
        {
            Close(null);
        }

        private void OnBtnCancel_Clicked(object sender, EventArgs e)
        {            
            Close(null);
        }

        private void OnBtnClose_Clicked(object sender, EventArgs e)
        {            
            Close(null);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "TitleBox":
                    {
                        titleLabel.Text = TitleBox;
                    }
                    break;
                case "SubTitleBox":
                    {
                        subTitleLabel.Text = SubTitleBox;
                    }
                    break;
                case "ContentCustomToolBox":
                    {
                        
                    }
                    break;
            }
        }
    }
}
