using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Sample.Models;
using Microsoft.Maui.Controls;
using System.Drawing;
using Microsoft.Maui.Graphics;
using DMCobranzas.Models;
using System.Diagnostics;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using DMCobranzas.Settings.Sqlite;
using DMSA.Models.Odoo.General.Responses;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DMCobranzas.Settings.helpers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Layouts;

namespace DMCobranzas.Controls
{
    public class PopupSelectInvoiceItems : PopupSelectBase<account_move_line>
    {
        public res_company Company { get; set; }
        //Origen de datos
        public res_partner partner { get; set; }

        ObservableCollection<account_move_line> resultItemsSearch { get; set; }

        public PopupSelectInvoiceItems(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants)
        {            
            DataField = "id, name, quantity, price_total, moveId, productId, accountId";
            //_searchBar.SearchButtonPressed += _searchBar_OnTextChanged;
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<account_move_line>();
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();

            var databaseInvoice = new AccountMoveDb();
            var resultInvoices = await databaseInvoice.GetItemsAsync(Company.id, partner.id, 50);

            var database = new AccountMoveLineDb();
            var result = await database.GetItemsAsync(TextForSearch, "product", resultInvoices.ToArray(), 25);

            //TODO: Proceso de agrupación, los items no deben repetirse
            var grupo = result.GroupBy(u=>u.productId).ToList();
            var listaItemsAgrupados = grupo.SelectMany(grupo => grupo).DistinctBy(item => item.productId).ToList();

            resultItemsSearch = new ObservableCollection<account_move_line>(listaItemsAgrupados);
            _collectionViewSearch.ItemsSource = resultItemsSearch;
            await SetDoneStatus();

            //IDispatcherTimer timer;

            //timer = Dispatcher.CreateTimer();
            //timer.IsRepeating = false;
            //timer.Interval = TimeSpan.FromMilliseconds(500);
            //timer.Tick += async (s, e) =>
            //{
            //    await SetWorkingStatus();

            //    Debug.WriteLine(_searchBar.Text);
            //    var database = new AccountMoveDb();
            //    var result = await database.GetItemsAsync(Company.id, partner.id, _searchBar.Text, 25);

            //    resultItemsSearch = new ObservableCollection<account_move>(result);

            //    Debug.WriteLine(resultItemsSearch.Count);
            //    //if(_collectionViewSearch.ItemsSource == null)
            //    _collectionViewSearch.ItemsSource = resultItemsSearch;

            //    await SetDoneStatus();

            //    timer.Stop();
            //};

            //timer.Start();

            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle(Company.name);
            SetSubtitle(partner.name);
            SetGridTitles("Col1, Col2, Col3, Col4");            
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            await LoadData();
        }

        private void PrepareForm()
        {
            IDispatcherTimer timer;

            timer = Dispatcher.CreateTimer();
            timer.IsRepeating = false;
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += async (s, e) =>
            {
                Debug.WriteLine("Cargando los datos...");

                //partner = new res_partner {
                //    email = "ronald.chonillo@gmail.com",
                //    id = 1,
                //    name = "Ronald Chonillo",
                //    vat = "0919826958"
                //};

                if(Company==null || Company.id == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Clientes",
                                        $"Se requiere que se especifique la compañia para poder realizar la búsqueda de clientes.",
                                        "Continuar");
                    await CloseAsync();
                    return;
                }

                _labelOverTitle.Text = Company.name;

                //if ( partner != null )
                //{

                //}
                //else
                //{
                //    //Si no se ha enviado el partner de origen no se permitirá el ingreso del dato
                //    await App.Current.MainPage.DisplayAlert("Nueva cuenta",
                //                        $"Se requiere que se especifique el cliente para poder crear nueva cuenta bancaria",
                //                        "Continuar");
                //    Close(null);
                //    return;
                //}                

                timer.Stop();
            };
            timer.Start();
        }

        public ICommand CommandSelectListItem { get; set; }

        private async void SelectListItem(object objItem)
        {            
            if (objItem is account_move_line Item)
            {
                await CloseAsync(Item);
            }
            else
            {
                Debug.WriteLine("Error de objeto");
            }
        }

        private async void OnBtnSave_Clicked(object sender, EventArgs e)
        {
            //ResPartnerDb partnerBankDb = new ResPartnerDb();
            ////res_partner res_Partner = await partnerBankDb.GetItem(1);
            //res_partner res_Partner = await partnerBankDb.GetItemsAsync(Company.id, partner.id);
            //// Lógica cuando se hace clic en el primer botón
            await CloseAsync();
        }

        private void OnBtnCancel_Clicked(object sender, EventArgs e)
        {
            // Lógica cuando se hace clic en el segundo botón
            //Close(null);
        }

        //private void OnBtnClose_Clicked(object sender, EventArgs e)
        //{
        //    // Lógica cuando se hace clic en el segundo botón
        //    Close(null);
        //}
    }
}
