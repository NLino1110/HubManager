using DMOrders.Controls.CustomRows.Lite;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.CustomRows.Lite;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectPartnerCreditData : PopupSelectBase<res_partner>
    {
        public res_company Company { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<res_partner> resultItemsSearch { get; set; }
        
        public PopupSelectPartnerCreditData(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants, true)
        {            
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<res_partner>();
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            MinimumHeightRequest = 400;            
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();
            ResPartnerDb partnerBankDb = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
            resultItemsSearch = new ObservableCollection<res_partner>(await partnerBankDb.GetItemsBySearchAsync(Company.id, TextForSearch.ToUpper(), 50));
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();

            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Clientes");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Cliente");            
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            await LoadData();
        }

        public override CollectionView builCollectionViewCustom()
        {
            var collectionView = new CollectionView
            {
                BackgroundColor = Colors.WhiteSmoke,
                HorizontalOptions = LayoutOptions.Fill,
                SelectionMode = SelectionMode.Single,
                EmptyView = "No hay datos para mostrar...",
                ItemsLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical)
            };

            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new ResPartnerCreditDataRow();
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
