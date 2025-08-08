using DMSA.Models.Odoo.Native;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrdersUI.Pages.Fragments.Product;

public partial class Info : ContentView
{    
    //public ObservableCollection<ISpinnerItem> Seconds { get; set; }
    private product_product data;

    public static readonly BindableProperty Base64SourceProperty =
             BindableProperty.Create(
                 nameof(Base64Source),
                 typeof(string),
                 typeof(Info),
                 string.Empty,
                 propertyChanged: OnBase64SourceChanged);

    private static void OnBase64SourceChanged(BindableObject bindable, object oldValue, object newValue)
    {        
        //MemoryStream stream = new MemoryStream(Convert.FromBase64String((string)newValue));        
        //((Image)bindable).Source = ImageSource.FromStream(() => stream);        
    }

    public string Base64Source
    {
        set
        {
            SetValue(Base64SourceProperty, value);
        }
        get
        {
            return (string)GetValue(Base64SourceProperty);
        }
    }

    public Info()
	{
		InitializeComponent();
    }

    public void FillData(product_product _data)
    {
        data = _data;
        LabelTitle.Text = data.name;        
        status.Text = data.active ? "Activo" : "Inactivo";
        calif.Text = "666";
        obs.Text = "Obs";
        days.Text = "0 dias";

        Base64Source = data.image_256 ?? string.Empty;

        //MemoryStream stream = new MemoryStream(Convert.FromBase64String((string)Base64Source));
        //productImage.Source = ImageSource.FromStream(() => stream);     

        //OnPropertyChanged(nameof(Base64Source));

        //byte[] imageBytes = Convert.FromBase64String(data.image_256);

        //productImage = new Image
        //{
        //    Source = ImageSource.FromStream(() => new MemoryStream(imageBytes)),
        //    //Aspect = Aspect.AspectFill,
        //    //HeightRequest = 200,
        //    //WidthRequest = 200
        //};
    }
}