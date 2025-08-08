using CommunityToolkit.Mvvm.Input;
using DMOrdersUI.Controls.CustomRows;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrdersUI.Pages.Fragments.Orders
{
    public class ListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<sale_order> Orders { get; set; } = new();

        private sale_order _selectedItem;
        public sale_order SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();                   
                }
            }
        }


        public ListViewModel()
        {
            LoadData();

            //EditCommand = new Command(EditItem);
            //EditCommand = new RelayCommand<ActivityHeader>(EditItem);
        }

        public void LoadData()
        {
            Orders.Clear();

            int company_id = App.Session.res_Company.id;

            SaleOrderDb saleOrderDb = new SaleOrderDb();
            saleOrderDb.GetItemsAsync(company_id).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var items = task.Result;

                    foreach (var item in items)
                    {
                        //Orders.Add(item);

                        ResPartnerDb resPartnerDb = new ResPartnerDb();
                        resPartnerDb.GetItemsAsync(item._company_id, item._partner_id).ContinueWith(taskPartner =>
                        {
                            if (taskPartner.IsCompletedSuccessfully)
                            {
                                var partner = taskPartner.Result;
                                //var existingItem = Orders.FirstOrDefault(o => o.id == item.id);
                                //if (existingItem != null)
                                //{
                                    //existingItem.partner_display = partner?.name ?? "-";
                                    item.partner_display = partner?.name ?? "-";
                                //}
                            }
                            else
                            {
                                Debug.WriteLine($"Error cargando partner: {taskPartner.Exception?.Message}");
                            }

                            Orders.Add(item);
                        });
                    }
                }
                else
                {
                    Debug.WriteLine($"Error al obtener órdenes: {task.Exception?.Message}");
                }
            });
        }

        //public ICommand EditCommand { get; set; }

        //private async void EditItem(object obj)
        //{
        //    Debug.WriteLine("EditItem");
        //    Details viewObj = new Details();
        //    //objPage.Sel_AccountMoveSendHeader = (AccountMoveSendHeader)obj;
        //    //objPage.editionMode = true;
        //    //objPage.Disappearing += NewGroup_Disappearing;
        //    //await Navigation.PushAsync(viewObj, false);
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
