using Microsoft.Maui.Controls;

namespace BeebTech.Maui.Controls;

public partial class CheckBoxEx : ContentView
{
	public CheckBoxEx()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty IsCheckedProperty =
            BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CheckBoxEx), false);

    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(CheckBoxEx), string.Empty);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CheckBoxEx),
            defaultValue: Colors.Black);

    public static readonly BindableProperty CheckBoxColorProperty =
        BindableProperty.Create(nameof(CheckBoxColor), typeof(Color), typeof(CheckBoxEx),
            defaultValue: Colors.DarkSlateGray);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color CheckBoxColor
    {
        get => (Color)GetValue(CheckBoxColorProperty);
        set => SetValue(CheckBoxColorProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    private void OnLabelTapped(object sender, EventArgs e)
    {
        checkBox.IsChecked = !checkBox.IsChecked;
    }
}