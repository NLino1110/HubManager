using System.Diagnostics;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Controls.Popups
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
            Padding = new Thickness(0);
            Margin = new Thickness(0);
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();

            var databaseInvoice = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            var resultInvoices = await databaseInvoice.GetItemsAsync(Company.id, partner.id, 50);

            var database = new AccountMoveLineDb(Constants.Session.odooConnection.DbNameSqlite);
            var result = await database.GetItemsAsync(TextForSearch, "product", resultInvoices.ToArray(), 25);

            //TODO: Proceso de agrupación, los items no deben repetirse
            var grupo = result.GroupBy(u=>u._product_id).ToList();
            var listaItemsAgrupados = grupo.SelectMany(grupo => grupo).DistinctBy(item => item._product_id).ToList();

            resultItemsSearch = new ObservableCollection<account_move_line>(listaItemsAgrupados);
            _collectionViewSearch.ItemsSource = resultItemsSearch;
            await SetDoneStatus();
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
