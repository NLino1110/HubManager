using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using System.Diagnostics;

namespace DMOrdersUI.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ProgressBarAnimationBehaviorPage : ContentPage
{
    public string title = "-";
    public string subTitle = "-";

    // Behaviors definidos como campos para mantener acceso
    //ProgressBarAnimationBehavior ProgressBarAnimationBehaviorLocal;
    //ProgressBarAnimationBehavior ProgressBarAnimationBehaviorLocalTotal;

    public ProgressBarAnimationBehaviorPage()
    {
        InitializeComponent();

        // Asignar comportamiento al primer ProgressBar
        
        
    }

    public void SetTitle(string newTitle)
    {
        Dispatcher.Dispatch(() => {
            title = newTitle;
            lblTitle.Text = newTitle;
        });
    }

    public void SetSubTitle(string newSubTitle)
    {
        Dispatcher.Dispatch(() => {
            subTitle = newSubTitle;
            lblSubTitle.Text = newSubTitle;
        });
    }

    public void SetPercentProgress(double newProgress)
    {
        ProgressBarAnimationBehaviorLocal.Progress = newProgress;
    }

    public void SetTotalPercentProgress(double newProgress)
    {
        ProgressBarAnimationBehaviorLocalTotal.Progress = newProgress;
    }

    public async void BtnUpdateProgress(object sender, EventArgs e)
    {
        double[] steps = [0.10, 0.20, 0.30, 0.60, 0.70, 0.90, 1.0];
        foreach (var step in steps)
        {
            ProgressBarAnimationBehaviorLocal.Progress = step;
            await Task.Delay(1000);
        }

        var toast = Toast.Make("Actualización terminada...", ToastDuration.Short, 14);
        await toast.Show();
    }

    protected override void OnDisappearing()
    {
        Debug.WriteLine("OnDisappearing");
        base.OnDisappearing();
    }

    protected override bool OnBackButtonPressed()
    {
        return true; // Bloquea el botón de retroceso
    }
}
