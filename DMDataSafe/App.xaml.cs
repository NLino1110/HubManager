using DMDataSafe.AppPages;
using Models.DMSA.Mbw.Security;

namespace DMDataSafe
{
    public partial class App : Application
    {
        public static AppSession Session { get; set; }

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