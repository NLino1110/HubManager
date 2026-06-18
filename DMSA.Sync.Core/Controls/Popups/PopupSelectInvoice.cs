using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.CustomRows.Lite;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectInvoice : PopupSelectBase<account_move>
    {
        public res_company Company { get; set; }        
        public res_partner partner { get; set; }
        ObservableCollection<account_move> resultItemsSearch { get; set; }
        public bool LoadAuto { get; set; } = false;
        //public ICommand CommandSelectListItem { get; set; }
        public PopupSelectInvoice(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants, true)
        {            
            DataField = "id, docnum_mask, name, invoice_date, payment_state, amount_residual, amount_total";            
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<account_move>();
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            _collectionViewSearch.MinimumHeightRequest = 400;
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();
            var database = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            var result = await database.GetItemsAsync(Company.id, partner.id, TextForSearch, 25);
            resultItemsSearch = new ObservableCollection<account_move>(result);
            _collectionViewSearch.ItemsSource = resultItemsSearch;
            await SetDoneStatus();
            return 1;
        }

        async Task<int> LoadDataLast20()
        {
            await SetWorkingStatus();

            ResPartnerDb resPartnerDb = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
            var res_Partner = await resPartnerDb.GetItemsAsync(x => x.is_salesman);
            var partnerMap = res_Partner.ToDictionary(x => x.id, x => x.name);

            var database = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            var result = await database.GetItemsAsync(Company.id, partner.id, "", 25);

            if(result.Any())
            {
                //YA NO USAR EL MONTO RESIDUAL
                //////var itemsToUpdate = result
                //////    .Where(i => i.amount_residual_virtual == 0 && i.amount_residual > 0)
                //////    .ToList();         

                foreach (var accountMoveItem in result)
                {
                    var SellerName = partnerMap.ContainsKey(accountMoveItem._partner_sale_id) ? partnerMap[accountMoveItem._partner_sale_id] : "";
                    accountMoveItem.l10n_ec_authorization_number = SellerName;
                }

                var itemsToUpdate = result
                   .Where(i => i.amount_residual > 0)
                   .ToList();

                foreach (var item in itemsToUpdate)
                {
                    item.amount_residual_virtual = item.amount_residual;
                }

                if (itemsToUpdate.Count > 0)
                {
                    foreach (var item in itemsToUpdate)
                        await database.UpdateAsync(item);                    
                }
            }

            resultItemsSearch = new ObservableCollection<account_move>(result);
            _collectionViewSearch.ItemsSource = resultItemsSearch;
            await SetDoneStatus();
            return 1;
        }

        async Task<int> LoadDataForView()
        {
            await SetWorkingStatus();
            var database = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            //var result = await database.GetItemsAsync(Company.id, partner.id, TextForSearch, 25);
            var result = await database.GetItemsByPartnerForPaymentAsync(partner);
            resultItemsSearch = new ObservableCollection<account_move>(result);
            _collectionViewSearch.ItemsSource = resultItemsSearch;
            await SetDoneStatus();

            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle(Company.name);
            SetSubtitle(partner.name);
            SetGridTitles("Facturas");

            if(LoadAuto)
                await LoadDataForView();

            //Custom control 
            Button _btnLoadLastInvoices = new Button
            {
                Text = "Últimas 20",
                BackgroundColor = Colors.SeaGreen,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf0ae"
                }
            };

            _btnLoadLastInvoices.Clicked += OnBtnLoadLast_Clicked;

            var _stackLayoutToolBox = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(2, 0, 0, 0),
                BackgroundColor = Colors.GhostWhite
            };

            _stackLayoutToolBox.Children.Add(_btnLoadLastInvoices);

            ContentCustomToolBox = new Microsoft.Maui.Controls.ContentView()
            {
                Content = _stackLayoutToolBox
            };
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            await LoadData();
        }
        
        //private async void SelectListItem(object objItem)
        //{            
        //    if (objItem is account_move Item)
        //    {
        //        await CloseAsync(Item);
        //    }
        //    else
        //    {
        //        Debug.WriteLine("Error de objeto");
        //    }
        //}        

        private async void OnBtnLoadLast_Clicked(object sender, EventArgs e)
        {
            await LoadDataLast20();
        }

        public override CollectionView builCollectionViewCustom()
        {
            var collectionView = new CollectionView
            {
                BackgroundColor = Colors.WhiteSmoke,
                HorizontalOptions = LayoutOptions.Fill,
                SelectionMode = SelectionMode.Single,
                EmptyView = "No hay datos para mostrar...",
                //ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
            };

            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new AccountMoveRow();
                row.ActionCommand = CommandSelectListItem;

                row.BindingContextChanged += (s, e) =>
                {
                    if (row.BindingContext != null)
                    {
                        var selectedItemBinding = new Binding
                        {
                            Path = "SelectedItem",
                            Source = collectionView,
                            Mode = BindingMode.TwoWay
                        };
                    }
                };

                return row;
            });

            return collectionView;
        }
    }
}
