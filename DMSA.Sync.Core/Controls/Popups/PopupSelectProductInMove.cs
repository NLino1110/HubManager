using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.CustomRows.Lite;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Collections.ObjectModel;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectProductInMove : PopupSelectBase<account_move_line_view>
    {
        public res_company Company { get; set; }
        public res_partner ResPartner { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<account_move_line_view> resultItemsSearch { get; set; }
       
        public PopupSelectProductInMove(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants,true)
        {            
            DataField = "id, name";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<account_move_line_view>();
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
            var dbItemsDb = new AccountMoveLineDb(Constants.Session.odooConnection.DbNameSqlite);
            //var productProductDb = new ProductProductDb(Constants.Session.odooConnection.DbNameSqlite);
            resultItemsSearch = new ObservableCollection<account_move_line_view>(await dbItemsDb.GetByProductName(TextForSearch, ResPartner));
            
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Productos...");
            SetSubtitle(Company.name);
            SetGridTitles("Productos");            
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
                //ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
            };
                        
            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new AccountMoveLineRow();
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
