using DMOrders.Models.Filters;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Activities;

public partial class Filters : ContentView
{
    public event EventHandler OnSearchButtonClicked;
    
    FStatus[] Status { get; set; }

    public Filters()
	{
		InitializeComponent();
        
        Status = new FStatus[3]
        {
            new FStatus { id = -1, Name = "Todos" },
            new FStatus { id = 0, Name = "Activo" },
            new FStatus { id = 1, Name = "Inactivo" },            
        };

        ddfStatus.ItemsSource = Status;        
        ddfStatus.ItemDisplayBinding = new Binding("Name");
        ddfStatus.SelectedItem = Status[0];

        datePickerStart.Date = DateTime.Now.AddDays(-7);
        datePickerEnd.Date = DateTime.Now;
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
        //entryCode.Text = "";
        //entryId.Text = "";
        //entryName.Text = "";
    }    

    internal int getStatus()
    {
        return ((FStatus)ddfStatus.SelectedItem).id;
    }

    internal DateTime? getDateStart()
    {
        return datePickerStart.Date;
    }

    internal DateTime? getDateEnd()
    {
        if (datePickerEnd?.Date == null)
            return null;

        var date = datePickerEnd.Date;

        // Combina solo la parte de fecha con hora 23:59:59
        return new DateTime(
            date.Value.Year,
            date.Value.Month,
            date.Value.Day,
            23, 59, 59,
            date.Value.Kind   // conserva el tipo (Local / UTC)
        );
    }
}