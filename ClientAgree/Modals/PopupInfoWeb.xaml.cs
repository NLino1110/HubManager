using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace ClientAgree.Modals;

public partial class PopupInfoWeb : ContentPage
{
    public static readonly BindableProperty UrlViewerProperty =
        BindableProperty.Create(nameof(UrlViewer), typeof(string), typeof(PopupInfoWeb),
            defaultBindingMode: BindingMode.TwoWay);

    public string UrlViewer
    {
        get => (string)GetValue(UrlViewerProperty);
        set 
        {
            SetValue(UrlViewerProperty, value);
            webViewPopup.Source = UrlViewer;            
        }
    }

    public PopupInfoWeb()
	{
		InitializeComponent();
        //webViewPopup.Source =  "https://www.dmujeres.ec/terminos-condiciones";
        //webViewPopup.Source = UrlViewer; // "https://macronegocios.ec/";
        //(myscroll as IView).InvalidateMeasure();
    }

    private async void Close(object sender, EventArgs e)
    {
        //bool answer = await DisplayAlert("Actualizar", "Está seguro que desea iniciar la actualización?", "Continuar", "Cancelar");
        ////Debug.WriteLine("Answer: " + answer);
        //if (!answer)
        //{
        //    return;
        //}
        await Navigation.PopModalAsync(false);
    }
}