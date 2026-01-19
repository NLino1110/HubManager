using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
namespace DMCobranzas.Controls;

public partial class CustomPicker : VerticalStackLayout
{
    public static readonly BindableProperty ItemsProperty =
        BindableProperty.Create(nameof(Items), typeof(IEnumerable<string>), typeof(CustomPicker));

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(string), typeof(CustomPicker),
                                propertyChanged: OnSelectedItemChanged);

    public CustomPicker()
	{
		InitializeComponent();

        BindingContextChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(SelectedText));
        };


        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) => await ShowPicker();
        this.GestureRecognizers.Add(tap);
    }

    public IEnumerable<string> Items
    {
        get => (IEnumerable<string>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string SelectedText => SelectedItem ?? "Seleccione...";

    async Task ShowPicker()
    {
        var popup = new CustomPickerPopup(Items, SelectedItem);
        popup.ItemSelected += (val) => SelectedItem = val;

        // Corregido: se debe pasar la instancia de Page como primer argumento
        var page = Application.Current?.MainPage;
        if (page != null)
        {
            await PopupExtensions.ShowPopupAsync<CustomPickerPopup>(page, popup);
        }
    }

    static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CustomPicker)bindable;
        control.OnPropertyChanged(nameof(SelectedText));
    }
}