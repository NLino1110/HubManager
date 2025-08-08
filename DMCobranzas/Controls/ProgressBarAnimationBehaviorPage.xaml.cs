using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Markup;
using System.Diagnostics;
using System.Security.Cryptography;

namespace CobranzasDMSA_Odoo;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ProgressBarAnimationBehaviorPage : ContentPage
{
    public string title = "-";
    public string subTitle = "-";

    public class Estado
    {
        public string title = string.Empty;
    }

    static public Estado estado { get; set; }

    //ProgressBarAnimationBehavior progressBarAnimationBehavior { get; set; }
    public ProgressBarAnimationBehaviorPage()
    {
        //title = "Hola!!-------->>";

        //estado = new Estado();
        //Content = new ProgressBar()
        //.Behaviors(new ProgressBarAnimationBehavior
        //{
        //    Progress = 0.75,
        //    Length = 250
        //});
        InitializeComponent();        
    }

    public void SetTitle(string newTitle)
    {
        //BtnUpdate.IsEnabled = false;
        //await Task.Delay(1000);
        //ProgressBarAnimationBehaviorPage.estado.title = newTitle;
        
        try
        {
            Dispatcher.Dispatch(() => {
                title = newTitle;
                lblTitle.Text = newTitle;                
                //SemanticScreenReader.Announce(lblTitle.Text);
            });
        }
        catch (Exception error)
        {
            throw error;
        }

    }

    public void SetSubTitle(string newSubTitle)
    {
        //BtnUpdate.IsEnabled = false;
        //await Task.Delay(1000);
        //ProgressBarAnimationBehaviorPage.estado.title = newTitle;

        try
        {
            Dispatcher.Dispatch(() => {
                subTitle = newSubTitle;
                lblSubTitle.Text = newSubTitle;                
                //SemanticScreenReader.Announce(lblTitle.Text);
            });
        }
        catch (Exception error)
        {
            throw error;
        }

    }

    public async void SetPercentProgress(double newProgress)
    {
        //BtnUpdate.IsEnabled = false;
        //await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = newProgress;
    }

    public async void SetTotalPercentProgress(double newProgress)
    {
        //BtnUpdate.IsEnabled = false;
        //await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocalTotal.Progress = newProgress;
    }

    public async void BtnUpdateProgress(object sender, EventArgs e)
    {
        //BtnUpdate.IsEnabled = false;
        await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = 0.10;
        await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = 0.20;
        await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = 0.30;
        await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = 0.60;
        await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = 0.70;
        await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = 0.90;
        await Task.Delay(1000);
        ProgressBarAnimationBehaviorLocal.Progress = 1;
        //BtnUpdate.IsEnabled = true;


        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        string text = "Actualización terminada...";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);

        //RandomNumberGenerator.Create();
        //var denominator = RandomNumberGenerator.GetInt32(1, 100);

        //double newP = 0.30;// double.Parse((denominator / 100).ToString());
        //Content = new ProgressBar()
        //.Behaviors(new ProgressBarAnimationBehavior
        //{
        //    Progress = newP,
        //    Length = 250
        //});
    }

    protected override void OnDisappearing()
    {
        Debug.WriteLine("OnDisappearing");
        base.OnDisappearing();
    }

    protected override bool OnBackButtonPressed()
    {
        //Dispatcher.Dispatch(async () =>
        //{
        //    var leave = await DisplayAlert("Leave lobby?", "Are you sure you want to leave the lobby?", "Yes", "No");

        //    if (leave)
        //    {
        //        //await handleLeaveAsync();
        //        //await Navigation.PushAsync(new MainPage());
        //        //Shell.Current.GoToAsync("/");
        //    }
        //});

        return true;
    }

    
    //   public ProgressBarAnimationBehaviorPage()
    //{
    //	InitializeComponent();
    //}
}