namespace BeebTech.Maui.Controls.Controls;

public partial class TitledPicker : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(TitledPicker), string.Empty);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList<object>), typeof(TitledPicker));

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(TitledPicker), null, BindingMode.TwoWay);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IList<object> ItemsSource
    {
        get => (IList<object>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public TitledPicker()
	{
		InitializeComponent();

        TitleLabel.IsVisible = DeviceInfo.Platform == DevicePlatform.Android;
    }
}