using BeebTech.Controls.UI;
using CommunityToolkit.Maui;
using DMCobranzas.Services;
using DMSA.Models.Security;
using DMSA.Sync.Core.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace DMCobranzas
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemiBold");                                        
                    fonts.AddFont("OpenSans-Medium.ttf", "sans-serif-medium");                    
                    fonts.AddFont("Font Awesome 5 Free-Regular-400.otf", "FontAwesome5Regular");
                    fonts.AddFont("Font Awesome 5 Brands-Regular-400.otf", "FontAwesome5Brands");
                    fonts.AddFont("Font Awesome 5 Free-Solid-900.otf", "FontAwesome5Solid");
                    fonts.AddFont("Consolas.ttf", "Consolas");
                    fonts.AddFont("Consoles.otf", "Consoles");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    //fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                })                
                .UseMauiCommunityToolkit(options => {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })                
                .UseBeebTechControls();

            App.Session = new AppSession();            

            App.Session.AppVersion = AppInfo.Current.VersionString;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            App.Session = new AppSession();

            App.Session.AppVersion = AppInfo.Current.VersionString;
            App.Session.SqliteCoreDbName = "_app";
            App.Session.AppCodeOdoo = "01";
            App.Session.AppMobileId = 1;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            DMSA.Sync.Core.Constants.Session = App.Session;

#if DEBUG
            builder.Logging.AddDebug();
#endif

            RegisterGlobalExceptionHandlers();

            return builder.Build();
        }

        private static void RegisterGlobalExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                Debug.WriteLine($"[UnhandledException] IsTerminating={args.IsTerminating} {ex?.Message}\n{ex?.StackTrace}");
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                Debug.WriteLine($"[UnobservedTaskException] {args.Exception?.Message}\n{args.Exception?.StackTrace}");
                args.SetObserved();
            };

#if ANDROID
            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, args) =>
            {
                Debug.WriteLine($"[AndroidUnhandledException] {args.Exception?.Message}\n{args.Exception?.StackTrace}");
                args.Handled = true;
            };
#endif
        }
    }
}