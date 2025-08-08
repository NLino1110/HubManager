using CommunityToolkit.Maui.Sample.Pages;

namespace CobranzasDMSA_Odoo.AppPages;

public partial class AppFlyout : FlyoutPage
{
	public AppFlyout()
	{
		InitializeComponent();
        flyoutPage.collectionView.SelectionChanged += OnSelectionChanged;
    }

    void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //var item = e.CurrentSelection.FirstOrDefault() as FlyoutPageItem;
        //if (item != null)
        //{
        //    Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));            
        //    IsPresented = false;
        //}

        var item = e.CurrentSelection.FirstOrDefault() as FlyoutPageItem;
        if (item != null)
        {            
            if (item.TargetType.Name.Equals("MultiplePopupPage") || item.TargetType.Name.Equals("NotasCreditoExtPage"))
            {
                //En caso de que se esten utilizando herencias de BasePage de Community Toolkit
                //   estas páginas permiten crear popups multiples que son más vistosas.
                Detail = new NavigationPage((BasePage)Activator.CreateInstance(item.TargetType, null, null, null, null));
            }
            else
            {
                //string fechaActualizacion = App.Session.CurrentUser.fechasincronizado;
                //if((fechaActualizacion == null || "" == fechaActualizacion.Trim()))

                if (App.Session.CurrentUser.log_fec_sincro.Date < DateTime.Today.Date && 
                    (
                    item.TargetType.Name != "UpdateData" &&
                    item.TargetType.Name != "MainPage" &&
                    item.TargetType.Name != "About" &&
                    item.TargetType.Name != "TestTool" &&
                    item.TargetType.Name != "SettingsPage"
                    ))
                {
                    DisplayAlert("Atención", "Actualice la información del sistema antes de empezar a realizar operaciones.", "Cerrar");
                    return;
                }
                //TODO: Programar aquí las condiciones para validar la fecha y estado de actualización del sistema
                //  sino cumple con las condiciones, debe mostrar un mensaje e impedir la carga de la página solicitada.
                //if (true)
                //{
                //    DisplayAlert("Atención", "Actualice la información del sistema antes de empezar a realizar operaciones.", "Cerrar");
                //    return;
                //}

                Detail = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
            }

            if (!((IFlyoutPageController)this).ShouldShowSplitMode)
                IsPresented = false;
        }
    }
}