namespace DMOrders;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ProgressBarPage : ContentPage
{
    public string _title;
    public string title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public string _subTitle;
    public string subTitle
    {
        get => _subTitle;
        set
        {
            if (_subTitle != value)
            {
                _subTitle = value;
                OnPropertyChanged();
            }
        }
    }

    private double progressCurrent;
    public double ProgressCurrent
    {
        get => progressCurrent;
        set
        {
            if (progressCurrent != value)
            {
                progressCurrent = value;
                OnPropertyChanged();
            }
        }
    }

    private double progressTotal;
    public double ProgressTotal
    {
        get => progressTotal;
        set
        {
            if (progressTotal != value)
            {
                progressTotal = value;
                OnPropertyChanged();
            }
        }
    }

    public class Estado
    {
        public string title = string.Empty;
    }

    static public Estado estado { get; set; }

    public ProgressBarPage()
    {        
        InitializeComponent();
        BindingContext = this;
    }

    public void SetTitle(string newTitle)
    {        
        try
        {
            title = newTitle;         
        }
        catch (Exception error)
        {
            throw error;
        }
    }

    public void SetSubTitle(string newSubTitle)
    {       
        try
        {
            subTitle = newSubTitle;         
        }
        catch (Exception error)
        {
            throw error;
        }
    }

    public async void SetPercent(double newProgress)
    {        
        //ProgressBarAnimationBehaviorLocal.Progress = newProgress;
        ProgressCurrent = newProgress;
    }

    public async void SetTotalPercent(double newProgress)
    {        
        //ProgressBarAnimationBehaviorLocalTotal.Progress = newProgress;
        ProgressTotal = newProgress;
    }

    //public async void BtnUpdateProgress(object sender, EventArgs e)
    //{        
    //    await Task.Delay(1000);
    //    ProgressBarAnimationBehaviorLocal.Progress = 0.10;
    //    await Task.Delay(1000);
    //    ProgressBarAnimationBehaviorLocal.Progress = 0.20;
    //    await Task.Delay(1000);
    //    ProgressBarAnimationBehaviorLocal.Progress = 0.30;
    //    await Task.Delay(1000);
    //    ProgressBarAnimationBehaviorLocal.Progress = 0.60;
    //    await Task.Delay(1000);
    //    ProgressBarAnimationBehaviorLocal.Progress = 0.70;
    //    await Task.Delay(1000);
    //    ProgressBarAnimationBehaviorLocal.Progress = 0.90;
    //    await Task.Delay(1000);
    //    ProgressBarAnimationBehaviorLocal.Progress = 1;       

    //    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    //    string text = "Actualización terminada...";
    //    ToastDuration duration = ToastDuration.Short;
    //    double fontSize = 14;
    //    var toast = Toast.Make(text, duration, fontSize);
    //    await toast.Show(cancellationTokenSource.Token);
    //}

    //protected override void OnDisappearing()
    //{
    //    Debug.WriteLine("OnDisappearing");
    //    base.OnDisappearing();
    //}

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

    //public new event PropertyChangedEventHandler PropertyChanged;

    //protected new void OnPropertyChanged([CallerMemberName] string name = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //}
}