using DMSA.Models.Odoo.Native;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Customers;

public partial class Info : ContentView
{    
    //public ObservableCollection<ISpinnerItem> Seconds { get; set; }
    private res_partner data;

    public Info()
	{
		InitializeComponent();
    }

    public void FillData(res_partner _data)
    {
        data = _data;
        tipIden.Text = "Cedula/Ruc";
        vat.Text = data.vat;
        telephone.Text = data.phone;
        channel.Text = "Canal";
        seller.Text = "Vendedor";
        status.Text = data.active ? "Activo" : "Inactivo";
        calif.Text = "666";                
        cupo.Text = data.credit.ToString();
        obs.Text = "Obs";
        days.Text = "0 dias";
    }
}