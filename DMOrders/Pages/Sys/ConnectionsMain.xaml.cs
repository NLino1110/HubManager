namespace DMOrders.Pages.Sys;

public partial class ConnectionsMain : ContentPage
{
    bool bottomPanelOpen = false;

    public ConnectionsMain()
	{
		InitializeComponent();
	}

    private async void BottomPanel_Tapped(object sender, EventArgs e)
    {
        if (bottomPanelOpen)
        {
            // OCULTAR
            await BottomPanel.TranslateTo(0, 30, 250, Easing.CubicOut);
            bottomPanelOpen = false;
        }
        else
        {
            // MOSTRAR
            await BottomPanel.TranslateTo(0, -100, 250, Easing.CubicIn);
            bottomPanelOpen = true;
        }
    }

    public async void ToggleBottomPanel()
    {
        if (bottomPanelOpen)
        {
            await BottomPanel.TranslateTo(0, 300, 250, Easing.CubicOut);
            bottomPanelOpen = false;
        }
        else
        {
            await BottomPanel.TranslateTo(0, 0, 250, Easing.CubicIn);
            bottomPanelOpen = true;
        }
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}