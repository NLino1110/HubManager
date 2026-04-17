using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.CustomRows.Lite;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Collections.ObjectModel;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectBank : PopupSelectBase<ResBank>
    {
        public res_company Company { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<ResBank> resultItemsSearch { get; set; }
       
        public PopupSelectBank(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants,true)
        {            
            DataField = "id, name";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<ResBank>();
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
            var dbItemsDb = new BankDb(Constants.Session.odooConnection.DbNameSqlite);
            //resultItemsSearch = new ObservableCollection<ResBank>(await dbItemsDb.GetItemsAsync(data => data.name.Contains(TextForSearch, StringComparison.OrdinalIgnoreCase)));
            
            var search = $"%{TextForSearch}%";

            TextForSearch = TextForSearch.ToUpper();
            //var items = await dbItemsDb.QueryAsync(
            //    "SELECT * FROM ResBank WHERE name LIKE ? COLLATE NOCASE",
            //    new object[] { search }
            //);

            var items = await dbItemsDb.SearchByColumnAsync("name", TextForSearch, 25);

            //var items = await dbItemsDb.GetItemsBySearchAsync(Constants.Session.res_Company.id, TextForSearch, 25);
            
            resultItemsSearch = new ObservableCollection<ResBank>(items);

            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Bancos");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Bancos");            
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
                ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
            };
                        
            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new ResBankRow();
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
