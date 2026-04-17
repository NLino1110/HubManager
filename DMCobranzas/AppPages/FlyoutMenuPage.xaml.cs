using DMCobranzas.AppPages.Printing;
using System.Windows.Input;

namespace DMCobranzas.AppPages;

public partial class FlyoutMenuPage : ContentPage
{
    public ICommand TryLogoutCommand { get; }

    public FlyoutMenuPage()
	{
        TryLogoutCommand = new Command(async () => await TryLogout());
        InitializeComponent();
        LoadSession();        
    }
    
    private void LoadSession()
    {
        if (App.Session != null)
        {
            txtConnection.Text = App.Session.odooConnection.Name;
            txtUser.Text = "" + App.Session.CurrentUserFront.username;
            txtName.Text = "" + App.Session.CurrentUserFront.nombres;
            txtEnvironment.Text = "Desarrollo";

            string version_data = "Versión " + App.Session.AppVersion;

            if(App.Session.odooConnection.IsProduction)
            {
                txtEnvironment.Text = "Producción";
            }

            FlyoutPageItem[] flyoutPageItem = (FlyoutPageItem[])collectionView.ItemsSource;
            List<FlyoutPageItem> ls_flyoutPageItems = flyoutPageItem.ToList();

            ls_flyoutPageItems.Add(new FlyoutPageItem()
            {
                Title = "Cerrar Sesión",
                FontFamily = "FontAwesome5Solid",
                IconSource = "\uf2f5",
                ExecuteMode = ExecuteModeEnum.Function,
                TargetCommand = TryLogoutCommand
            });

            ls_flyoutPageItems.Add(new FlyoutPageItem()
            {
                Title = version_data,
                FontFamily = "FontAwesome5Solid",
                IconSource = "\uf126",
                ExecuteMode = ExecuteModeEnum.Function,
                TargetCommand = null
            });

            if (App.Session.odooConnection.IsTestMode)
            {               
                ls_flyoutPageItems.Add(new FlyoutPageItem()
                {
                    Title = "Prueba de Impresión",
                    FontFamily = "FontAwesome5Solid",
                    IconSource = "\uf02f",
                    ExecuteMode = ExecuteModeEnum.Page,
                    TargetType = typeof(TestTool)
                });                

                //ls_flyoutPageItems.Add(new FlyoutPageItem()
                //{
                //    Title = "Configuración",
                //    FontFamily = "FontAwesome5Solid",
                //    IconSource = "#",
                //    TargetType = typeof(SettingsPage)
                //});

                collectionView.ItemsSource = ls_flyoutPageItems.ToArray();
                //Debug.WriteLine(flyoutPageItem.Length);
            }
        }
    }

    public async void TryLogout(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Salir", "Está seguro que desea cerrar la sesión?", "Si", "No");
        //Debug.WriteLine("Answer: " + answer);
        if(answer)
        { 
            App.Current.MainPage = new Login();
        }    
    }

    public async Task TryLogout()
    {
        bool answer = await DisplayAlert("Salir", "Está seguro que desea cerrar la sesión?", "Si", "No");
        //Debug.WriteLine("Answer: " + answer);
        if (answer)
        {
            App.Current.MainPage = new Login();
        }
    }
}