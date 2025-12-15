using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Dictionaries;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Sales;
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

    public async Task FillData(res_partner _data)
    {
        data = _data;

        clienteTitulo.Text = "DATOS CLIENTE - " + data.id;
        if (string.IsNullOrEmpty( _data.doc_type_identification_name ) )
        {
            TipoIdentificacion tipoIdentificacion = new TipoIdentificacion();
            _data.doc_type_identification_name = tipoIdentificacion[_data._doc_type_identification_id];
        }

        tipIden.Text = _data.doc_type_identification_name;
        vat.Text = data.vat_doc;
        telephone.Text = data.phone;

        if(data._product_pricelist_id > 0)
        {
            var productPricelistDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);
            var productPricelist = await productPricelistDb.GetItem(data._product_pricelist_id);
            if (productPricelist != null)
            {
                data.display_channel_name = productPricelist.name;
            }
        }
        else
        {
            data.display_channel_name = "No asignado";
        }

        channel.Text = data.display_channel_name; //RELLENAR

        if(string.IsNullOrEmpty(data.display_seller_name))
        {
            if(data._adic_comercial_id > 0)
            {
                ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                var seller = await resPartnerDb.GetItemsAsync(data._company_id, data._adic_comercial_id);
                if (seller != null)
                {
                    data.display_seller_name = seller.name;
                }
            }
            else
            {
                data.display_seller_name = "No asignado";
            }
            
        }

        seller.Text = data.display_seller_name; //RELLENAR
        status.Text = data.active ? "Activo" : "Inactivo";

        if (string.IsNullOrEmpty(data.display_ranking_credit))
        {
            if (data._calificacion_crediticia_id > 0)
            {
                CalificacionCrediticiaDb calificacionDb = new CalificacionCrediticiaDb(App.Session.odooConnection.DbNameSqlite);
                var calificacion = await calificacionDb.GetItem(data._calificacion_crediticia_id);
                if (calificacion != null)
                {
                    data.display_ranking_credit = calificacion.name;
                }
            }
            else
            {
                data.display_ranking_credit = "No asignado";
            }
        }


        calif.Text = data.display_ranking_credit;
        cupo.Text = data.facturacion_cupo_maximo.ToString();
        obs.Text = data.misc_comentarios;
        days.Text = data.facturacion_dias_credito_limite.ToString();
    }
}