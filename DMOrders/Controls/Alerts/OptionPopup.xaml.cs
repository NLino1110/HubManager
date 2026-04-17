using CommunityToolkit.Maui.Views;

namespace DMOrders.Controls.Alerts;

public partial class OptionPopup : Popup
{
    public string Result { get; private set; }
    public OptionPopup()
    {
        InitializeComponent();
    }

    private async void OnTotalClicked(object sender, EventArgs e)
    {
        Result = "Total";
        await CloseAsync();
    }

    private async void OnParcialClicked(object sender, EventArgs e)
    {
        Result = "Parcial";
        await CloseAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        Result = "Cancelar";
        await CloseAsync();
    }
}