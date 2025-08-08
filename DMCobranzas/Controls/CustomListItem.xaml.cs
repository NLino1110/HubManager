using System.Diagnostics;

namespace CobranzasDMSA_Odoo.Controls;

public partial class CustomListItem : ContentView
{
	public CustomListItem()
	{
		InitializeComponent();
	}

    private void OnSwipeLeft(object sender, SwipedEventArgs e)
    {
        //Debug.WriteLine(e.Parameter.ToString());
        Debug.WriteLine("SwipeLeft");

        ////// Obtener el elemento del CollectionView al que pertenece el StackLayout
        //////var element = ((SwipeGestureRecognizer)sender).Parent.Parent.Parent.Parent;
        //var element = ((Grid)sender);

        ////// Obtener el StackLayout dentro del elemento
        //StackLayout stackLayout = (StackLayout)element.FindByName("ActionsBox");

        ////// Realizar las modificaciones deseadas en el StackLayout
        //////stackLayout.IsVisible = !stackLayout.IsVisible; // Cambiar la visibilidad
        //////stackLayout.Scale = stackLayout.IsVisible ? 1.5 : 1.0; // Cambiar el tamaño
        //ShowHideTools(stackLayout);
    }
}