using CommunityToolkit.Maui.Views;

namespace BeebTech.Maui.Controls.Controls;

public partial class DropdownMenu : Popup
{
    public IList<string> Items { get; }

    public Action<string>? ItemSelected { get; set; }

    public DropdownMenu(IList<string> items)
    {
        InitializeComponent();
        Items = items;
        BindingContext = this;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string value)
        {
            ItemSelected?.Invoke(value);
            Close();
        }
    }
}