using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Window;
using DMOrders.Shared;
using ApplicationMaui = Microsoft.Maui.Controls.Application;

namespace DMOrders
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | 
        ConfigChanges.Orientation | 
        ConfigChanges.UiMode | 
        ConfigChanges.ScreenLayout | 
        ConfigChanges.SmallestScreenSize | 
        ConfigChanges.Density,
        ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : MauiAppCompatActivity
    {
////        protected override void OnCreate(Bundle savedInstanceState)
////        {
////            base.OnCreate(savedInstanceState);

////#if ANDROID33_0_OR_GREATER
////            // Android 13+ usa OnBackInvokedDispatcher
////            //OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(
////            //    1,
////            //    new BackCallback());
////#else
////            // Android 12 o menor
////            OnBackPressedDispatcher.OnBackPressed += () =>
////            {
////                HandleBackPressed();
////            };
////#endif
////        }
    }

    ////class BackCallback : Java.Lang.Object, IOnBackInvokedCallback
    ////{
    ////    public void OnBackInvoked()
    ////    {
    ////        MainThread.BeginInvokeOnMainThread(() =>
    ////        {
    ////            var application = IPlatformApplication.Current.Application as ApplicationMaui;
    ////            var mainPage = application?.Windows.FirstOrDefault()?.Page;

    ////            var currentPage = mainPage?.Navigation?.NavigationStack?.LastOrDefault();

    ////            if (currentPage is IBackButtonHandler handler)
    ////            {
    ////                handler.OnBackButtonPressedAsync();
    ////            }
    ////        });
    ////    }
    ////}
}

