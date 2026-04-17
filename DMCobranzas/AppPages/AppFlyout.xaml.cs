namespace DMCobranzas.AppPages;

public partial class AppFlyout : FlyoutPage
{
	public AppFlyout()
	{
		InitializeComponent();
        flyoutPage.collectionView.SelectionChanged += OnSelectionChanged;
    }

    async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as FlyoutPageItem;
        if (item != null)
        {
            if (App.Session.CurrentUserFront.log_fec_sincro.Date < DateTime.Today.Date &&
                (
                item.TargetType.Name != "UpdateData" &&
                item.TargetType.Name != "MainPage" &&
                item.TargetType.Name != "About" &&
                item.TargetType.Name != "TestTool" &&
                item.TargetType.Name != "SettingsPage"
                ))
            {
                await DisplayAlert("Atención", "Actualice la información del sistema antes de empezar a realizar operaciones.", "Cerrar");
                return;
            }

            if (item.ExecuteMode == ExecuteModeEnum.Page)
            {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
            }
            else
            {
                if (item.TargetCommand != null)
                    item.TargetCommand?.Execute(null);
            }

            if (!((IFlyoutPageController)this).ShouldShowSplitMode)
                IsPresented = false;
        }
    }    
}