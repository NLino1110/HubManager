using DMDataSafe.AppPages.Sys;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Models.DMSA.Mbw.Security;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Diagnostics;

namespace DMDataSafe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 5 Free-Regular-400.otf", "FontAwesome5Regular");
                    fonts.AddFont("Font Awesome 5 Brands-Regular-400.otf", "FontAwesome5Brands");
                    fonts.AddFont("Font Awesome 5 Free-Solid-900.otf", "FontAwesome5Solid");
                })
                .UseMauiCommunityToolkit()
                .UseSkiaSharp();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            //Routing.RegisterRoute("about", typeof(About));

            //Se coloca instancia de inicio de sesión
            App.Session = new AppSession();
            Debug.WriteLine(App.Session.isProduction);

            //Solo cuando se inicia en modo producción
            if (App.Session.isProduction)
            {
                App.Session.EndPointServer = App.Session.EndPointServerProd;
                //TODO: Se Omite por ahora ya que aun falta hacerle el NAT al servidor
                //App.Session.CacheFilesUrl = App.Session.CacheFilesUrlProd;

                App.Session.EndPointServerNewApi = App.Session.EndPointServerNewApiInternal;
            }

            return builder.Build();
        }
    }
}