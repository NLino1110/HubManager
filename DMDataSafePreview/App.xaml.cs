using Microsoft.Extensions.DependencyInjection;
using DMSA.Models.Security;

namespace DMDataSafePreview
{
    public partial class App : Application
    {
        public static AppSession Session { get; set; }

        public App()
        {
            InitializeComponent();
            // Initialize a default application session so shared login/process code
            // that expects `App.Session` is available at runtime.
            Session = new AppSession();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}