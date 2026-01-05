using System.Windows.Input;

namespace DMCobranzas.Controls.CustomRows;

public partial class CustomHeaderView : ContentView
{
    public static readonly BindableProperty ShowButton1Property =
            BindableProperty.Create(nameof(ShowButton1), typeof(bool), typeof(CustomHeaderView), true);

    public bool ShowButton1
    {
        get => (bool)GetValue(ShowButton1Property);
        set => SetValue(ShowButton1Property, value);
    }

    public static readonly BindableProperty ShowButton2Property =
        BindableProperty.Create(nameof(ShowButton2), typeof(bool), typeof(CustomHeaderView), true);

    public bool ShowButton2
    {
        get => (bool)GetValue(ShowButton2Property);
        set => SetValue(ShowButton2Property, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomHeaderView), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty Button1CommandProperty =
        BindableProperty.Create(nameof(Button1Command), typeof(ICommand), typeof(CustomHeaderView), null);

    public ICommand Button1Command
    {
        get => (ICommand)GetValue(Button1CommandProperty);
        set => SetValue(Button1CommandProperty, value);
    }

    public static readonly BindableProperty Button2CommandProperty =
        BindableProperty.Create(nameof(Button2Command), typeof(ICommand), typeof(CustomHeaderView), null);

    public ICommand Button2Command
    {
        get => (ICommand)GetValue(Button2CommandProperty);
        set => SetValue(Button2CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CustomHeaderView), null);

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public CustomHeaderView()
	{
		InitializeComponent();
	}
}