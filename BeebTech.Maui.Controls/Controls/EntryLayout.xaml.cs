using Microsoft.Maui.Controls;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace BeebTech.Maui.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EntryLayout : ContentView
{
    public event EventHandler Completed;

    public bool AllowHidePasswordPassword
    {
        get => (bool) GetValue(AllowHidePasswordProperty);
        set => SetValue(AllowHidePasswordProperty, value);
    }

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public EntryLayout()
	{
		InitializeComponent();
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
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(EntryLayout), defaultValue: "Placeholder");

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(EntryLayout),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty HidePasswordProperty =
        BindableProperty.Create(nameof(HidePassword), typeof(bool), typeof(EntryLayout),
            defaultValue: true);

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(EntryLayout),
            defaultValue: Colors.Black);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(EntryLayout),
            defaultValue: Colors.Black);

    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(EntryLayout),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(EntryLayout),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(EntryLayout),
            defaultValue: false);

    public static readonly BindableProperty HidePasswordColorProperty =
        BindableProperty.Create(nameof(HidePasswordColor), typeof(Color), typeof(EntryLayout),
            defaultValue: Colors.Black);

    public static readonly BindableProperty AllowHidePasswordProperty =
        BindableProperty.Create(nameof(AllowHidePasswordPassword), typeof(bool), typeof(EntryLayout),
            defaultValue: false);

    public static readonly BindableProperty ShowIconProperty =
        BindableProperty.Create(nameof(ShowIcon), typeof(bool), typeof(EntryLayout),
            defaultValue: true);

    public static readonly BindableProperty HintColorEditingProperty =
        BindableProperty.Create(nameof(HintColorEditing), typeof(Color), typeof(EntryLayout),
            defaultValue: Colors.Blue);
    public Color HintColorEditing
    {
        get => (Color)GetValue(HintColorEditingProperty);
        set => SetValue(HintColorEditingProperty, value);
    }

    public static readonly BindableProperty HintColorNonEditingProperty =
        BindableProperty.Create(nameof(HintColorNonEditing), typeof(Color), typeof(EntryLayout),
            defaultValue: Colors.Gray);
    public Color HintColorNonEditing
    {
        get => (Color)GetValue(HintColorNonEditingProperty);
        set => SetValue(HintColorNonEditingProperty, value);
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
}