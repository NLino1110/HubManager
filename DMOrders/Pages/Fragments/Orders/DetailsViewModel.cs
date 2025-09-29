using DMOrders.Models; // Asegúrate de que aquí esté la definición de tu modelo Activity
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders
{
    public class DetailsViewModel : INotifyPropertyChanged
    {
        public ICommand AddLineCommand { get; }
        public res_company CurrentCompany { get; set; }
        public res_partner _CurrentPartner { get; set; }
        public sale_order CurrentSaleOrder { get; set; }

        private ObservableCollection<sale_order_line> _order_lines;
        private sale_order_line _selectedItem;

        public string Note
        {
            get => CurrentSaleOrder?.note;
            set
            {
                if (CurrentSaleOrder != null && CurrentSaleOrder.note != value)
                {
                    CurrentSaleOrder.note = value;
                    OnPropertyChanged(nameof(Note));
                }
            }
        }

        public ObservableCollection<sale_order_line> OrderLines
        {
            get => _order_lines;
            set
            {
                _order_lines = value;
                OnPropertyChanged();
            }
        }

        public sale_order_line SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SyncCommand { get; }

        public DetailsViewModel()
        {
            OrderLines = new ObservableCollection<sale_order_line>();
            //LoadData();

            CloseCommand = new Command(OnClose);
            NewCommand = new Command(OnNew);
            SaveCommand = new Command(OnSave);
            SyncCommand = new Command(OnSync);

            AddLineCommand = new Command<product_product>(OnAddLine);
        }

        public async Task AddOrderLine(sale_order_line NewOrderLine)
        {
            OrderLines.Add(NewOrderLine);
        }

        public async Task LoadData()
        {
            OrderLines.Clear();

            if (CurrentSaleOrder != null)
            {
                OnPropertyChanged(nameof(Note));

                SaleOrderLineDb saleOrderLinesDb = new SaleOrderLineDb();
                _ = saleOrderLinesDb.GetItemsAsync(CurrentSaleOrder.id).ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        var orderLines = task.Result;
                        foreach (var line in orderLines)
                        {                         
                            ProductProductDb productDb = new ProductProductDb();
                            productDb.GetItem(line.product_id).ContinueWith(taskProduct =>
                            {
                                if (taskProduct.IsCompletedSuccessfully)
                                {
                                    var product = taskProduct.Result;
                                    line.product_code = product.code;
                                    line.product_display = product.display_name;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error al obtener productos: {taskProduct.Exception?.Message}");
                                }

                                OrderLines.Add(line);
                            });                            
                        }
                    }
                });                
            }
        }

        private void OnClose()
        {
            
        }

        private void OnNew()
        {
            var newLine = new sale_order_line { id = 2, product_id = 20777, product_display = "[ST-9700] *** SILETI MALETIN COSMETIQUERO", product_uom_qty = 1, price_unit = 3, discount = 0, price_subtotal = 0, price_tax = 0, price_total = 0 };
            OrderLines.Add(newLine);
            SelectedItem = newLine;
        }

        private void OnSave()
        {
            
        }

        private async void OnSync()
        {            
            await Task.Delay(1000);
        }

        private void OnAddLine(product_product product)
        {
            if (product is null) return;

            // Lógica para convertir product_product -> SaleOrderLine
            var line = new sale_order_line
            {
                product_id = product.id,
                product_display = product.name,
                product_qty = 1,
                price_total = (decimal) product.list_price,
                // ... lo que corresponda
            };

            OrderLines.Add(line);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
