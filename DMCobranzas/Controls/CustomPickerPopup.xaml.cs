using CommunityToolkit.Maui.Views;

namespace DMCobranzas.Controls;

public partial class CustomPickerPopup : Popup
{
    //public CustomPickerPopup()
    //{
    //	InitializeComponent();
    //}

    public event Action<string> ItemSelected;

    public CustomPickerPopup(IEnumerable<string> items, string selectedItem)
    {
        InitializeComponent();

        ListView.ItemsSource = items;

        ListView.SelectedItem = selectedItem;

        ListView.SelectionChanged += (s, e) =>
        {
            if (e.CurrentSelection.FirstOrDefault() is string selected)
            {
                ItemSelected?.Invoke(selected);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await CloseAsync();
                });
            }
        };
    }
}