using CommunityToolkit.Maui;
using DMOrdersUI.Services;
using DMSA.Models.Security;
using DotNurse.Injector;
using InputKit.Shared.Controls;
using MemoryToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using UraniumUI;
using UraniumUI.Icons.MaterialSymbols;
using UraniumUI.Options;
using UraniumUI.Validations;

namespace DMOrdersUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureMopups()
                .UseUraniumUIBlurs()
                .UseMauiCommunityToolkit()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 5 Free-Regular-400.otf", "FontAwesome5Regular");
                    fonts.AddFont("Font Awesome 5 Brands-Regular-400.otf", "FontAwesome5Brands");
                    fonts.AddFont("Font Awesome 5 Free-Solid-900.otf", "FontAwesome5Solid");

                    fonts.AddFontAwesomeIconFonts();
                    fonts.AddMaterialSymbolsFonts();
                    fonts.AddMaterialIconFonts();
                    fonts.AddFluentIconFonts();

                });


            App.Session = new AppSession();
            Debug.WriteLine(App.Session.isProduction);

            App.Session.AppVersion = AppInfo.Current.VersionString;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
            }

            App.Session.EndPointServer = "http://192.168.100.108:8069";

            //Solo cuando se inicia en modo producción
            if (App.Session.isProduction)
            {
                App.Session.EndPointServer = App.Session.EndPointServerProd;
            }

#if DEBUG
            builder.Logging.AddDebug();

            var memoryLeakEvents = new MemoryLeakDetectEvents();
            builder.Services.AddSingleton(memoryLeakEvents);
            builder.UseLeakDetection(onLeaked: memoryLeakEvents.InvokeOnLeaked, memoryLeakEvents.InvokeOnCollected);
#endif

            builder.Services.Configure<AutoFormViewOptions>(options =>
            {
                options.ValidationFactory = DataAnnotationValidation.CreateValidations;
            });

            RxApp.DefaultExceptionHandler = new AnonymousObserver<Exception>(ex =>
            {
                App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");

                // Track the exception here... (e.g. AppCenter, Sentry, etc.)
            });

            var thisAssembly = typeof(MauiProgram).Assembly;

            builder.Services.AddServicesFrom(
                type => typeof(Page).IsAssignableFrom(type),
                ServiceLifetime.Transient,
                options => options.Assembly = thisAssembly)
            .AddServicesByAttributes(assembly: thisAssembly);

            builder.Services.AddCommunityToolkitDialogs();
            builder.Services.AddMopupsDialogs();

            //App.PushRelayGlobal = new PushRelay();
            //App.PushRelayGlobal.Name = "---";
            //App.PushRelayGlobal.Message = "ConnectCommand";

            return builder.Build();
        }
    }
}
