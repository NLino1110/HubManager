using ClientAgree.AppPages;
using Models.DMSA.Mbw.Security;

namespace ClientAgree
{
    public partial class App : Application
    {
        public static AppSession Session { get; set; }

        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();
            //MainPage = new MainPageForProcess();
            
            MainPage = new Login();
        }
    }
}