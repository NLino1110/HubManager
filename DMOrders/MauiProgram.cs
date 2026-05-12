using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using DMOrders.Services;
using DMOrders.Services.Update;
using DMSA.Models.Security;
using DMSA.Sync.Core.Services;
using Microsoft.Extensions.Logging;
using MPowerKit.VirtualizeListView;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Diagnostics;
using System.Globalization;
using UraniumUI;

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
                .UseMauiCommunityToolkit(options => {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .UseMauiCommunityToolkitMarkup()
                .UseMauiCommunityToolkitCamera()
                .UseUraniumUI()
                .UseUraniumUIMaterial();

            builder.Services.AddCommunityToolkitDialogs();

            App.Session = new AppSession();
            App.Session.AppVersion = AppInfo.Current.VersionString;
            App.Session.SqliteCoreDbName = "DMOrders_app";
            App.Session.AppCodeOdoo = "02";
            App.Session.AppMobileId = 2;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            DMSA.Sync.Core.Constants.Session = App.Session;
#if DEBUG
            builder.Logging.AddDebug();
#endif

            // CONFIGURACIÓN DE CULTURA
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.CurrencyDecimalSeparator = ".";
            culture.NumberFormat.PercentGroupSeparator = ",";
            culture.NumberFormat.NumberGroupSeparator = ",";
            culture.NumberFormat.CurrencyGroupSeparator = ",";

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            return builder.Build();
        }
    }
}

