using DMOrders.Models; // Asegúrate de que aquí esté la definición de tu modelo Activity
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using Microsoft.Maui.Controls.Shapes;
using MPowerKit.VirtualizeListView;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders
{
    public class CrudViewModel : INotifyPropertyChanged
    {
        public List<SaleOrderPromotions> saleOrderPromotions { get; set; }
        public ICommand AddLineCommand { get; }
        public ICommand AddLineCommandByQty { get; }
        public res_company CurrentCompany { get; set; }
        public res_partner _CurrentPartner { get; set; }
        public sale_order CurrentSaleOrder { get; set; }
        public product_pricelist CurrentPriceList { get; set; }

        private sale_order_line _selectedItem;

        private ObservableCollection<sale_order_line> _order_lines;

        int decimalPositions = 4;

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

        private ObservableCollection<PromotionEvalResultV2> _appliedPromotionResults;

        private ObservableCollection<PromotionEvalResultV2> AppliedPromotionResults
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
                Debug.WriteLine($"==================================");
                Debug.WriteLine("Calculo Subtotal");

                decimal tmp_Subtotal = 0;
                if(OrderLines != null)
                    foreach(var orderLine in OrderLines)
                    {
                        //if (orderLine.virtual_price_no_tax == 0)
                        //    orderLine.virtual_price_no_tax = (decimal) 5.5;

                        //orderLine.price_subtotal = orderLine.product_uom_qty * orderLine.virtual_price_no_tax;
                        //////orderLine.discount_amount = orderLine.price_subtotal * (orderLine.discount / 100);
                        //tmp_Subtotal += orderLine.product_uom_qty * orderLine.virtual_price_no_tax;
                        //tmp_Subtotal += orderLine.price_subtotal;
                        //tmp_Subtotal += orderLine.price_total - orderLine.amount_discount;
                        tmp_Subtotal += orderLine.price_subtotal;
                        Debug.WriteLine($"price_subtotal: {orderLine.price_subtotal}");
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
                        //tmp_Impuesto += (orderLine.virtual_price_no_tax * orderLine._virtual_iva_percentage) / 100;
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
        /// Total general = Neto + Impuesto
        /// </summary>
        public decimal Total => Subtotal + Impuesto;

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
            saleOrderPromotions = new List<SaleOrderPromotions>();
            CloseCommand = new Command(OnClose);           
            SaveCommand = new Command(OnSave);
            SyncCommand = new Command(OnSync);
            AddLineCommand = new Command<product_product>(OnAddLine);
            AddLineCommandByQty = new Command<ItemPickedArgs>(OnAddLine);
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

                var product_ids = orderLines.Select(ol => ol.product_id).Distinct().ToArray();
                var productsList = await new ProductProductDb(App.Session.odooConnection.DbNameSqlite).GetByProductsIds(product_ids, 0);

                foreach (var line in orderLines)
                {
                    var product = productsList.FirstOrDefault(p => p.id == line.product_id);
                    line.product_code = product.code;
                    line.product_display = product.name;
                    line.uom_category_display = product.uom_display;
                    OrderLines.Add(line);
                }

                //foreach (var line in orderLines)
                //{
                //    var product = await new ProductProductDb(App.Session.odooConnection.DbNameSqlite).GetItem(line.product_id);
                //    line.product_code = product.code;
                //    line.product_display = product.name; //product.display_name;
                //    //line.uom_category_display = product._uom_id;
                //    line.uom_category_display = "UND";
                //    OrderLines.Add(line);
                //}

                var saleOrderPromotionsDb = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
                var saleOrderPromotions_tmp = await saleOrderPromotionsDb.GetItemsByOrder(CurrentSaleOrder.id);

                foreach (var sop in saleOrderPromotions_tmp)
                {
                    saleOrderPromotions.Add(sop);
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

        public class PriceCalculationResult
        {
            public decimal Price { get; set; }                 // Precio final (con IVA)
            public decimal PriceWithoutIva { get; set; }       // Precio sin IVA
            public decimal DiscountAmount { get; set; }         // Valor del descuento
            public decimal DiscountPercent { get; set; }       // Porcentaje del descuento
            public decimal TotalLine { get; set; }             // Total multiplicado por cantidad
            public decimal PriceTax { get; set; }
            public decimal IvaPercentage { get; set; }
            public decimal PriceSubtotal { get; set; }
            public decimal LineSubtotal { get; set; }
            public bool ExistsInPriceList { get; set; }            
        }

        private async Task<PriceCalculationResult> getPriceWithPricelistWithSearch(
            product_product product,
            product_pricelist product_Pricelist,
            decimal quantity)
        {
            bool ExistsInPriceList = false;
            var accountTaxDb = new AccountTaxDb(App.Session.odooConnection.DbNameSqlite);
            var tax_sale = await accountTaxDb.GetItem(product._taxes_id);

            var priceListProductsDb = new ProductPricelistItemDb(App.Session.odooConnection.DbNameSqlite);

            //decimal list_price = (decimal) product.list_price;  // NO SE VA A USAR ESTE CAMPO
            decimal price_list_value = 0m;
            decimal discount_percent = 0m;
            decimal discount_value = 0m;

            var priceListItem = await priceListProductsDb.GetItemAsync(x =>
                x._product_tmpl_id == product._product_tmpl_id &&
                x._pricelist_id == product_Pricelist.id);

            if (priceListItem != null)
            {
                switch (priceListItem.compute_price)
                {
                    case "fixed":
                        price_list_value = priceListItem.fixed_price;
                        discount_percent = 0;
                        discount_value = 0;

                        //discount_percent = ((list_price - price_list_value) / list_price) * 100;
                        //if (discount_percent < 2)
                        //    discount_percent = 0;

                        //if (discount_percent > 0)
                        //    discount_value = list_price - price_list_value;

                        break;

                    case "percentage":
                        //TODO: No debe usarse
                        decimal list_price = priceListItem.fixed_price;
                        price_list_value = list_price - (list_price * (priceListItem.percent_price / 100));
                        discount_percent = priceListItem.percent_price;
                        if (discount_percent > 0)
                            discount_value = list_price * (discount_percent / 100m);

                        break;

                    default:
                        price_list_value = priceListItem.fixed_price;
                        discount_percent = 0;
                        discount_value = 0;
                        break;
                }

                ExistsInPriceList = true;
            }

            // IVA (ya incluido en el precio de lista)
            decimal iva_tax = (decimal) tax_sale.amount; //15m;
            decimal factor_iva = 1 + (iva_tax / 100m);

            decimal price_without_iva = price_list_value / factor_iva;            
            decimal total_line = (price_list_value * quantity) - discount_value;

            return new PriceCalculationResult
            {
                Price = price_list_value,
                PriceWithoutIva = price_without_iva,
                DiscountAmount = discount_value,
                DiscountPercent = discount_percent,
                TotalLine = total_line,
                IvaPercentage = iva_tax,
                PriceTax = (price_without_iva * quantity * iva_tax) / 100,
                ExistsInPriceList = ExistsInPriceList
            };
        }

        private async Task<PriceCalculationResult> getPriceWithPricelist(
            product_product product,
            product_pricelist product_Pricelist,
            decimal quantity)
        {
            int decimalPositions = 4;
            bool ExistsInPriceList = false;
            var accountTaxDb = new AccountTaxDb(App.Session.odooConnection.DbNameSqlite);
            var tax_sale = await accountTaxDb.GetItem(product._taxes_id);

            //decimal list_price = (decimal) product.list_price;  // NO SE VA A USAR ESTE CAMPO
            decimal price_list_value = 0m;
            decimal discount_percent = 0m;
            decimal discount_value = 0m;
            
            if (product.list_price > 0)
            {               
                price_list_value = (decimal) product.list_price;
                discount_percent = 0;
                discount_value = 0;
                ExistsInPriceList = true;
            }

            // IVA (ya incluido en el precio de lista)
            decimal iva_tax = (decimal)tax_sale.amount; //15m;
            decimal factor_iva = 1 + (iva_tax / 100m);

            decimal price_without_iva = Math.Round(price_list_value / factor_iva, 7);
            decimal total_line = Math.Round((price_list_value * quantity) - discount_value, decimalPositions);

            //return new PriceCalculationResult
            //{
            //    Price = price_list_value,
            //    PriceWithoutIva = price_without_iva,
            //    DiscountAmount = discount_value,
            //    DiscountPercent = discount_percent,
            //    TotalLine = total_line,
            //    IvaPercentage = iva_tax,
            //    PriceTax = (price_without_iva * quantity * iva_tax) / 100,
            //    ExistsInPriceList = ExistsInPriceList
            //};

            return new PriceCalculationResult
            {
                Price = Math.Round(price_list_value, decimalPositions),
                PriceWithoutIva = price_without_iva,
                DiscountAmount = Math.Round(discount_value, decimalPositions),
                DiscountPercent = Math.Round(discount_percent, decimalPositions),
                TotalLine = total_line,
                IvaPercentage = Math.Round(iva_tax, decimalPositions),
                PriceTax = Math.Round((price_without_iva * quantity * iva_tax) / 100, decimalPositions),
                PriceSubtotal = (price_without_iva * quantity) - discount_value,
                LineSubtotal = price_without_iva * quantity,
                ExistsInPriceList = ExistsInPriceList
            };
        }

        private async void OnAddLine(product_product product)
        {
            if (product is null) return;

            // Buscar si el producto ya existe en la lista
            var existingLine = OrderLines.FirstOrDefault(l => l.product_id == product.id);

            if (existingLine != null)
            {
                // Si existe, aumentar la cantidad
                existingLine.product_uom_qty_real += 1;
                existingLine.product_uom_qty += 1;

                var priceCalc = await getPriceWithPricelist(product, CurrentPriceList, existingLine.product_uom_qty);

                //product.list_price = (float) priceCalc.Price;
                existingLine.price_total = priceCalc.TotalLine;
                existingLine.price_unit = priceCalc.Price;
                existingLine.price_subtotal = priceCalc.PriceSubtotal; //(priceCalc.PriceWithoutIva * existingLine.product_uom_qty) - priceCalc.DiscountAmount;
                existingLine.discount = priceCalc.DiscountPercent;
                existingLine.amount_discount = priceCalc.DiscountAmount;
                existingLine.price_tax = priceCalc.PriceTax;
                existingLine.virtual_price_no_tax = priceCalc.PriceWithoutIva;
                existingLine.virtual_iva_percentage = priceCalc.IvaPercentage;
                existingLine.virtual_line_subtotal = priceCalc.LineSubtotal;
                existingLine.product_tmpl_id = product._product_tmpl_id;
                OnPropertyChanged(nameof(OrderLines));
            }
            else
            {
                var priceCalc = await getPriceWithPricelist(product, CurrentPriceList, 1);

                if (priceCalc.ExistsInPriceList)
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
                        price_subtotal = priceCalc.PriceSubtotal, // (priceCalc.PriceWithoutIva * 1) - priceCalc.DiscountAmount,
                        discount = priceCalc.DiscountPercent,
                        amount_discount = priceCalc.DiscountAmount,
                        price_tax = priceCalc.PriceTax,
                        price_unit = priceCalc.Price,
                        price_total = priceCalc.TotalLine,
                        virtual_price_no_tax = priceCalc.PriceWithoutIva,
                        virtual_iva_percentage = priceCalc.IvaPercentage,
                        virtual_line_subtotal = priceCalc.LineSubtotal,
                        product_tmpl_id = product._product_tmpl_id
                    };

                    OrderLines.Add(line);
                }
                else
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Warning", "Producto no se puede agregar porque no existe en la lista de precios.", "OK");
                    return;
                }
            }

            UpdateTotals();
        }



        private async void OnAddLine(ItemPickedArgs itemPickedArgs)
        {
            if (itemPickedArgs is null) return;

            var product = itemPickedArgs.product;
            var qty_real = itemPickedArgs.qty_real;
            var qty_sol = itemPickedArgs.qty_sol;
            // Buscar si el producto ya existe en la lista
            var existingLine = OrderLines.FirstOrDefault(l => l.product_id == product.id && !l.is_gift);

            if (existingLine != null)
            {
                // Si existe, aumentar la cantidad
                existingLine.product_uom_qty_real += qty_real;
                existingLine.product_uom_qty += qty_sol;

                var priceCalc = await getPriceWithPricelist(product, CurrentPriceList, existingLine.product_uom_qty);

                //product.list_price = (float) priceCalc.Price;
                existingLine.price_total = priceCalc.TotalLine;
                existingLine.price_unit = priceCalc.Price;
                existingLine.price_subtotal = priceCalc.PriceSubtotal; //(priceCalc.PriceWithoutIva * existingLine.product_uom_qty) - priceCalc.DiscountAmount;
                existingLine.discount = priceCalc.DiscountPercent;
                existingLine.amount_discount = priceCalc.DiscountAmount;
                existingLine.price_tax = priceCalc.PriceTax;
                existingLine.virtual_price_no_tax = priceCalc.PriceWithoutIva;
                existingLine.virtual_iva_percentage = priceCalc.IvaPercentage;
                existingLine.virtual_line_subtotal = priceCalc.LineSubtotal;
                existingLine.product_tmpl_id = product._product_tmpl_id;
                OnPropertyChanged(nameof(OrderLines));
            }
            else
            {
                var priceCalc = await getPriceWithPricelist(product, CurrentPriceList, 1);

                if (priceCalc.ExistsInPriceList)
                {
                    // Si no existe, agregar una nueva línea
                    var line = new sale_order_line
                    {
                        product_id = product.id,
                        product_display = product.name,
                        product_code = product.code,
                        qty_to_deliver = qty_sol,
                        product_uom_qty_real = qty_real,
                        product_uom_qty = qty_sol,
                        uom_category_display = "UND",
                        price_subtotal = priceCalc.PriceSubtotal, // (priceCalc.PriceWithoutIva * 1) - priceCalc.DiscountAmount,
                        discount = priceCalc.DiscountPercent,
                        amount_discount = priceCalc.DiscountAmount,
                        price_tax = priceCalc.PriceTax,
                        price_unit = priceCalc.Price,
                        price_total = priceCalc.TotalLine,
                        virtual_price_no_tax = priceCalc.PriceWithoutIva,
                        virtual_iva_percentage = priceCalc.IvaPercentage,
                        virtual_line_subtotal = priceCalc.LineSubtotal,
                        product_tmpl_id = product._product_tmpl_id
                    };

                    OrderLines.Add(line);
                }
                else
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Warning", "Producto no se puede agregar porque no existe en la lista de precios.", "OK");
                    return;
                }
            }

            UpdateTotals();
        }



        public async void UpdateOrderLine(sale_order_line sale_Order_Line,  product_product product)
        {
            if (sale_Order_Line != null)
            {
                if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                {
                    List<PromotionEvalItemV2> promotionEvalItemParent = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(sale_Order_Line.promotion_data);
                    if (promotionEvalItemParent != null && promotionEvalItemParent.Count > 0)
                    {
                        foreach (var benefitItem in promotionEvalItemParent)
                        {
                            if (benefitItem.Promotion._promotion_type_id == 2 && benefitItem.Promotion._selection_type_id == 2)
                            {
                                //Si contiene regalos asociados manuales, no se procede a recalcular
                                return;
                            }
                        }
                    }
                }


                var priceCalc = await getPriceWithPricelist(product, CurrentPriceList, sale_Order_Line.product_uom_qty);
                sale_Order_Line.price_total = priceCalc.TotalLine;
                sale_Order_Line.price_unit = priceCalc.Price;
                sale_Order_Line.price_subtotal = priceCalc.PriceSubtotal; // (priceCalc.PriceWithoutIva * sale_Order_Line.product_uom_qty) - priceCalc.DiscountAmount;
                sale_Order_Line.price_tax = priceCalc.PriceTax; //(priceCalc.PriceWithoutIva * sale_Order_Line.product_uom_qty * priceCalc.IvaPercentage) / 100;
                sale_Order_Line.discount = priceCalc.DiscountPercent;
                sale_Order_Line.amount_discount = priceCalc.DiscountAmount;
                sale_Order_Line.virtual_price_no_tax = priceCalc.PriceWithoutIva;
                sale_Order_Line.virtual_iva_percentage = priceCalc.IvaPercentage;
                sale_Order_Line.virtual_line_subtotal = priceCalc.LineSubtotal;
                sale_Order_Line.product_tmpl_id = product._product_tmpl_id;

                if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                {
                    List<PromotionEvalItemV2> benefitFromData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(sale_Order_Line.promotion_data);
                    if (benefitFromData != null && benefitFromData.Count > 0)
                    {
                        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
                        foreach (var benefit in benefitFromData)
                        {
                            await promotionEngineRunner.AddApplyPromotion(CurrentSaleOrder, benefit, -1, saleOrderPromotions);
                        }
                    }                    
                }

                sale_Order_Line.promotion_data = "";

                var giftList = OrderLines.Where(x => x.is_gift).ToList();

                if (giftList != null)
                {
                    foreach (var itemGift in giftList)
                    {
                        if (itemGift.promotion_data != null)
                        {
                            List<PromotionEvalItemV2> promotionEvalItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(itemGift.promotion_data);
                            Debug.WriteLine(promotionEvalItem);

                            if (promotionEvalItem != null && promotionEvalItem.Count > 0)
                            {
                                foreach (var benefitItem in promotionEvalItem)
                                {
                                    foreach (var ruleMatch in benefitItem.RuleSet)
                                    {
                                        int[] listIdsProd = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);

                                        foreach (var productIdCompare in listIdsProd)
                                        {
                                            if (sale_Order_Line.product_tmpl_id == productIdCompare && (sale_Order_Line.is_gift && !sale_Order_Line.is_manual))
                                            {
                                                OrderLines.Remove(itemGift);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                //OnPropertyChanged(nameof(OrderLines));

                //foreach (var item in OrderLines.Where(x => x.product_id_origin == sale_Order_Line.product_id).ToList())
                //{
                //    OrderLines.Remove(item);
                //}               

            }

            UpdateTotals();
        }


        public async void UpdateOrderLineRefresh(sale_order_line sale_Order_Line, product_product product)
        {
            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

            if (sale_Order_Line != null)
            {
                if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                {
                    List<PromotionEvalItemV2> promotionEvalItemParent = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(sale_Order_Line.promotion_data);
                    if (promotionEvalItemParent != null && promotionEvalItemParent.Count > 0)
                    {
                        foreach (var benefitItem in promotionEvalItemParent)
                        {
                            if (benefitItem.Promotion._promotion_type_id == 2 && benefitItem.Promotion._selection_type_id == 2)
                            {
                                //Si contiene regalos asociados manuales, no se procede a recalcular
                                return;
                            }
                        }
                    }
                }

                var priceCalc = await getPriceWithPricelist(product, CurrentPriceList, sale_Order_Line.product_uom_qty);
                sale_Order_Line.price_total = priceCalc.TotalLine;
                sale_Order_Line.price_unit = priceCalc.Price;
                sale_Order_Line.price_subtotal = priceCalc.PriceSubtotal; // (priceCalc.PriceWithoutIva * sale_Order_Line.product_uom_qty) - priceCalc.DiscountAmount;
                sale_Order_Line.price_tax = priceCalc.PriceTax; //(priceCalc.PriceWithoutIva * sale_Order_Line.product_uom_qty * priceCalc.IvaPercentage) / 100;
                sale_Order_Line.discount = priceCalc.DiscountPercent;
                sale_Order_Line.amount_discount = priceCalc.DiscountAmount;
                sale_Order_Line.virtual_price_no_tax = priceCalc.PriceWithoutIva;
                sale_Order_Line.virtual_iva_percentage = priceCalc.IvaPercentage;
                sale_Order_Line.virtual_line_subtotal = priceCalc.LineSubtotal;
                sale_Order_Line.product_tmpl_id = product._product_tmpl_id;

                //Se resetea los regalos asignados
                //sale_Order_Line.max_gifts = 0;
                //sale_Order_Line.assigned_gifts = 0;

                await promotionEngineRunner.ResetManualGiftBenefit(CurrentSaleOrder, saleOrderPromotions);

                if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                {
                    List<PromotionEvalItemV2> benefitFromData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(sale_Order_Line.promotion_data);
                    if (benefitFromData != null && benefitFromData.Count > 0)
                    {                        
                        foreach (var benefit in benefitFromData)
                        {
                            await promotionEngineRunner.AddApplyPromotion(CurrentSaleOrder, benefit, -1, saleOrderPromotions);
                        }
                    }
                }
                
                //for (var io = 0; io < OrderLines.Count(); io++)
                //{
                //    var lineMemory = OrderLines[io];
                //    if (!string.IsNullOrEmpty(lineMemory.promotion_data) && !lineMemory.is_gift)
                //    {
                //        List<PromotionEvalItemV2> promotionEvalItemParentMem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineMemory.promotion_data);
                //        if (promotionEvalItemParentMem != null && promotionEvalItemParentMem.Count > 0)
                //        {
                //            foreach (var benefitItem in promotionEvalItemParentMem)
                //            {
                //                if (benefitItem.Promotion._promotion_type_id == 2 && benefitItem.Promotion._selection_type_id == 2)
                //                {
                //                    var arrayProdTmpls = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(benefitItem.RuleSet[0].ProductTmplIds);
                //                    if(arrayProdTmpls.Any())
                //                    {
                //                        for(var xp=0; xp < arrayProdTmpls.Length; xp++)
                //                        {

                //                            var productInRule = OrderLines.Where(x=> x.product_tmpl_id == arrayProdTmpls[xp] && !x.is_gift).FirstOrDefault();

                //                            if (productInRule != null)
                //                            {
                //                                productInRule.promotion_data = "";
                //                                await promotionEngineRunner.AddApplyPromotion(CurrentSaleOrder, benefitItem, -1, saleOrderPromotions);                                                
                //                            }
                //                        }
                //                    }                                    
                //                }
                //            }
                //        }
                //    }
                //}

                //Se resetea las promociones tipo regalo manual
                // y se buscan otras lineas principales que podrian estar relacionadas a la misma promocion
                for (var ib = 0; ib < saleOrderPromotions.Count(); ib++)
                {
                    var benefitMemory = saleOrderPromotions[ib];

                    if (benefitMemory.promotion_type_id == 2 && benefitMemory.promotion_selection_type_id == 2)
                    {
                        saleOrderPromotions.Remove(benefitMemory);
                    }
                }

                sale_Order_Line.promotion_data = "";

                var giftList = OrderLines.Where(x => x.is_gift).ToList();

                if (giftList != null)
                {
                    foreach (var itemGift in giftList)
                    {
                        if (itemGift.promotion_data != null)
                        {
                            List<PromotionEvalItemV2> promotionEvalItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(itemGift.promotion_data);
                            Debug.WriteLine(promotionEvalItem);

                            if (promotionEvalItem != null && promotionEvalItem.Count > 0)
                            {
                                foreach (var benefitItem in promotionEvalItem)
                                {
                                    foreach (var ruleMatch in benefitItem.RuleSet)
                                    {
                                        int[] listIdsProd = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);

                                        foreach (var productIdCompare in listIdsProd)
                                        {
                                            if (sale_Order_Line.product_tmpl_id == productIdCompare)
                                            {
                                                OrderLines.Remove(itemGift);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            UpdateTotals();
        }

        public async void RemoveOrderLine(sale_order_line sale_Order_Line) //, product_product product)
        {
            if (sale_Order_Line != null)
            {
                //Cuando es un producto main (asumiendo que contenga regalos asociados)
                //Se buscan los regalos asociados a la línea seleccionada
                // para eliminarlos también
                if (!sale_Order_Line.is_gift)
                {                    
                    var giftList = OrderLines.Where(x => x.is_gift).ToList();

                    if (giftList != null)
                    {
                        foreach (var itemGift in giftList)
                        {
                            if (itemGift.promotion_data != null)
                            {
                                List<PromotionEvalItemV2> promotionEvalItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(itemGift.promotion_data);
                                Debug.WriteLine(promotionEvalItem);

                                if (promotionEvalItem != null && promotionEvalItem.Count > 0)
                                {
                                    foreach (var evalItem in promotionEvalItem)
                                    {
                                        foreach(var ruleMatch in evalItem.RuleSet)
                                        {
                                            int[] listIdsProd = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);

                                            foreach (var productIdCompare in listIdsProd)
                                            {
                                                if (sale_Order_Line.product_tmpl_id == productIdCompare)
                                                {
                                                    OrderLines.Remove(itemGift);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Si es que es un regalo
                    //Se busca el producto principal para reducirle la cantidad de regalos asociados
                    var mainOrderLine = OrderLines.Where(x => x.product_id == sale_Order_Line.product_id_origin).FirstOrDefault();
                    if(mainOrderLine != null)
                    {
                        //mainOrderLine.assigned_gifts = mainOrderLine.assigned_gifts - (int) sale_Order_Line.product_uom_qty_real;

                        List<PromotionEvalItemV2> benefitFromData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(mainOrderLine.promotion_data);
                        if (benefitFromData != null && benefitFromData.Count > 0)
                        {
                            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
                            foreach (var benefit in benefitFromData)
                            {
                                var dataBenefit = await promotionEngineRunner.GetDataBenefit(CurrentSaleOrder, benefit, saleOrderPromotions);
                                dataBenefit.assigned_gifts = dataBenefit.assigned_gifts - (int)sale_Order_Line.product_uom_qty_real;

                                await promotionEngineRunner.AddApplyPromotion(CurrentSaleOrder, benefit, -1, saleOrderPromotions);
                            }
                        }
                    }
                }

                //Se borra linea seleccionada
                OrderLines.Remove(sale_Order_Line);
                UpdateTotals();
            }
        }

        public void UpdateTotals()
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
