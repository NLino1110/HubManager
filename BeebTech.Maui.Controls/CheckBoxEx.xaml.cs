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

    public static readonly BindableProperty FontSizeProperty =
    BindableProperty.Create(nameof(FontSize), typeof(double), typeof(CheckBoxEx), 14.0);

    public static readonly BindableProperty FontAttributesProperty =
        BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(CheckBoxEx), FontAttributes.None);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(CheckBoxEx), default(string));


    public static readonly BindableProperty ScaleXProperty =
    BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(CheckBoxEx), 1.3);

    public static readonly BindableProperty ScaleYProperty =
        BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(CheckBoxEx), 1.3);

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

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontAttributes FontAttributes
    {
        get => (FontAttributes)GetValue(FontAttributesProperty);
        set => SetValue(FontAttributesProperty, value);
    }

    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double ScaleX
    {
        get => (double)GetValue(ScaleXProperty);
        set => SetValue(ScaleXProperty, value);
    }

    public double ScaleY
    {
        get => (double)GetValue(ScaleYProperty);
        set => SetValue(ScaleYProperty, value);
    }
}