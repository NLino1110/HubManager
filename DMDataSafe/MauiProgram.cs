using BeebTech.Controls.UI;
using CommunityToolkit.Maui;
using DMDataSafe.Services;
using DMSA.Models.Security;
using DMSA.Sync.Core.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace DMDataSafe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 5 Free-Regular-400.otf", "FontAwesome5Regular");
                    fonts.AddFont("Font Awesome 5 Brands-Regular-400.otf", "FontAwesome5Brands");
                    fonts.AddFont("Font Awesome 5 Free-Solid-900.otf", "FontAwesome5Solid");
                })
                .UseMauiCommunityToolkit(options => {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .UseBeebTechControls();

            //builder.Services.AddCommunityToolkitDialogs();

            App.Session = new AppSession();
            App.Session.AppVersion = AppInfo.Current.VersionString;
            App.Session.SqliteCoreDbName = "DMDataSafe";
            App.Session.AppCodeOdoo = "03";
            App.Session.AppMobileId = 3;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            DMSA.Sync.Core.Constants.Session = App.Session;
#if DEBUG
            builder.Logging.AddDebug();
#endif

            //Debug.WriteLine(App.Session.odooConnection.IsProduction);

            ////Solo cuando se inicia en modo producción
            //if (App.Session.odooConnection.IsProduction)
            //{

            //}

            return builder.Build();
        }
    }
}