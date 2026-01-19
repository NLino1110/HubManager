using BeebTech.Controls.UI;
using CommunityToolkit.Maui;
using DMCobranzas.Services;
using DMSA.Models.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using System.Diagnostics;
using UraniumUI;

//////#if ANDROID
//////using Microsoft.Maui.Handlers;
//////using Android.App;
//////using Android.Content;
//////using Android.Views;
//////#endif


namespace DMCobranzas
{
    public static class MauiProgram
    {
//////#if ANDROID
//////        class TouchBlocker : Java.Lang.Object, Android.Views.View.IOnTouchListener
//////        {
//////            public bool OnTouch(Android.Views.View v, MotionEvent e)
//////            {
//////                // Return true = consumimos el evento → MAUI no abre su popup
//////                return true; // permite el Click pero bloquea el popup interno
//////            }
//////        }
//////#endif

        public static MauiApp CreateMauiApp()
        {
//            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
//            {
//#if ANDROID
//            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
//#endif
//            });

            var builder = MauiApp.CreateBuilder();
//////#if ANDROID
//////            PickerHandler.Mapper.AppendToMapping("HighlightFix", (handler, view) =>
//////            {
//////                var platformView = handler.PlatformView;
//////                var virtualView = handler.VirtualView;

//////                // ❌ Evita que aparezca el popup nativo de MAUI
//////                platformView.ShowSoftInputOnFocus = false;
//////                platformView.Focusable = true;
//////                platformView.FocusableInTouchMode = true;

//////                // Evita que el Picker nativo abra su selector
//////                platformView.Clickable = true;
//////                platformView.LongClickable = false;

//////                // Interceptar Touch para evitar que MAUI abra su popup
//////                platformView.SetOnTouchListener(new TouchBlocker());

//////                // Nuestro popup custom
//////                platformView.Click += (sender, e) =>
//////                {
//////                    var items = virtualView.Items;
//////                    if (items == null || items.Count == 0)
//////                        return;

//////                    var context = platformView.Context;
//////                    var dialog = new AlertDialog.Builder(context);

//////                    dialog.SetSingleChoiceItems(
//////                        items.ToArray(),
//////                        virtualView.SelectedIndex,
//////                        (s, args) =>
//////                        {
//////                            virtualView.SelectedIndex = args.Which;
//////                            ((AlertDialog)s).Dismiss();
//////                        });

//////                    dialog.SetTitle(virtualView.Title);
//////                    dialog.Show();
//////                };
//////            });
//////#endif

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
                //.UseUraniumUIBlurs()
                .UseMauiCommunityToolkit()
                .UseUraniumUI()
                //.UseUraniumUIMaterial()
                .UseBeebTechControls();

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