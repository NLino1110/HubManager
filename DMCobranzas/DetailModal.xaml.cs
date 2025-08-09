//using Android.Provider;
using static DMCobranzas.DetailModal;

namespace DMCobranzas;

public partial class DetailModal : ContentPage
{
	public class Estado
	{
		public int id { get; set; }
		public string name { get; set; }
	}

    static public Estado estado { get; set; }

    public DetailModal()
	{
		estado = new Estado();
		estado.id = 0;
		estado.name = "--";
        //SemanticScreenReader.Announce(lblForUpdate.Text);
        InitializeComponent();
	}
    
    public DetailModal(Estado estadoNew)
    {		
        estado = estadoNew;
        InitializeComponent();
    }

    public void SetEstado()
    {
        SemanticScreenReader.Announce(lblForUpdate.Text);
    }

    protected override void OnAppearing()
    {
        Console.WriteLine("Aparece!");
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        
        base.OnDisappearing();
    }
}