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
using CobranzasDMSA_Odoo.Models;
using System.Diagnostics;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using CobranzasDMSA_Odoo.Settings.Sqlite;
using DMSA.Models.Odoo.General.Responses;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CobranzasDMSA_Odoo.Settings.helpers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Layouts;

namespace CobranzasDMSA_Odoo.Controls
{
    public class PopupSelectPartner : PopupSelectBase
    {
        public res_company Company { get; set; }
        //Origen de datos
        //public res_partner partner { get; set; }

        public int DetailMode { get; set; } = 0;
        ObservableCollection<res_partner> resultItemsSearch { get; set; }

        public static Microsoft.Maui.Controls.ContentView _CustomDataTemplate()
        {
            //get
            //{
            //Grid scrollGridContent = null;
            var swipeView = new SwipeView { IsClippedToBounds = true };
            return swipeView;
        }

        public PopupSelectPartner(PopupSizeConstants popupSizeConstants) : 
            base(popupSizeConstants,
            true // si se coloca true aquí, es requerido implementar
                                    // public override CollectionView builCollectionViewCustom()
            )
        {
            DataField = "id, name, vat, email, total_due, total_overdue, total_invoiced, total_to_beat, positive_balance, title, client_type_display";
            //DataField = "Datos de clientes";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<res_partner>();
            
            Debug.WriteLine("PopupSelectPartner");
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();
            ResPartnerDb partnerBankDb = new ResPartnerDb();
            resultItemsSearch = new ObservableCollection<res_partner>(await partnerBankDb.GetItemsBySearchAsync(Company.id, TextForSearch.ToUpper(), 25));
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();

            //IDispatcherTimer timer;

            //timer = Dispatcher.CreateTimer();
            //timer.IsRepeating = false;
            //timer.Interval = TimeSpan.FromMilliseconds(500);
            //timer.Tick += async (s, e) =>
            //{
            //    await SetWorkingStatus();

            //    //SearchBar searchBar = (SearchBar) sender;

            //    if (_searchBar.Text.Length < 2)
            //    {
            //        return;
            //    }

            //    ResPartnerDb partnerBankDb = new ResPartnerDb();
            //    ObservableCollection<res_partner> lpartners = new ObservableCollection<res_partner>();
            //    lpartners = new ObservableCollection<res_partner>(await partnerBankDb.GetItemsBySearchAsync(Company.id, _searchBar.Text.ToUpper(), 25));
            //    _collectionViewSearch.ItemsSource = lpartners;

            //    timer.Stop();

            //    await SetDoneStatus();

            //    timer.Stop();
            //};

            //timer.Start();

            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Clientes");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Cliente");
            //SetDataFields("id, name, vat, email, total_overdue, total_invoiced");
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            await LoadData();
        }
        
        public override CollectionView builCollectionViewCustom()
        {
            return new CollectionView
            {
                //Hay que tener claro que cada vez que se crea un item se vuelve a renderizar
                // y se vuelven a cargar los child dentro del ItemTemplate
                ItemTemplate = new DataTemplate(() =>
                {
                    var swipeView = new SwipeView { IsClippedToBounds = true };

                    Button btnForSwipe = new Button
                    {
                        CornerRadius = 2,
                        ContentLayout = new Button.ButtonContentLayout( Button.ButtonContentLayout.ImagePosition.Top,0 ),
                        Command = CommandSelectListItem,
                        BackgroundColor = Colors.DeepSkyBlue,
                        Text = " ",
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 13,
                        HorizontalOptions = LayoutOptions.End,
                        WidthRequest = 100,
                        ImageSource = new FontImageSource
                        {
                            FontFamily = "FontAwesome5Solid",
                            Color = Colors.White,
                            Size = 20,
                            FontAutoScalingEnabled = true,
                            Glyph = "\uf058"
                        }
                    };

                    //btnSelectSwipeWindows.SetBinding(Button.CommandParameterProperty, new Binding("."));
                    btnForSwipe.SetBinding(Button.CommandParameterProperty, new Binding("."));

                    swipeView.RightItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;

                    swipeView.RightItems.Add(
                            new SwipeItemView
                            {
                                Content = btnForSwipe
                            }
                        );

                    scrollGridContent = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        BackgroundColor = Colors.WhiteSmoke,
                        RowDefinitions = new RowDefinitionCollection
                        {
                            new RowDefinition { Height = GridLength.Auto },
                            new RowDefinition { Height = GridLength.Auto },
                            new RowDefinition { Height = GridLength.Auto },
                        },
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Auto }, //0
                            new ColumnDefinition { Width = GridLength.Auto }, //1
                            new ColumnDefinition { Width = GridLength.Star }, //2
                            new ColumnDefinition { Width = GridLength.Star }, //3
                            new ColumnDefinition { Width = GridLength.Star }, //4
                            new ColumnDefinition { Width = GridLength.Star }, //5
                            new ColumnDefinition { Width = GridLength.Star }  //6
                        }
                    };

                    SetDataFields(DataField);

                    var frame = new Frame
                    {
                        BorderColor = Colors.Transparent,
                        Padding = new Thickness(8),
                        Margin = new Thickness(0),
                        BackgroundColor = Colors.WhiteSmoke,
                        CornerRadius = 0,
                        Content = scrollGridContent
                    };

                    swipeView.Content = frame;
                    //swipeView.Open(OpenSwipeItem.RightItems, false);
                    //swipeView.Open(OpenSwipeItem.LeftItems, false);

                    return swipeView;
                })
            };
        }

        public override void AddDataColumn(Grid scrollGridContent, int ColIndex, string ColName)
        {
            if (DetailMode == 0)
            {
                if (ColName.Contains("vat") ||
                    ColName.Contains("client_type_display") 
                   )
                {
                    return;
                }
            }

            if (DetailMode == 1)
            {
                if (ColName.Contains("total_due") ||
                    ColName.Contains("total_overdue") ||
                    ColName.Contains("total_to_beat") ||
                    ColName.Contains("positive_balance")

                    )
                {
                    return;
                }
            }

            //DataField = "id, name, vat, email, total_due, total_overdue, total_invoiced, total_to_beat, positive_balance";
            Dictionary<string, string> titlesDictionary = new Dictionary<string, string>
            {
                { "vat", "RUC:" },
                { "email", "EMAIL:" },
                { "total_due", "TOTAL:" },
                { "total_overdue", "VENCIDO:" },
                { "total_to_beat", "X VENCER:" },
                { "positive_balance", "A FAVOR:" },
                { "client_type_display", "TIPO CLIENTE:" },
            };

            Dictionary<string, int> rowsDictionary = new Dictionary<string, int>
            {
                { "title", 0 },
                //{ "name", 0 },
                //{ "vat", -1 },
                //{ "email", -1 },
                { "total_overdue", 1 },
                { "total_to_beat", 1 },
                { "positive_balance", 2 },
                { "total_due", 2 },
                //{ "total_invoiced", 1 },
                { "vat", 1 },
                { "client_type_display", 2 },
            };

            Dictionary<string, int> columnsDictionary = new Dictionary<string, int>
            {
                { "title", 0 },
                //{ "name", 1 },
                //{ "vat", -1 },
                //{ "email", -1 },
                { "total_overdue", 0 },
                { "total_to_beat", 1 },
                { "positive_balance", 0 },
                { "total_due", 1 },
                //{ "total_invoiced", 1 },
                { "vat", 0 },
                { "client_type_display", 0 },
            };

            if (rowsDictionary.TryGetValue(ColName.Trim(), out var valueRow))
            {
                Debug.WriteLine(valueRow);
            }
            else
            {
                //By pass data field
                return;
            }

            if (columnsDictionary.TryGetValue(ColName.Trim(), out var valueCol))
            {
                Debug.WriteLine(valueCol);
            }
            else
            {
                //By pass data field
                return;
            }

            var stackDataItem = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            var lblColumnItem = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.None,
                FontSize = 14,
                Margin = new Thickness(2),
            };

            lblColumnItem.SetBinding(Label.TextProperty, new Binding(ColName));

            //var label1 = new Label
            //{
            //    HorizontalOptions = LayoutOptions.Start,
            //    VerticalOptions = LayoutOptions.Center,
            //    Margin = new Thickness(5, 0, 15, 0)
            //};

            bool useTitle = false;
            string dataTitle = "";

            if (titlesDictionary.TryGetValue(ColName.Trim(), out var value))
            {
                var lblDataTitle = new Label
                {
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.None,
                    FontSize = 14,
                    Margin = new Thickness(2),
                    Text = value
                };

                stackDataItem.Children.Add(lblDataTitle);
            }

            stackDataItem.Children.Add(lblColumnItem);

            scrollGridContent.Children.Add(stackDataItem);
            Grid.SetRow(stackDataItem, valueRow);
            Grid.SetColumn(stackDataItem, valueCol);
            //Grid.SetColumn(stackDataItem, ColIndex);
        }

        public override void SetDataFields(string Fields)
        {
            scrollGridContent.Children.Clear();
            //scrollGridContent.HorizontalOptions = LayoutOptions.Fill;
            //scrollGridContent.BackgroundColor = Colors.WhiteSmoke;

            var fieldsItems = Fields.Split(",");
            
            for (int i = 0; i < fieldsItems.Length; i++)
            {
                var field = fieldsItems[i];
                AddDataColumn(scrollGridContent, i, field);
            }

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Colors.Transparent,
                IsVisible = true,
                Padding = new Thickness(5, 0, 0, 0)
            };

            Button btnSelectSwipeWindows = new Button
            {
                CornerRadius = 0,
                Command = CommandSelectListItem,
                BackgroundColor = Colors.DeepSkyBlue,
                Text = "",
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf058"
                }
            };
            
            btnSelectSwipeWindows.SetBinding(Button.CommandParameterProperty, new Binding("."));

            stackLayout.Children.Add(btnSelectSwipeWindows);
            
            scrollGridContent.Add(stackLayout, 6, 0);
            Grid.SetRow(stackLayout, 0);
            Grid.SetColumn(stackLayout, 6);
            Grid.SetRowSpan(stackLayout, 3);
        }

        //private void PrepareForm()
        //{
        //    IDispatcherTimer timer;

        //    timer = Dispatcher.CreateTimer();
        //    timer.IsRepeating = false;
        //    timer.Interval = TimeSpan.FromMilliseconds(500);
        //    timer.Tick += async (s, e) =>
        //    {
        //        Debug.WriteLine("Cargando los datos...");

        //        //partner = new res_partner {
        //        //    email = "ronald.chonillo@gmail.com",
        //        //    id = 1,
        //        //    name = "Ronald Chonillo",
        //        //    vat = "0919826958"
        //        //};

        //        if(Company==null || Company.id == 0)
        //        {
        //            await App.Current.MainPage.DisplayAlert("Clientes",
        //                                $"Se requiere que se especifique la compañia para poder realizar la búsqueda de clientes.",
        //                                "Continuar");
        //            Close(null);
        //            return;
        //        }

        //        _labelOverTitle.Text = Company.name;

        //        //if ( partner != null )
        //        //{

        //        //}
        //        //else
        //        //{
        //        //    //Si no se ha enviado el partner de origen no se permitirá el ingreso del dato
        //        //    await App.Current.MainPage.DisplayAlert("Nueva cuenta",
        //        //                        $"Se requiere que se especifique el cliente para poder crear nueva cuenta bancaria",
        //        //                        "Continuar");
        //        //    Close(null);
        //        //    return;
        //        //}                

        //        timer.Stop();
        //    };
        //    timer.Start();
        //}

        //public ICommand CommandSelectListItem { get; set; }

        //private async void SelectListItem(object objItem)
        //{            
        //    if (objItem != null)
        //    {
        //        Close(objItem);
        //    }
        //    else
        //    {
        //        Debug.WriteLine("Error de objeto");
        //    }
        //}

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
