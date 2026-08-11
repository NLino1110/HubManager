using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;

namespace DMOrders.Controls.Tools
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
        }

        public static async Task HideLoading(AbsoluteLayout _absoluteLayout)
        {
            _loadingNow = false;
            _absoluteLayout.IsVisible = false;
        }

        private static DMOrders.Controls.PopupLoadingTask simplePopup = null;
        private static bool _isShowing = false;

        public static async Task ShowLoadingPopup(ContentPage contentPage)
        {
            if (_isShowing)
                return;

            _isShowing = true;

            if (simplePopup == null)
            {
                PopupSizeConstants popupSizeConstants =
                    new PopupSizeConstants(DeviceDisplay.Current);

                simplePopup = new DMOrders.Controls.PopupLoadingTask(popupSizeConstants);
                simplePopup.CanBeDismissedByTappingOutsideOfPopup = false;
            }

            if (Application.Current?.Windows[0] is not { Page: not null } window)
                throw new InvalidOperationException("Unable to find page");

            window.Page.ShowPopup(simplePopup, new PopupOptions
            {
                Shape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(10),
                    Stroke = Colors.Gray,
                    StrokeThickness = 2
                }
            });
        }

        public static async Task HideLoadingPopup()
        {
            var popup = simplePopup;
            _isShowing = false;
            simplePopup = null;

            if (popup == null)
                return;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await popup.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"HideLoadingPopup CloseAsync: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HideLoadingPopup: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza el texto del popup de loading.
        /// ANTES: simplePopup.SetNotify sin validar → NullReferenceException si el popup
        ///   no estaba creado o ya se había cerrado (HideLoading deja simplePopup = null).
        /// DESPUÉS: return si simplePopup es null.
        /// </summary>
        public static async Task SetNotifyLoadingPopup(string newNotify)
        {
            if (simplePopup == null)
                return;

            simplePopup.SetNotify(newNotify);
        }
    }
}

