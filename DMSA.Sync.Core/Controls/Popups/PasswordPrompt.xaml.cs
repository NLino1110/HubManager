using CommunityToolkit.Maui.Views;
using DMSA.Sync.Core.Reponses;

namespace DMSA.Sync.Core.Controls.Popups;

public partial class PasswordPrompt : Popup<PasswordPromptResult>
{
    public static readonly BindableProperty TitleBoxProperty =
        BindableProperty.Create(nameof(TitleBox),
            typeof(string),
            typeof(PasswordPrompt),
            default(string));

    public string TitleBox
    {
        get => (string)GetValue(TitleBoxProperty);
        set => SetValue(TitleBoxProperty, value);
    }

    public PasswordPrompt()
    {
        InitializeComponent();
        Margin = new Thickness(0);
        Padding = new Thickness(0);
        BindingContext = this;
    }

    public PasswordPrompt(string title)
    {
        InitializeComponent();
        Margin = new Thickness(0);
        Padding = new Thickness(0);
        BindingContext = this;
        TitleBox = title;
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        await CloseAsync(new PasswordPromptResult
        {
            IsAccepted = true,
            Password = PasswordEntry.Text
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(new PasswordPromptResult
        {
            IsAccepted = false,
            Password = null
        });
    }
}