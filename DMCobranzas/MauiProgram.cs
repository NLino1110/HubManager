using DMCobranzas.Services;
using CommunityToolkit.Maui;
using DMSA.Models.Security;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UraniumUI;

namespace DMCobranzas
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
//            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
//            {
//#if ANDROID
//            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
//#endif
//            });

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemiBold");
                    
                    //Resuelve error en Android
                    fonts.AddFont("OpenSans-Medium.ttf", "sans-serif-medium");
                    
                    fonts.AddFont("Font Awesome 5 Free-Regular-400.otf", "FontAwesome5Regular");
                    fonts.AddFont("Font Awesome 5 Brands-Regular-400.otf", "FontAwesome5Brands");
                    fonts.AddFont("Font Awesome 5 Free-Solid-900.otf", "FontAwesome5Solid");
                    fonts.AddFont("Consolas.ttf", "Consolas");
                    fonts.AddFont("Consoles.otf", "Consoles");
                })
                .UseUraniumUIBlurs()
                .UseMauiCommunityToolkit()
                .UseUraniumUI()
                .UseUraniumUIMaterial();

            App.Session = new AppSession();
            //Debug.WriteLine(App.Session.odooConnection.IsProduction);

            App.Session.AppVersion = AppInfo.Current.VersionString;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            //if (DeviceInfo.Platform == DevicePlatform.WinUI)
            //{
            //    App.Session.AppVersion = AppInfo.Current.Version.Major.ToString() + "." +
            //        AppInfo.Current.Version.Minor.ToString() + "." +
            //        AppInfo.Current.Version.Build.ToString() + ".";
            //}

            App.Session = new AppSession();

            App.Session.AppVersion = AppInfo.Current.VersionString;
            App.Session.SqliteCoreDbName = "_app";

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            DMSA.Sync.Core.Constants.Session = App.Session;

#if DEBUG
            builder.Logging.AddDebug();
#endif

            App.PushRelayGlobal = new PushRelay();
            App.PushRelayGlobal.Name = "---";
            App.PushRelayGlobal.Message = "ConnectCommand";

            //App.PushRelayGlobal.ConnectCommand.Execute("");
            //var sendcmd = App.PushRelayGlobal.SendMessageCommand;
            //sendcmd.Execute("");
           
            //App.PushRelayGlobal.ConnectCommand.ExecuteAsync("").Wait();
            //var sendcmd = App.PushRelayGlobal.SendMessageCommand;
            //sendcmd.ExecuteAsync("OOLLLL").Wait();

            return builder.Build();
        }
    }
}