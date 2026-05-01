using DMCobranzas.Services;
using DMSA.Models.Security;
using DMSA.Sync.Core.Services;

namespace DMCobranzas
{
    public partial class App : Application
    {
        //public static IAlertService AlertSvc;
        public static IServiceProvider Services;
        public static AppSession Session { get; set; }
        public static PushRelay PushRelayGlobal { get; set; }

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}