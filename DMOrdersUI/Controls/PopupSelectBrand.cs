
using DMOrdersUI.Controls.CustomRows;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrdersUI.Controls
{
    public class PopupSelectBrand : PopupSelectBase
    {
        public res_company Company { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<product_brand> resultItemsSearch { get; set; }
       
        public PopupSelectBrand(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants,true)
        {            
            DataField = "id, name";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<product_brand>();            
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();
            ProductBrandDb dbItemsDb = new ProductBrandDb();
            resultItemsSearch = new ObservableCollection<product_brand>((await dbItemsDb.GetItemsAsync()).Where(data=>data.name.Contains(TextForSearch.ToUpper())));
            //resultItemsSearch = new ObservableCollection<product_brand>((await dbItemsDb.GetItemsAsync()).ToList().Take(10));
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Marcas");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Marca");            
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            await LoadData();
        }
        
        public override CollectionView builCollectionViewCustom()
        {
            var collectionView = new CollectionView
            {                
                HorizontalOptions = LayoutOptions.Fill,
                SelectionMode = SelectionMode.Single,
                EmptyView = "Datos no encontrados...",
                ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
            };
                        
            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new BrandRow();
                row.SetBinding(BrandRow.ItemProperty, new Binding("."));

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
                        row.SetBinding(BrandRow.SelectedItemProperty, selectedItemBinding);
                    }
                };

                return row;
            });

            return collectionView;

        }
    }
}
