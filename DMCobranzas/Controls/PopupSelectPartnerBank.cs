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
    public class PopupSelectPartnerBank : PopupSelectBase<res_partner_bank>
    {
        public res_company Company { get; set; }
        //Origen de datos
        public res_partner partner { get; set; }

        ObservableCollection<res_partner_bank> resultItemsSearch { get; set; }

        public bool LoadAuto { get; set; } = false;
        public PopupSelectPartnerBank(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants)
        {
            DataField = "id, acc_number, acc_holder_name, type_account, bank_name";
            //_searchBar.SearchButtonPressed += _searchBar_OnTextChanged;
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<res_partner_bank>();
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();
            
            PartnerBankDb partnerBankDb = new PartnerBankDb();
            var resultVar = await partnerBankDb.GetItemsAsync(Company.id, partner.id, TextForSearch);
            resultItemsSearch = new ObservableCollection<res_partner_bank>(resultVar);
            _collectionViewSearch.ItemsSource = resultItemsSearch;

            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Cuentas bancarias");
            SetSubtitle(partner.name);
            SetGridTitles("Id, Cuenta No., Nombre, Tipo, Banco");
            //SetDataFields("id, name, vat, email, total_overdue, total_invoiced");

            if (LoadAuto)
                await LoadDataCustomerAcc();

            //Custom control 
            Button _btnLoadCustomerAcc = new Button
            {
                Text = "Cuentas del cliente",
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

            _btnLoadCustomerAcc.Clicked += OnbtnLoadCustomerAcc_Clicked;

            var _stackLayoutToolBox = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(2, 0, 0, 0),
                BackgroundColor = Colors.GhostWhite
            };

            _stackLayoutToolBox.Children.Add(_btnLoadCustomerAcc);

            ContentCustomToolBox = new Microsoft.Maui.Controls.ContentView()
            {
                Content = _stackLayoutToolBox
            };
        }

        async Task<int> LoadDataCustomerAcc()
        {
            await SetWorkingStatus();

            PartnerBankDb partnerBankDb = new PartnerBankDb();            
            resultItemsSearch = new ObservableCollection<res_partner_bank>(await partnerBankDb.GetItemsAsync(Company.id, partner.id));
            _collectionViewSearch.ItemsSource = resultItemsSearch;

            await SetDoneStatus();
            return 1;
        }


        private async void OnbtnLoadCustomerAcc_Clicked(object sender, EventArgs e)
        {
            await LoadDataCustomerAcc();
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            //Debug.WriteLine("Iniciando busqueda!!");
            await LoadData();
        }
    }
}
