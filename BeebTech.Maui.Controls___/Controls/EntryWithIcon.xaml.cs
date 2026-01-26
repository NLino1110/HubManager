//using CommunityToolkit.Maui.Markup;

namespace BeebTech.Maui.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EntryWithIcon : ContentView
{
	public EntryWithIcon()
	{
		InitializeComponent();        
    }   

    public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(EntryWithIcon));

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(EntryWithIcon),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty HidePasswordProperty =
        BindableProperty.Create(nameof(HidePassword), typeof(bool), typeof(EntryWithIcon),
            defaultValue: true);

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(EntryWithIcon),
            defaultValue: Colors.Black);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(EntryWithIcon),
            defaultValue: Colors.Black);

    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(EntryWithIcon),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(EntryWithIcon),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(EntryWithIcon),
            defaultValue: false);

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool HidePassword
    {
        get => (bool)GetValue(HidePasswordProperty);
        set => SetValue(HidePasswordProperty, value);
    }

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    private void OnImageButtonClicked(object sender, EventArgs e)
    {
        HidePassword = !HidePassword;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        //if (EntryControl == null || e.NewElement == null) return;

        //if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        //    Control.BackgroundTintList = ColorStateList.ValueOf(Color.White);
        //else
        //    Control.Background.SetColorFilter(Color.White, PorterDuff.Mode.SrcAtop);

        //
    }    
}