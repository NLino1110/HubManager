using UraniumUI.Material.Resources;
using DMSA.Models.Security;
using DMOrdersUI.Services;
using UraniumUI;
using System.Diagnostics;

#if WINDOWS
using Microsoft.UI.Windowing;
#endif

namespace DMOrdersUI
{
    public partial class App : Application
    {
        public static bool IsDebug =>
#if DEBUG
    true;
#else
    false;
#endif

        public static AppSession Session { get; set; }
        public static PushRelay PushRelayGlobal { get; set; }
        public App()
        {
            InitializeComponent();
            //MainPage = new Login();

            //AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            //{
            //    Exception ex = e.ExceptionObject as Exception;
            //    Debug.WriteLine($"[UNHANDLED] {ex?.Message}\n{ex?.StackTrace}");
            //};

            MainPage = UraniumServiceProvider.Current.GetRequiredService<AppShellStart>();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Title = "DMOrdersUI";
#if WINDOWS
            window.HandlerChanged += (sender, args) =>
            {
                if (window.Handler?.PlatformView is MauiWinUIWindow w)
                {
                    var presenter = (w.AppWindow.Presenter as OverlappedPresenter);
                }
            };
#endif
            return window;
            //return new Window(UraniumServiceProvider.Current.GetRequiredService<AppShellStart>());
            //return new Window(new AppShellStart());            
        }
    }
}
