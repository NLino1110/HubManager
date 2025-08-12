using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages;

public partial class FilterCustomer : ContentView
{
    public List<ISpinnerItem> Items { get; set; }
    public List<ISpinnerItem> Hours { get; set; }
    public List<ISpinnerItem> Minutes { get; set; }
    public ObservableCollection<ISpinnerItem> Seconds { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }

    public event EventHandler OnSearchButtonClicked;

    //public ICommand TapGesture { get; set; }
    class day
    {
        public int id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    class status
    {
        public int id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    day[] Days { get; set; }

    status[] Status { get; set; }

    public FilterCustomer()
	{
		InitializeComponent();

        Days = new day[8]
        {
            new day { id = 0, Name = "Todos" },
            new day { id = 1, Name = "Lunes" },
            new day { id = 2, Name = "Martes" },
            new day { id = 3, Name = "Miercoles" },
            new day { id = 4, Name = "Jueves" },
            new day { id = 5, Name = "Viernes" },
            new day { id = 6, Name = "Sabado" },
            new day { id = 7, Name = "Domingo" },
        };

        ddfDays.ItemsSource = Days;
        ddfDays.ItemDisplayBinding = new Binding("Name");
        ddfDays.SelectedItemChanged += DdfDays_SelectedItemChanged;
        //ddfDays.ItemDisplayBinding = new ;

        Status = new status[3]
        {
            new status { id = 0, Name = "Todos" },
            new status { id = 1, Name = "Activo" },
            new status { id = 2, Name = "Inactivo" },            
        };

        ddfStatus.ItemsSource = Status;
        ddfStatus.ItemDisplayBinding = new Binding("Name");
        //ddfDays.SelectedItemChanged += DdfDays_SelectedItemChanged;

        Items = new List<ISpinnerItem>();
        Hours = new List<ISpinnerItem>();
        Minutes = new List<ISpinnerItem>();
        Seconds = new ObservableCollection<ISpinnerItem>();
        for (int i = 0; i < 10; i++)
            Items.Add(new SpinnerItem { Text = "Item " + i.ToString(), ImageSource = ImageSource.FromFile("dotnet_bot.png") });
        OnPropertyChanged(nameof(Items));
        spinner.SelectedItem = Items[3];

        for (int i = 0; i < 24; i++)
            Hours.Add(new SpinnerItem { Text = i.ToString() });
        OnPropertyChanged(nameof(Hours));
        for (int i = 0; i < 60; i++)
        {
            Minutes.Add(new SpinnerItem { Text = i.ToString() });
            Seconds.Add(new SpinnerItem { Text = i.ToString() });
        }
        OnPropertyChanged(nameof(Minutes));
        OnPropertyChanged(nameof(Seconds));
        Hour = DateTime.Now.Hour;
        Minute = DateTime.Now.Minute;
        Second = DateTime.Now.Second;
        OnPropertyChanged(nameof(Hour));
        OnPropertyChanged(nameof(Minute));
        OnPropertyChanged(nameof(Second));
    }

    private void DdfDays_SelectedItemChanged(object? sender, object e)
    {
        //throw new NotImplementedException();
        Debug.WriteLine(e);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        OnSearchButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        entryCode.Text = "";
        entryId.Text = "";
        entryName.Text = "";
    }

    internal void getCode()
    {
        throw new NotImplementedException();
    }

    internal void getId()
    {
        throw new NotImplementedException();
    }

    internal string getName()
    {
        return entryName.Text;
    }

    internal void getDays()
    {
        throw new NotImplementedException();
    }

    internal void getStatus()
    {
        throw new NotImplementedException();
    }
}