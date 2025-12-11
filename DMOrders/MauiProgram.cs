using CommunityToolkit.Maui;
using DMOrders.Services;
using DMSA.Models.Security;
using Microsoft.Extensions.Logging;
using MPowerKit.VirtualizeListView;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Diagnostics;
using System.Globalization;
using UraniumUI;
using DMOrders.Services.Database.Sqlite;    // ← IMPORTANTE

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


            // 🔥 ----------------------------------------
            // REGISTRAR SERVICIO SQLITE
            // ----------------------------------------
            builder.Services.AddSingleton<StockLocationDb>(provider =>
            {
                return new StockLocationDb(App.Session.SqliteCoreDbName);
            });


            // 🔥 ----------------------------------------
            // WARMUP PARA ELIMINAR LENTITUD DE STOCK
            // ----------------------------------------
            Task.Run(async () =>
            {
                try
                {
                    var provider = builder.Services.BuildServiceProvider();
                    var stockDb = provider.GetService<StockLocationDb>();

                    if (stockDb != null)
                        await stockDb.Warmup();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Warmup StockLocationDb Error: " + ex.Message);
                }
            });

            // 🔥 FIN CAMBIOS IMPORTANTES


            return builder.Build();
        }
    }
}

