using CommunityToolkit.Maui;
using DMOrders.Services;
using DMSA.Models.Security;
using Microsoft.Extensions.Logging;
using MPowerKit.VirtualizeListView;
using System.Diagnostics;
using UraniumUI;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace DMOrders
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMPowerKitListView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    fonts.AddFont("OpenSans-Medium.ttf", "sans-serif-medium");

                    fonts.AddFont("Font Awesome 5 Free-Regular-400.otf", "FontAwesome5Regular");
                    fonts.AddFont("Font Awesome 5 Brands-Regular-400.otf", "FontAwesome5Brands");
                    fonts.AddFont("Font Awesome 5 Free-Solid-900.otf", "FontAwesome5Solid");
                    fonts.AddFont("Consolas.ttf", "Consolas");                    
                    fonts.AddFontAwesomeIconFonts();
                    fonts.AddMaterialSymbolsFonts();
                    fonts.AddMaterialIconFonts();
                    fonts.AddFluentIconFonts();
                })
                .UseUraniumUIBlurs()
                .UseMauiCommunityToolkit()
                .UseUraniumUI()
                .UseUraniumUIMaterial();

            builder.Services.AddCommunityToolkitDialogs();

            App.Session = new AppSession();
            Debug.WriteLine(App.Session.isProduction);

            App.Session.AppVersion = AppInfo.Current.VersionString;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            App.Session.EndPointServer = "http://192.168.56.1:8069";

            //Solo cuando se inicia en modo producción
            if (App.Session.isProduction)
            {
                App.Session.EndPointServer = App.Session.EndPointServerProd;
            }

#if DEBUG
            builder.Logging.AddDebug();
#endif

            //App.PushRelayGlobal = new PushRelay();
            //App.PushRelayGlobal.Name = "---";
            //App.PushRelayGlobal.Message = "ConnectCommand";

            return builder.Build();
        }
    }
}
