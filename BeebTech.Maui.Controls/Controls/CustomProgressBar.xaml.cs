namespace BeebTech.Maui.Controls.Controls;

public partial class CustomProgressBar : ContentView
{
	public CustomProgressBar()
	{
		InitializeComponent();
	}

    public string ProgressText => $"{(Progress <= 1 ? Progress * 100 : Progress):0}%";

    public static readonly BindableProperty ProgressProperty =
            BindableProperty.Create(
                nameof(Progress),
                typeof(double),
                typeof(CustomProgressBar),
                0.0,
                propertyChanged: OnProgressChanged);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static readonly BindableProperty ProgressColorProperty =
        BindableProperty.Create(
            nameof(ProgressColor),
            typeof(Color),
            typeof(CustomProgressBar),
            Colors.DodgerBlue);

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public static readonly BindableProperty TrackColorProperty =
        BindableProperty.Create(
            nameof(TrackColor),
            typeof(Color),
            typeof(CustomProgressBar),
            Colors.LightGray);

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public static readonly BindableProperty BarHeightProperty =
        BindableProperty.Create(
            nameof(BarHeight),
            typeof(double),
            typeof(CustomProgressBar),
            6.0);

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    public static readonly BindableProperty AnimationDurationProperty =
        BindableProperty.Create(
            nameof(AnimationDuration),
            typeof(uint),
            typeof(CustomProgressBar),
            (uint)250);

    public uint AnimationDuration
    {
        get => (uint)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }
    private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CustomProgressBar)bindable;
        control.OnPropertyChanged(nameof(ProgressText));
        control.AnimateProgress((double)newValue);
    }

    private void AnimateProgress_OLD(double value)
    {
        if (Width <= 0)
            return;

        if (value > 1)
            value /= 100.0;

        value = Math.Max(0, Math.Min(1, value));

        double targetWidth = Width * value;

        var animation = new Animation(v =>
        {
            ProgressFill.WidthRequest = v;
        }, ProgressFill.Width, targetWidth);

        animation.Commit(this, "ProgressAnim", 16, AnimationDuration);
    }

    private void AnimateProgress(double value)
    {
        if (Width <= 0)
            return;

        if (value > 1)
            value /= 100.0;

        value = Math.Max(0, Math.Min(1, value));

        double targetWidth = Width * value;

        double start = ProgressFill.Width;

        var animation = new Animation(v =>
        {
            ProgressFill.WidthRequest = v;

            // mueve el texto dinámicamente
            UpdateTextPosition(v);
        }, start, targetWidth);

        animation.Commit(this, "ProgressAnim", 16, AnimationDuration, Easing.CubicInOut);
    }

    private void UpdateTextPosition(double currentWidth)
    {        
        if (currentWidth < 40)
        {
            ProgressLabel.HorizontalOptions = LayoutOptions.Start;
            ProgressLabel.Margin = new Thickness(6, 0);
            ProgressLabel.TextColor = Colors.Black;
        }
        else
        {
            ProgressLabel.HorizontalOptions = LayoutOptions.End;
            ProgressLabel.Margin = new Thickness(0, 0, 6, 0);
            ProgressLabel.TextColor = Colors.White;
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        AnimateProgress(Progress);
    }
}