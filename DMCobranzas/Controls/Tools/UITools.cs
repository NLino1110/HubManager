using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Controls.Tools
{
    public static class UITools
    {
        private static bool _loadingNow = false;

        public static bool LoadingNow()
        {
            return _loadingNow;
        }

        public static async Task ShowLoading(AbsoluteLayout _absoluteLayout)
        {
            ActivityIndicator _activityIndicator = new ActivityIndicator();

            if (_absoluteLayout.Children.Count == 0)
            {
                var grid = new Grid
                {
                    Padding = new Thickness(1),
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                },
                    RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                }
                };

                var label = new Label
                {
                    Text = "Procesando, espere un momento...",
                    FontSize = 13,
                    TextColor = Colors.DimGray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(20, 20, 20, 20),
                    FontAttributes = FontAttributes.Bold
                };

                _activityIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Colors.Red,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(20, 20, 20, 20),
                };

                //_absoluteLayout.Children.Add(content);
                //_absoluteLayout.Children.Add(_activityIndicator);

                grid.Add(label);
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, 0);

                grid.Add(_activityIndicator);
                Grid.SetColumn(_activityIndicator, 0);
                Grid.SetRow(_activityIndicator, 1);

                _absoluteLayout.Children.Add(grid);
            }

            _loadingNow = true;
            _absoluteLayout.IsVisible = true;
            //_absoluteLayout.HeightRequest = 300;
        }

        public static async Task HideLoading(AbsoluteLayout _absoluteLayout)
        {
            _loadingNow = false;
            //_absoluteLayout.Children.Clear();
            _absoluteLayout.IsVisible = false;
        }

        private static DMCobranzas.Controls.PopupLoadingTask simplePopup = null;

        public static async Task ShowLoadingPopup(ContentPage contentPage)
        {
            PopupSizeConstants popupSizeConstants = 
                new PopupSizeConstants(DeviceDisplay.Current);
            simplePopup = new DMOrders.Controls.PopupLoadingTask(popupSizeConstants);
            simplePopup.CanBeDismissedByTappingOutsideOfPopup = false;            
            //contentPage.ShowPopup(simplePopup);

            if (Application.Current?.Windows[0] is not { Page: not null } window)
            {
                throw new InvalidOperationException("Unable to find page");
            }
            
            //simplePopup.Margin = new Thickness(0,0);
            //simplePopup.Padding = new Thickness(0,0);
            
            window.Page.ShowPopup(simplePopup, new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(10),
                    Stroke = Colors.Gray,
                    StrokeThickness = 2
                },
                Shadow = new Shadow
                {
                    Brush = Brush.Black,
                    Offset = new Point(15, 15),
                    Opacity = 0.5f,
                    Radius = 10
                },
            });
        }

        public static async Task HideLoadingPopup()
        {
            if (simplePopup != null)
            {                
                await simplePopup.CloseAsync();
            }
        }

        public static async Task SetNotifyLoadingPopup(string newNotify)
        {
            simplePopup.SetNotify(newNotify);
        }
    }
}
