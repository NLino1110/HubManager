using DMOrders.Services;
using DMSA.Models.Security;
using DMSA.Sync.Core.Services;

namespace DMOrders
{
    public partial class App : Application
    {
        public static AppSession Session { get; set; }
        public static PushRelay PushRelayGlobal { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}