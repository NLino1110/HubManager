namespace BeebTech.Maui.Controls.UI;

public partial class TitledEntryLight : ContentView
{
    public TitledEntryLight()
    {
        InitializeComponent();

        ApplyDefaultStyles();

        entryMain.TextChanged += (s, e) => TextChanged?.Invoke(this, e);
        entryMain.Completed += (s, e) => Completed?.Invoke(this, e);
        entryMain.Focused += (s, e) => Focused?.Invoke(this, e);
        entryMain.Unfocused += (s, e) => Unfocused?.Invoke(this, e);
    }

    public static readonly BindableProperty AllowPasswordModeProperty =
        BindableProperty.Create(
            nameof(AllowPasswordMode),
            typeof(bool),
            typeof(TitledEntryLight),
            false);

    public bool AllowPasswordMode
    {
        get => (bool)GetValue(AllowPasswordModeProperty);
        set => SetValue(AllowPasswordModeProperty, value);
    }

    public static readonly BindableProperty HidePasswordColorProperty =
        BindableProperty.Create(nameof(HidePasswordColor), typeof(Color), typeof(TogglePasswordEntry),
            defaultValue: Colors.Black);


    public Color HidePasswordColor
    {
        get => (Color)GetValue(HidePasswordColorProperty);
        set => SetValue(HidePasswordColorProperty, value);
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(TitledEntryLight),
            default(string),
            BindingMode.TwoWay);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(TitledEntryLight),
            default(string));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(TitledEntryLight),
            default(string));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(
            nameof(Keyboard),
            typeof(Keyboard),
            typeof(TitledEntryLight),
            Keyboard.Default);

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(
            nameof(IsPassword),
            typeof(bool),
            typeof(TitledEntryLight),
            false);

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    //public static readonly BindableProperty HidePasswordProperty =
    //    BindableProperty.Create(nameof(HidePassword), typeof(bool), typeof(TogglePasswordEntry),
    //        defaultValue: true);

    //public bool HidePassword
    //{
    //    get => (bool)GetValue(HidePasswordProperty);
    //    set => SetValue(HidePasswordProperty, value);
    //}

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(
            nameof(MaxLength),
            typeof(int),
            typeof(TitledEntryLight),
            int.MaxValue);

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(TitledEntryLight),
            Colors.Black);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty EntryBackgroundColorProperty =
        BindableProperty.Create(
            nameof(EntryBackgroundColor),
            typeof(Color),
            typeof(TitledEntryLight),
            Colors.Transparent);

    public Color EntryBackgroundColor
    {
        get => (Color)GetValue(EntryBackgroundColorProperty);
        set => SetValue(EntryBackgroundColorProperty, value);
    }

    public event EventHandler<TextChangedEventArgs> TextChanged;
    public new event EventHandler Focused;
    public new event EventHandler Unfocused;
    public event EventHandler Completed;

    public void FocusEntry() => labelMain.Focus();
    public void UnfocusEntry() => labelMain.Unfocus();


    public static readonly BindableProperty LabelStyleProperty =
        BindableProperty.Create(
            nameof(LabelStyle),
            typeof(Style),
            typeof(TitledEntryLight),
            default(Style));

    public Style LabelStyle
    {
        get => (Style)GetValue(LabelStyleProperty);
        set => SetValue(LabelStyleProperty, value);
    }


    public static readonly BindableProperty EntryStyleProperty =
        BindableProperty.Create(
            nameof(EntryStyle),
            typeof(Style),
            typeof(TitledEntryLight),
            default(Style));

    public Style EntryStyle
    {
        get => (Style)GetValue(EntryStyleProperty);
        set => SetValue(EntryStyleProperty, value);
    }

    private void ApplyDefaultStyles()
    {

        if (LabelStyle == null)
        {
            LabelStyle = new Style(typeof(Label))
            {
                Setters =
            {
                new Setter { Property = Label.FontSizeProperty, Value = 11 },
                new Setter { Property = Label.TextColorProperty, Value = Colors.Gray },
                new Setter { Property = Label.MarginProperty, Value = GetPlatformMargin() },
                new Setter { Property = Label.PaddingProperty, Value = new Thickness(0) }
            }
            };
        }

        if (EntryStyle == null)
        {
            EntryStyle = new Style(typeof(Entry))
            {
                Setters =
            {
                new Setter { Property = Entry.FontSizeProperty, Value = 14 },
                new Setter { Property = Entry.TextColorProperty, Value = Colors.Black },
                new Setter { Property = Entry.BackgroundColorProperty, Value = Colors.Transparent }
            }
            };
        }

        
        labelMain.Style = LabelStyle;
        entryMain.Style = EntryStyle;
    }

    private Thickness GetPlatformMargin()
    {
        var platform = DeviceInfo.Platform;

        if (platform == DevicePlatform.WinUI)
            return new Thickness(5, 0, 0, 0);

        if (platform == DevicePlatform.Android)
            return new Thickness(5, -8, 0, 0);

        if (platform == DevicePlatform.iOS)
            return new Thickness(5, -6, 0, 0);

        return new Thickness(5, 0, 0, 0);
    }

    private void OnImageButtonClicked(object sender, EventArgs e)
    {
        IsPassword = !IsPassword;
    }

}