using BeebTech.Maui.Controls.Models.UI;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BeebTech.Maui.Controls.Controls;

public partial class OptionsSelector : ContentView
{
    public IconData Icon { get; set; }

    public ObservableCollection<IconData> Icons { get; } =
        [
            new IconData
            {
                Name = "invoice",
                Description = "Una factura",
                IconSource = "\uf058",
                FontFamily = "FontAwesome5Solid"
            },
            new IconData
            {
                Name = "invoices",
                Description = "Varias facturas",
                IconSource = "\uf0ae",
                FontFamily = "FontAwesome5Solid"
            }
        ];

    public OptionsSelector()
	{
		InitializeComponent();
	}

    [RelayCommand]
    private async Task SelectionChanged(object parameter)
    {
        Debug.WriteLine("SelectionChanged");
        Debug.WriteLine(Icon);

    }
}