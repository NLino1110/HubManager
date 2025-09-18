using DMCobranzas.AppPages.Printing;
using System.Diagnostics;
using System.Xml.Linq;

namespace DMCobranzas.AppPages;

//public static class VisualStateProperties
//{
//    //public static readonly BindableProperty MyColorProperty =
//    //    BindableProperty.CreateAttached("MyColor", typeof(Color), typeof(VisualStateProperties), Colors.Red);

//    //public static Color GetMyColor(BindableObject view)
//    //{
//    //    return (Color)view.GetValue(MyColorProperty);
//    //}

//    //public static void SetMyColor(BindableObject view, Color value)
//    //{
//    //    view.SetValue(MyColorProperty, value);
//    //}
//}

public partial class FlyoutMenuPage : ContentPage
{
    //public static readonly BindableProperty MyColorProperty =
    //    BindableProperty.Create(nameof(MyColor), typeof(Color), typeof(FlyoutMenuPage), Colors.Red);

    //public Color MyColor
    //{
    //    get { return (Color)GetValue(MyColorProperty); }
    //    set { SetValue(MyColorProperty, value); }
    //}
    public FlyoutMenuPage()
	{
		InitializeComponent();
        LoadSession();
	}
    
    private void LoadSession()
    {
        if (App.Session != null)
        {
            txtUser.Text = "" + App.Session.CurrentUser.username;
            txtName.Text = "" + App.Session.CurrentUser.nombres;
            txtEnvironment.Text = "Desarrollo";
            if(App.Session.odooConnection.IsProduction)
            {
                txtEnvironment.Text = "Producción";
            }

            if (App.Session.odooConnection.IsTestMode)
            {
                //
                FlyoutPageItem[] flyoutPageItem = (FlyoutPageItem[])collectionView.ItemsSource;
                List<FlyoutPageItem> ls_flyoutPageItems = flyoutPageItem.ToList();
                ls_flyoutPageItems.Add(new FlyoutPageItem()
                {
                    Title = "Prueba de Impresión",
                    FontFamily = "FontAwesome5Solid",
                    IconSource = "#",
                    TargetType = typeof(TestTool)
                });

                ls_flyoutPageItems.Add(new FlyoutPageItem()
                {
                    Title = "Configuración",
                    FontFamily = "FontAwesome5Solid",
                    IconSource = "#",
                    TargetType = typeof(SettingsPage)
                });

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
}