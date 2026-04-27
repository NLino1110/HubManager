using DMDataSafe.Models;
using DMDataSafe.ViewModels;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace DMDataSafe
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

#if ANDROID
            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
#elif IOS
             UIKit.UIDevice.CurrentDevice.SetValueForKey(Foundation.NSNumber.FromNInt((int)(UIKit.UIInterfaceOrientation.LandscapeLeft)), new Foundation.NSString("orientation"));  

#endif
            DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;
        }

        private void Current_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            if (e.DisplayInfo.Orientation == DisplayOrientation.Landscape)
            {
                //if you use navigatepage    
                NavigationPage.SetHasNavigationBar(this, false);
                //if you use shell    
                Shell.SetNavBarIsVisible(this, false);
            }
        }

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void _dataGrid1_ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            //Console.W
            //e.CurrentSelection

            if (e.CurrentSelection.Count == 0) return;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Team team = (Team)e.CurrentSelection[0];

            string text = "Seleccionado: " + team.Name;
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            toast.Show(cancellationTokenSource.Token).Wait();

            //App.Current.MainPage = obj;
            //await Navigation.PushModalAsync(obj, true);
        }
    }
}