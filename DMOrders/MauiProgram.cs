using CommunityToolkit.Maui;
using DMOrders.Services;
using DMSA.Models.Security;
using Microsoft.Extensions.Logging;
using MPowerKit.VirtualizeListView;
using SkiaSharp.Views.Maui.Controls.Hosting;
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
                .UseSkiaSharp()
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
            
            App.Session.AppVersion = AppInfo.Current.VersionString;
            App.Session.SqliteCoreDbName = "DMOrders_app";

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
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
