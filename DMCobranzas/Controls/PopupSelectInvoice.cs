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
    public class PopupSelectInvoice : PopupSelectBase
    {
        public res_company Company { get; set; }
        //Origen de datos
        public res_partner partner { get; set; }

        ObservableCollection<account_move> resultItemsSearch { get; set; }

        public bool LoadAuto { get; set; } = false;

        public PopupSelectInvoice(PopupSizeConstants popupSizeConstants) : 
            base(popupSizeConstants, 
                true)
        {            
            DataField = "id, name, invoice_date, payment_state, amount_residual, amount_total";
            //_searchBar.SearchButtonPressed += _searchBar_OnTextChanged;
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<account_move>();
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();
            var database = new AccountMoveDb();
            var result = await database.GetItemsAsync(Company.id, partner.id, TextForSearch, 25);
            resultItemsSearch = new ObservableCollection<account_move>(result);
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

        async Task<int> LoadDataLast20()
        {
            await SetWorkingStatus();
            var database = new AccountMoveDb();
            var result = await database.GetItemsAsync(Company.id, partner.id, "", 25);
            resultItemsSearch = new ObservableCollection<account_move>(result);
            _collectionViewSearch.ItemsSource = resultItemsSearch;
            await SetDoneStatus();
            return 1;
        }

        async Task<int> LoadDataForView()
        {
            await SetWorkingStatus();
            var database = new AccountMoveDb();
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
            SetGridTitles("Datos de Facturas");

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

            ContentCustomToolBox = new Microsoft.Maui.Controls.ContentView() {
                Content = _stackLayoutToolBox
            };
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
                
                if(Company==null || Company.id == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Clientes",
                                        $"Se requiere que se especifique la compañia para poder realizar la búsqueda de clientes.",
                                        "Continuar");
                    Close(null);
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

        //public ICommand CommandSelectListItem { get; set; }

        private async void SelectListItem(object objItem)
        {            
            if (objItem != null)
            {
                Close(objItem);
            }
            else
            {
                Debug.WriteLine("Error de objeto");
            }
        }

        private async void OnBtnLoadLast_Clicked(object sender, EventArgs e)
        {
            await LoadDataLast20();
        }

        public override CollectionView builCollectionViewCustom()
        {
            return new CollectionView
            {
                //Hay que tener claro que cada vez que se crea un item se vuelve a renderizar
                // y se vuelven a cargar los child dentro del ItemTemplate
                ItemTemplate = new DataTemplate(() =>  new ItemAccountMove(CommandSelectListItem))
            };
        }

        //private async void OnBtnSave_Clicked(object sender, EventArgs e)
        //{
        //    //ResPartnerDb partnerBankDb = new ResPartnerDb();
        //    ////res_partner res_Partner = await partnerBankDb.GetItem(1);
        //    //res_partner res_Partner = await partnerBankDb.GetItemsAsync(Company.id, partner.id);
        //    //// Lógica cuando se hace clic en el primer botón
        //    Close(null);
        //}

        //private void OnBtnCancel_Clicked(object sender, EventArgs e)
        //{
        //    // Lógica cuando se hace clic en el segundo botón
        //    //Close(null);
        //}

        //private void OnBtnClose_Clicked(object sender, EventArgs e)
        //{
        //    // Lógica cuando se hace clic en el segundo botón
        //    Close(null);
        //}
    }
}
