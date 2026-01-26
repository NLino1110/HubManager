using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
//#if WINDOWS
//using Microsoft.UI.Xaml;
//using Microsoft.UI.Xaml.Controls;
//#endif

#if WINDOWS
using WinThickness = Microsoft.UI.Xaml.Thickness;
#endif

namespace BeebTech.Maui.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TitledEntry : ContentView
{
    public event EventHandler Completed;

    public static readonly BindableProperty ShowIconProperty =
    BindableProperty.Create(
        nameof(ShowIcon),
        typeof(bool),
        typeof(TitledEntry),
        true); 

    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(
            nameof(Glyph),
            typeof(string),
            typeof(TitledEntry),
            "\uf02d"); 

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(TitledEntry),
            "FontAwesome5Solid");

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(
            nameof(IconColor),
            typeof(Color),
            typeof(TitledEntry),
            Colors.Gray);


    public bool AllowHidePassword
    {
        get => (bool) GetValue(AllowHidePasswordProperty);
        set => SetValue(AllowHidePasswordProperty, value);
    }

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public TitledEntry()
	{
		InitializeComponent();
        //ModifyEntry();
        BindingContext = this;

        txtContent.HandlerChanged += TxtContent_HandlerChanged;
    }

    private void TxtContent_HandlerChanged(object? sender, EventArgs e)
    {
        if (txtContent.Handler == null)
            return;

#if WINDOWS
        if (txtContent.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.Padding = new WinThickness(40, 10, 10, 0);
            textBox.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
        }
#elif ANDROID
        if (txtContent.Handler.PlatformView is Android.Widget.EditText editText)
        {
            var density = editText.Context.Resources.DisplayMetrics.Density;

            int extraLeftDp = 36; // AJUSTA ESTE VALOR (dp reales)
            int extraLeftPx = (int)(extraLeftDp * density);

            editText.SetPadding(
                editText.PaddingLeft + extraLeftPx,
                editText.PaddingTop + (int)(1 * density),
                editText.PaddingRight,
                editText.PaddingBottom
            );

            editText.Gravity = Android.Views.GravityFlags.CenterVertical;


            Debug.WriteLine("ANDROID padding aplicado");
        }
#endif
    }

    private void OnRootTapped(object sender, EventArgs e)
    {
        txtContent.Focus();
    }


    public string Placeholder
    {
        get => (string) GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public Color HidePasswordColor
    {
        get => (Color) GetValue(HidePasswordColorProperty);
        set => SetValue(HidePasswordColorProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(TitledEntry), defaultValue: "Placeholder");

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TitledEntry),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty HidePasswordProperty =
        BindableProperty.Create(nameof(HidePassword), typeof(bool), typeof(TitledEntry),
            defaultValue: true);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(TitledEntry),
            defaultValue: Colors.Black);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(TitledEntry),
            defaultValue: false);

    public static readonly BindableProperty HidePasswordColorProperty =
        BindableProperty.Create(nameof(HidePasswordColor), typeof(Color), typeof(TitledEntry),
            defaultValue: Colors.Black);

    public static readonly BindableProperty AllowHidePasswordProperty =
        BindableProperty.Create(nameof(AllowHidePassword), typeof(bool), typeof(TitledEntry),
            defaultValue: false);

    public static readonly BindableProperty HintColorEditingProperty =
        BindableProperty.Create(nameof(HintColorEditing), typeof(Color), typeof(TitledEntry),
            defaultValue: Colors.Blue);
    public Color HintColorEditing
    {
        get => (Color)GetValue(HintColorEditingProperty);
        set => SetValue(HintColorEditingProperty, value);
    }

    public static readonly BindableProperty HintColorNonEditingProperty =
        BindableProperty.Create(nameof(HintColorNonEditing), typeof(Color), typeof(TitledEntry),
            defaultValue: Colors.Gray);
    public Color HintColorNonEditing
    {
        get => (Color)GetValue(HintColorNonEditingProperty);
        set => SetValue(HintColorNonEditingProperty, value);
    }

    public static readonly BindableProperty KeyboardProperty =
    BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(TitledEntry), Keyboard.Default);

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    private void TxtContent_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtContent.Text))
        {
            if (!txtContent.IsFocused)
            {
                lblPlaceholder.TranslationY = 16;
                lblPlaceholder.FontSize = 14;
                lblPlaceholder.TextColor = HintColorNonEditing;
            }
        }
        else
        {
            lblPlaceholder.TranslationY = 5;
            lblPlaceholder.FontSize = 12;
            lblPlaceholder.TextColor = HintColorEditing;
        }
        //Debug.WriteLine(txtContent.Text);
    }

    private async void UpdatePlaceholderState()
    {
        bool hasText = !string.IsNullOrWhiteSpace(Text);
        await AnimatePlaceholder(hasText || txtContent.IsFocused);
    }

    private async Task AnimatePlaceholder(bool up)
    {
        await Task.WhenAll(
            lblPlaceholder.TranslateTo(0, up ? 5 : 16, 120, Easing.CubicOut),
            lblPlaceholder.ScaleTo(up ? 0.85 : 1, 120)
        );

        lblPlaceholder.TextColor = up ? HintColorEditing : HintColorNonEditing;
    }

    private void TxtContent_Focused(object sender, FocusEventArgs e)
    {
        lblPlaceholder.TranslationY = 5;
        lblPlaceholder.FontSize = 12;
        lblPlaceholder.TextColor = HintColorEditing;
    }

    private void TxtContent_Unfocused(object sender, FocusEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtContent.Text))
        {
            lblPlaceholder.TranslationY = 16;
            lblPlaceholder.FontSize = 14;
            lblPlaceholder.TextColor = HintColorNonEditing;
        }
        else
        {
            lblPlaceholder.TranslationY = 5;
            lblPlaceholder.FontSize = 12;
            lblPlaceholder.TextColor = HintColorEditing;
        }
    }

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
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    public void FocusEntry() => txtContent.Focus();
    
}