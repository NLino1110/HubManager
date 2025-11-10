using DMOrders.Models; // Asegúrate de que aquí esté la definición de tu modelo Activity
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders
{
    public class CrudViewModel : INotifyPropertyChanged
    {
        public ICommand AddLineCommand { get; }
        public res_company CurrentCompany { get; set; }
        public res_partner _CurrentPartner { get; set; }
        public sale_order CurrentSaleOrder { get; set; }
                
        private sale_order_line _selectedItem;

        private ObservableCollection<sale_order_line> _order_lines;

        public ObservableCollection<sale_order_line> OrderLines
        {
            get => _order_lines;
            set
            {
                _order_lines = value;
                OnPropertyChanged();
                UpdateTotals();
            }
        }

        public ObservableCollection<PromotionBenefit> ItemsDataBenefits
        {
            get => _ItemsDataBenefits;
            set
            {
                _ItemsDataBenefits = value;
                OnPropertyChanged(nameof(ItemsDataBenefits));
            }
        }

        private ObservableCollection<PromotionBenefit> _ItemsDataBenefits;

        private ObservableCollection<PromotionEvalResult> _appliedPromotionResults;

        private ObservableCollection<PromotionEvalResult> AppliedPromotionResults
        {
            get => _appliedPromotionResults;
            set
            {
                _appliedPromotionResults = value;

                if (_ItemsDataBenefits != null)
                    _ItemsDataBenefits.Clear();
                else
                    _ItemsDataBenefits = new ObservableCollection<PromotionBenefit>();

                foreach (var promo in _appliedPromotionResults)
                {
                    if (promo.Items != null)
                    {
                        foreach (var benefit in promo.Items)
                        {
                            _ItemsDataBenefits.Add(benefit.Promotion);
                        }
                    }
                }

                OnPropertyChanged(nameof(AppliedPromotionResults));
                OnPropertyChanged(nameof(ItemsDataBenefits));
            }
        }

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

        public decimal Subtotal
        {
            get
            {
                decimal tmp_Subtotal = 0;
                if(OrderLines != null)
                    foreach(var orderLine in OrderLines)
                    {
                        if (orderLine.price_unit == 0)
                            orderLine.price_unit = (decimal) 5.5;

                        orderLine.price_subtotal = orderLine.product_uom_qty * orderLine.price_unit;
                        //orderLine.discount_amount = orderLine.price_subtotal * (orderLine.discount / 100);
                        tmp_Subtotal += orderLine.product_uom_qty * orderLine.price_unit;
                    }
                return tmp_Subtotal;
            }
        }

        /// <summary>
        /// Descuento total aplicado sobre todas las líneas
        /// </summary>
        public decimal Descuento
        {
            get
            {
                decimal tmp_Descuento = 0;
                if (OrderLines != null)
                {
                    foreach (var orderLine in OrderLines)
                    {
                        if (orderLine.discount > 0)
                        {
                            orderLine.amount_discount = orderLine.price_subtotal * (orderLine.discount / 100);
                            tmp_Descuento += orderLine.amount_discount;
                        }
                    }
                }
                return tmp_Descuento;
            }
        }

        /// <summary>
        /// Neto = Subtotal - Descuento
        /// </summary>
        public decimal Neto => Subtotal - Descuento;

        /// <summary>
        /// Impuesto total (suma de los impuestos de cada línea)
        /// </summary>
        public decimal Impuesto
        {
            get
            {
                decimal tmp_Impuesto = 0;
                if (OrderLines != null)
                {
                    foreach (var orderLine in OrderLines)
                    {
                        tmp_Impuesto += orderLine.price_tax;
                    }
                }
                return tmp_Impuesto;
            }
        }

        /// <summary>
        /// Contribución solidaria (si aplica en las líneas)
        /// </summary>
        public decimal CompSolidaria
        {
            get
            {
                decimal tmp_Comp = 0;
                if (OrderLines != null)
                {
                    foreach (var orderLine in OrderLines)
                    {
                        tmp_Comp += orderLine.price_unit;
                    }
                }
                return tmp_Comp;
            }
        }

        /// <summary>
        /// Total general = Neto + Impuesto + Comp. Solidaria
        /// </summary>
        public decimal Total => Neto + Impuesto + CompSolidaria;

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
        public ICommand SaveCommand { get; }
        public ICommand SyncCommand { get; }

        public CrudViewModel()
        {
            OrderLines = new ObservableCollection<sale_order_line>();            
            CloseCommand = new Command(OnClose);           
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

                var saleOrderLinesDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);
                //_ = saleOrderLinesDb.GetItemsAsync(CurrentSaleOrder.id).ContinueWith(task =>
                //{
                //    if (task.IsCompletedSuccessfully)
                //    {
                //        var orderLines = task.Result
                //            .OrderBy(l => l.id) 
                //            .ToList();

                //        foreach (var line in orderLines)
                //        {
                //            ProductProductDb productDb = new ProductProductDb();
                //            productDb.GetItem(line.product_id).ContinueWith(taskProduct =>
                //            {
                //                if (taskProduct.IsCompletedSuccessfully)
                //                {
                //                    var product = taskProduct.Result;
                //                    line.product_code = product.code;
                //                    line.product_display = product.display_name;
                //                }
                //                else
                //                {
                //                    System.Diagnostics.Debug.WriteLine($"Error al obtener productos: {taskProduct.Exception?.Message}");
                //                }

                //                OrderLines.Add(line);
                //            });
                //        }
                //    }
                //});

                var orderLines = await saleOrderLinesDb.GetItemsAsync(CurrentSaleOrder.id);
                foreach (var line in orderLines)
                {
                    var product = await new ProductProductDb(App.Session.odooConnection.DbNameSqlite).GetItem(line.product_id);
                    line.product_code = product.code;
                    line.product_display = product.name; //product.display_name;
                    //line.uom_category_display = product._uom_id;
                    line.uom_category_display = "UND";
                    OrderLines.Add(line);
                }

                UpdateTotals();
            }
        }

        private void OnClose()
        {
            
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

            // Buscar si el producto ya existe en la lista
            var existingLine = OrderLines.FirstOrDefault(l => l.product_id == product.id);

            if (existingLine != null)
            {
                // Si existe, aumentar la cantidad
                existingLine.product_uom_qty_real += 1;
                existingLine.product_uom_qty += 1;

                // Recalcular totales (si aplica)
                existingLine.price_total = existingLine.qty_to_deliver * (decimal)product.list_price;
                existingLine.price_subtotal = existingLine.price_total; // o el cálculo que corresponda
                OnPropertyChanged(nameof(OrderLines));                
            }
            else
            {
                // Si no existe, agregar una nueva línea
                var line = new sale_order_line
                {
                    product_id = product.id,
                    product_display = product.name,
                    product_code = product.code,
                    qty_to_deliver = 1,
                    product_uom_qty_real = 1,
                    product_uom_qty = 1,
                    uom_category_display = "UND",
                    price_subtotal = 5,
                    discount = 15,
                    price_tax = 8,
                    price_total = (decimal)product.list_price,
                };

                OrderLines.Add(line);
            }

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Descuento));
            OnPropertyChanged(nameof(Neto));
            OnPropertyChanged(nameof(Impuesto));
            OnPropertyChanged(nameof(CompSolidaria));
            OnPropertyChanged(nameof(Total));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
