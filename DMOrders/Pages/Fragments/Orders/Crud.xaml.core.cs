using System.Globalization;
using DMOrders.Models;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Sync.Core;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders
{
    public partial class Crud
    {
        public ICommand AddLineCommand { get; }
        public ICommand AddLineCommandByQty { get; }

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

        public string Note2
        {
            get => CurrentSaleOrder?.note2;
            set
            {
                if (CurrentSaleOrder != null && CurrentSaleOrder.note2 != value)
                {
                    CurrentSaleOrder.note2 = value;
                    OnPropertyChanged(nameof(Note2));
                }
            }
        }

        private decimal _subtotal;
        private bool _subtotalDirty = true;

        public decimal Subtotal
        {
            get
            {
                if (_subtotalDirty)
                {
                    _subtotal = OrderLines?.Sum(x => x?.price_subtotal ?? 0) ?? 0;
                    _subtotalDirty = false;
                }
                return _subtotal;
            }
        }

        //public decimal Subtotal
        //{
        //    get
        //    {
        //        Debug.WriteLine($"==================================");
        //        Debug.WriteLine("Calculo Subtotal");

        //        decimal tmp_Subtotal = 0;
        //        if(OrderLines != null)
        //            foreach(var orderLine in OrderLines)
        //            {
        //                //if (orderLine.virtual_price_no_tax == 0)
        //                //    orderLine.virtual_price_no_tax = (decimal) 5.5;

        //                //orderLine.price_subtotal = orderLine.product_uom_qty * orderLine.virtual_price_no_tax;
        //                //////orderLine.discount_amount = orderLine.price_subtotal * (orderLine.discount / 100);
        //                //tmp_Subtotal += orderLine.product_uom_qty * orderLine.virtual_price_no_tax;
        //                //tmp_Subtotal += orderLine.price_subtotal;
        //                //tmp_Subtotal += orderLine.price_total - orderLine.amount_discount;
        //                tmp_Subtotal += orderLine.price_subtotal;
        //                Debug.WriteLine($"price_subtotal: {orderLine.price_subtotal}");
        //            }
        //        return tmp_Subtotal;
        //    }
        //}

        /// <summary>
        /// Descuento total aplicado sobre todas las líneas
        /// </summary>
        //public decimal Descuento
        //{
        //    get
        //    {
        //        decimal tmp_Descuento = 0;
        //        if (OrderLines != null)
        //        {
        //            foreach (var orderLine in OrderLines)
        //            {
        //                if (orderLine.discount > 0)
        //                {                            
        //                    tmp_Descuento += orderLine.amount_discount;
        //                }
        //            }
        //        }
        //        return tmp_Descuento;
        //    }
        //}

        private decimal _descuento;
        private bool _descuentoDirty = true;

        public decimal Descuento
        {
            get
            {
                if (_descuentoDirty)
                {
                    _descuento = OrderLines?.Sum(x => x?.amount_discount ?? 0) ?? 0;
                    _descuentoDirty = false;
                }
                return _descuento;
            }
        }

        /// <summary>
        /// Neto = Subtotal - Descuento
        /// </summary>
        public decimal Neto => Subtotal - Descuento;

        /// <summary>
        /// Impuesto total (suma de los impuestos de cada línea)
        /// </summary>
        //public decimal Impuesto
        //{
        //    get
        //    {
        //        decimal tmp_Impuesto = 0;
        //        if (OrderLines != null)
        //        {
        //            foreach (var orderLine in OrderLines)
        //            {
        //                //tmp_Impuesto += (orderLine.virtual_price_no_tax * orderLine._virtual_iva_percentage) / 100;
        //                tmp_Impuesto += orderLine.price_tax;
        //            }
        //        }
        //        return tmp_Impuesto;
        //    }
        //}

        private decimal _impuesto;
        private bool _impuestoDirty = true;

        public decimal Impuesto
        {
            get
            {
                if (_impuestoDirty)
                {
                    _impuesto = OrderLines?.Sum(x => x?.price_tax ?? 0) ?? 0;
                    _impuestoDirty = false;
                }
                return _impuesto;
            }
        }

        /// <summary>
        /// Contribución solidaria (si aplica en las líneas)
        /// </summary>
        //public decimal CompSolidaria
        //{
        //    get
        //    {
        //        decimal tmp_Comp = 0;
        //        if (OrderLines != null)
        //        {
        //            foreach (var orderLine in OrderLines)
        //            {
        //                tmp_Comp += orderLine.price_unit;
        //            }
        //        }
        //        return tmp_Comp;
        //    }
        //}

        private decimal _compSolidaria;
        private bool _compSolidariaDirty = true;

        public decimal CompSolidaria
        {
            get
            {
                if (_compSolidariaDirty)
                {
                    _compSolidaria = OrderLines?.Sum(x => x?.price_unit ?? 0) ?? 0;
                    _compSolidariaDirty = false;
                }
                return _compSolidaria;
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

        //public ICommand CloseCommand { get; }        
        //public ICommand SaveCommand { get; }
        //public ICommand SyncCommand { get; }

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
                OnPropertyChanged(nameof(Note2));

                var saleOrderLinesDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);
                
                var orderLines = await saleOrderLinesDb.GetItemsAsync(CurrentSaleOrder.id);

                var product_ids = orderLines.Select(ol => ol.product_id).Distinct().ToArray();
                //var productsList = await new ProductProductDb(App.Session.odooConnection.DbNameSqlite).GetByProductsIds(product_ids, 0);
                var productsList = await new ProductProductDb(App.Session.odooConnection.DbNameSqlite).GetByProductsIdsLite(product_ids, 0);                

                foreach (var line in orderLines)
                {
                    var product = productsList.FirstOrDefault(p => p.id == line.product_id);
                    if (product != null)
                    {
                        line.product_code = product.code;
                        line.product_display = product.name;
                        line.uom_category_display = product.uom_sale_display;
                        OrderLines.Add(line);
                    }
                    else
                    {
                        //await Toast.Make("Producto huerfano " + line.product_id.ToString()).Show();
                        Debug.WriteLine("Producto huerfano " + line.product_id.ToString());
                    }
                }

                var saleOrderPromotionsDb = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
                var saleOrderPromotions_tmp = await saleOrderPromotionsDb.GetItemsByOrder(CurrentSaleOrder.id);

                foreach (var sop in saleOrderPromotions_tmp)
                {
                    saleOrderPromotions.Add(sop);
                }

                UpdateTotals();
            }
        }

        //private void OnClose()
        //{
            
        //}

        //private void OnSave()
        //{
            
        //}

        //private async void OnSync()
        //{            
        //    await Task.Delay(1000);
        //}

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

            float tax_sale_amount = 15f;

            if (tax_sale != null)
            {
                tax_sale_amount = tax_sale.amount;
            }            

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
            decimal iva_tax = (decimal)tax_sale_amount; //15m;
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

        [Obsolete("Use OnAddLine(ItemPickedArgs itemPickedArgs) instead.")]
        private async void OnAddLine(product_product product)
        {
            if (product is null) return;

            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

            int sequence_line = OrderLines.Count;
            sequence_line++;

            // Buscar si el producto ya existe en la lista
            var existingLine = OrderLines.Where(l => l.product_id == product.id && !l.is_gift).FirstOrDefault();

            if (existingLine != null)
            {
                if ((int)product.cantidad_disponible < (int)(existingLine.product_uom_qty_real + 1))
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Warning", "Producto no se puede agregar porque no hay stock sufuciente.", "OK");
                    return;
                }

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

                if(product.cantidad_disponible == 0)
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Warning", "Producto no se puede agregar porque no hay stock sufuciente.", "OK");
                    return;
                }

                if (priceCalc.ExistsInPriceList)
                {
                    // Si no existe, agregar una nueva línea
                    var line = new sale_order_line
                    {
                        sequence = sequence_line,
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
                    OnPropertyChanged(nameof(OrderLines));
                }
                else
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Warning", "Producto no se puede agregar porque no existe en la lista de precios.", "OK");
                    return;
                }
            }

            await promotionEngineRunner.ResetManualGiftBenefitSoft(CurrentSaleOrder, saleOrderPromotions);
            UpdateTotals();
        }

        private async void OnAddLine(ItemPickedArgs itemPickedArgs)
        {
            if (itemPickedArgs is null) return;

            var product = itemPickedArgs.product;
            var qty_real = itemPickedArgs.qty_real;
            var qty_sol = itemPickedArgs.qty_sol;

            if (qty_sol > qty_real)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Alerta", "La cantidad solicitada no puede ser mayor a la cantidad real.", "Aceptar");
                return;
            }

            if(itemPickedArgs.product.cantidad_disponible < (float) qty_sol)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Alerta", "La cantidad solicitada no puede ser mayor a la disponible en inventario.", "Aceptar");
                return;
            }

            int sequence_line = OrderLines.Count;
            sequence_line++;

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
                var priceCalc = await getPriceWithPricelist(product, CurrentPriceList, qty_sol);

                if (priceCalc.ExistsInPriceList)
                {
                    // Si no existe, agregar una nueva línea
                    var line = new sale_order_line
                    {
                        sequence = sequence_line,
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
                //if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                {
                    //List<PromotionEvalItem> promotionEvalItemParent = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(sale_Order_Line.promotion_data);
                    List<PromotionEvalItem> promotionEvalItemParent = sale_Order_Line.promotionDataList;
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

                //if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                //{
                    //List<PromotionEvalItem> benefitFromData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(sale_Order_Line.promotion_data);
                    List<PromotionEvalItem> benefitFromData = sale_Order_Line.promotionDataList;
                    if (benefitFromData != null && benefitFromData.Count > 0)
                    {
                        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
                        foreach (var benefit in benefitFromData)
                        {
                            await promotionEngineRunner.AddApplyPromotion(CurrentSaleOrder, benefit, -1, saleOrderPromotions);
                        }
                    }
                //}

                sale_Order_Line.promotion_data = "";
                DMSA.Models.Odoo.Promotions.Tools.ClearPromotionData(sale_Order_Line);

                var giftList = OrderLines.Where(x => x.is_gift).ToList();

                if (giftList != null)
                {
                    foreach (var itemGift in giftList)
                    {
                        //if (itemGift.promotion_data != null)
                        //{
                            //List<PromotionEvalItem> promotionEvalItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(itemGift.promotion_data);
                            List<PromotionEvalItem> promotionEvalItem = itemGift.promotionDataList;
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
                        //}
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
                //if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                //{
                    //List<PromotionEvalItem> promotionEvalItemParent = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(sale_Order_Line.promotion_data);
                List<PromotionEvalItem> promotionEvalItemParent = sale_Order_Line.promotionDataList;
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
                //}

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

                //await promotionEngineRunner.ResetManualGiftBenefit(CurrentSaleOrder, saleOrderPromotions);

                //if (!string.IsNullOrEmpty(sale_Order_Line.promotion_data))
                //{
                //List<PromotionEvalItem> benefitFromData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(sale_Order_Line.promotion_data);
                List<PromotionEvalItem> benefitFromData = sale_Order_Line.promotionDataList;
                if (benefitFromData != null && benefitFromData.Count > 0)
                    {                        
                        foreach (var benefit in benefitFromData)
                        {
                            //await promotionEngineRunner.AddApplyPromotion(CurrentSaleOrder, benefit, -1, saleOrderPromotions);
                        }
                    }
                //}
                
                //Se resetea las promociones tipo regalo manual
                // y se buscan otras lineas principales que podrian estar relacionadas a la misma promocion
                //for (var ib = 0; ib < saleOrderPromotions.Count(); ib++)
                //{
                //    var benefitMemory = saleOrderPromotions[ib];

                //    if (benefitMemory.promotion_type_id == 2 && benefitMemory.promotion_selection_type_id == 2)
                //    {
                //        saleOrderPromotions.Remove(benefitMemory);
                //    }
                //}

                //sale_Order_Line.promotion_data = "";

                for (var si = 0; si < saleOrderPromotions.Count; si++)
                {
                    var currentBenefit = saleOrderPromotions[si];
                    var tmplIds = saleOrderPromotions[si].related_product_tmpl_ids;
                    int[] ints = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(tmplIds);

                    if (!ints.Any())
                    {
                        continue;
                    }

                    if (!ints.Contains(sale_Order_Line.product_tmpl_id))
                    {
                        continue;
                    }

                    var giftList = OrderLines.Where(x => x.is_gift).ToList();
                    if (giftList != null && giftList.Count > 0)
                    {
                        //await promotionEngineRunner.ResetManualGiftBenefit(CurrentSaleOrder, saleOrderPromotions);

                        foreach (var itemGift in giftList)
                        {
                            //if (itemGift.promotion_data != null)
                            //{
                                //List<PromotionEvalItem> promotionEvalItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(itemGift.promotion_data);
                            List<PromotionEvalItem> promotionEvalItem = itemGift.promotionDataList;
                            Debug.WriteLine(promotionEvalItem);

                                if (promotionEvalItem != null && promotionEvalItem.Count > 0)
                                {
                                    foreach (var evalItem in promotionEvalItem)
                                    {
                                        if (evalItem.Promotion.id == currentBenefit.promotion_id)
                                        {
                                            if(evalItem.Promotion._promotion_type_id == 2 && evalItem.Promotion._selection_type_id == 2)
                                            {
                                                //await promotionEngineRunner.ResetManualGiftBenefitSoft(CurrentSaleOrder, saleOrderPromotions);
                                                //Si es regalo manual, no se elimina
                                                continue;
                                            }
                                            
                                            OrderLines.Remove(itemGift);
                                        }
                                    }
                                }
                            //}
                        }
                    }

                    //Se resetea las promociones tipo regalo manual
                    // y se buscan otras lineas principales que podrian estar relacionadas a la misma promocion
                    if (currentBenefit.promotion_type_id == 2 && currentBenefit.promotion_selection_type_id == 2)
                    {
                        //saleOrderPromotions.Remove(currentBenefit);
                    }
                }
            }

            UpdateTotals();
        }

        public async Task<bool> ExistsLinkedGifts(sale_order_line sale_Order_Line)
        {
            if (sale_Order_Line != null)
            {
                if (!sale_Order_Line.is_gift)
                {
                    for (var si = 0; si < saleOrderPromotions.Count; si++)
                    {
                        var currentBenefit = saleOrderPromotions[si];
                        var tmplIds = saleOrderPromotions[si].related_product_tmpl_ids;
                        int[] ints = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(tmplIds);

                        if (!ints.Any())
                        {
                            continue;
                        }

                        if (!ints.Contains(sale_Order_Line.product_tmpl_id))
                        {
                            continue;
                        }

                        var giftList = OrderLines.Where(x => x.is_gift).ToList();
                        if (giftList != null && giftList.Count > 0)
                        {
                            foreach (var itemGift in giftList)
                            {
                                //if (itemGift.promotion_data != null)
                                //{
                                    //List<PromotionEvalItem> promotionEvalItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(itemGift.promotion_data);
                                List<PromotionEvalItem> promotionEvalItem = itemGift.promotionDataList;
                                Debug.WriteLine(promotionEvalItem);

                                    if (promotionEvalItem != null && promotionEvalItem.Count > 0)
                                    {
                                        foreach (var evalItem in promotionEvalItem)
                                        {
                                            if (evalItem.Promotion.id == currentBenefit.promotion_id)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                //}
                            }
                        }
                    }
                }
            }

            return false;
        }

        public async void RemoveOrderLine(sale_order_line sale_Order_Line)
        {
            bool requiredConfirm = await ExistsLinkedGifts(sale_Order_Line);
            if (requiredConfirm)
            {
                //await Application.Current.Windows[0].Page.DisplayAlert("Warning", "Producto no se puede eliminar porque tiene regalos relacionados, elimine los regalos para proceder.", "OK");
                var leave = await Application.Current.Windows[0].Page.DisplayAlert("Atención", "Al eliminar el producto se eliminaran los regalos relacionados", "Si", "No");

                if (!leave)
                {
                    return;
                }                
            }

            await RemoveOrderLineProcess(sale_Order_Line);
        }

        public async Task RemoveOrderLineProcess(sale_order_line sale_Order_Line) //, product_product product)
        {
            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

            if (sale_Order_Line != null)
            {
                //Cuando es un producto main (asumiendo que contenga regalos asociados)
                //Se buscan los regalos asociados a la línea seleccionada
                // para eliminarlos también
                if (!sale_Order_Line.is_gift)
                {
                    for (var si = 0; si < saleOrderPromotions.Count; si++)
                    {
                        var currentBenefit = saleOrderPromotions[si];
                        var tmplIds = saleOrderPromotions[si].related_product_tmpl_ids;
                        int[] ints = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(tmplIds);

                        if (!ints.Any())
                        {
                            continue;
                        }

                        if (!ints.Contains(sale_Order_Line.product_tmpl_id))
                        {
                            continue;
                        }

                        var giftList = OrderLines.Where(x => x.is_gift).ToList();
                        if (giftList != null && giftList.Count > 0)
                        {
                            await promotionEngineRunner.ResetManualGiftBenefit(CurrentSaleOrder, saleOrderPromotions);

                            foreach (var itemGift in giftList)
                            {
                                //if (itemGift.promotion_data != null)
                                //{
                                //List<PromotionEvalItem> promotionEvalItem = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(itemGift.promotion_data);
                                List<PromotionEvalItem> promotionEvalItem = itemGift.promotionDataList;
                            Debug.WriteLine(promotionEvalItem);

                                    if (promotionEvalItem != null && promotionEvalItem.Count > 0)
                                    {
                                        foreach (var evalItem in promotionEvalItem)
                                        {
                                            if (evalItem.Promotion.id == currentBenefit.promotion_id)
                                            {
                                                OrderLines.Remove(itemGift);
                                            }
                                        }
                                    }
                                //}
                            }
                        }
                    }
                }
                else
                {
                    //TODO: AQUI REQUIERE REFACTORIZACION, se debe usar origin_gift_line_ids_offline para buscar la data.
                    //Si es que es un regalo
                    //Se busca el producto principal para reducirle la cantidad de regalos asociados
                    if(!string.IsNullOrEmpty(sale_Order_Line.origin_gift_line_ids_offline))
                    {
                        var originIds = JsonConvert.DeserializeObject<List<OriginPromoOrderLine>>(sale_Order_Line.origin_gift_line_ids_offline);
                        //var mainOrderLine = OrderLines.Where(x => x.product_id == sale_Order_Line.product_id_origin).FirstOrDefault();

                        if (originIds.Any())
                        {

                            var mainOrderLine = OrderLines.Where(x => x.product_id == originIds[0].product_id && x.sequence == originIds[0].sequence).FirstOrDefault();

                            if (mainOrderLine != null) // && mainOrderLine.promotion_data != null)
                            {
                                //List<PromotionEvalItem> benefitFromData = JsonConvert.DeserializeObject<List<PromotionEvalItem>>(mainOrderLine.promotion_data);
                                List<PromotionEvalItem> benefitFromData = mainOrderLine.promotionDataList;
                                if (benefitFromData != null && benefitFromData.Count > 0)
                                {
                                    foreach (var benefit in benefitFromData)
                                    {
                                        var dataBenefit = await promotionEngineRunner.GetDataBenefit(CurrentSaleOrder, benefit, saleOrderPromotions);
                                        if (dataBenefit != null)
                                        {
                                            dataBenefit.assigned_gifts = dataBenefit.assigned_gifts - (int)sale_Order_Line.product_uom_qty_real;                                            
                                        }

                                        await promotionEngineRunner.AddApplyPromotion(CurrentSaleOrder, benefit, -1, saleOrderPromotions);
                                    }
                                }
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
            InvalidateTotals();
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Descuento));
            OnPropertyChanged(nameof(Neto));
            OnPropertyChanged(nameof(Impuesto));
            OnPropertyChanged(nameof(CompSolidaria));
            OnPropertyChanged(nameof(Total));
        }

        private void InvalidateTotals()
        {
            _subtotalDirty = true;
            _descuentoDirty = true;
            _impuestoDirty = true;
        }

        private async void OnPegarClicked(object sender, EventArgs e)
        {
            if (Clipboard.HasText)
            {
                var text = await Clipboard.GetTextAsync();
                await ProcesarTextoAsync(text);
            }
        }

        private async Task ProcesarTextoAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var lineas = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

            foreach (var linea in lineas)
            {
                try
                {
                    if (!linea.Contains("["))
                        continue;

                    var partes = linea.Split('\t');

                    if (partes.Length < 10)
                        continue;

                    // Código
                    var codigo = partes[0]
                        .Split(']')[0]
                        .Replace("[", "")
                        .Trim();

                    // Cantidades y precio (strings crudos)
                    var cantidadSolStr = partes[3];
                    var cantidadStr = partes[4];
                    var precioStr = partes[6];

                    // Limpieza
                    precioStr = precioStr.Replace("$", "").Trim();

                    // Parse seguro (cultura invariante para soportar 15,683.00)
                    if (!decimal.TryParse(cantidadSolStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidadSol))
                        continue;

                    if (!decimal.TryParse(cantidadStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidad))
                        continue;

                    if (!decimal.TryParse(precioStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio))
                        continue;

                    Debug.WriteLine($"Código: {codigo}, Cantidad: {cantidad}, Precio: {precio}");

                    var productItem = await productDb.GetItemAsync(p => p.code == codigo);

                    if (productItem == null)
                        continue;

                    productItem.list_price = (float) precio;

                    var itemPickedArgs = new ItemPickedArgs
                    {
                        product = productItem,
                        qty_real = cantidadSol,
                        qty_sol = cantidad,                        
                        // opcional si luego lo necesitas
                        // price = precio
                    };

                    OnAddLine(itemPickedArgs);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error procesando línea: {ex.Message}");
                }
            }
        }

    //void ProcesarTexto(string text)
    //{
    //    var lineas = text.Split('\n');
    //    var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
    //    foreach (var linea in lineas)
    //    {
    //        if (!linea.Contains("["))
    //            continue;

    //        var partes = linea.Split('\t');

    //        if (partes.Length < 10)
    //            continue;

    //        var codigo = partes[0]
    //            .Split(']')[0]
    //            .Replace("[", "");

    //        var cantidad_sol = partes[2];
    //        var cantidad = partes[4];
    //        var precio = partes[9]
    //            .Replace("$", "")
    //            .Trim();

    //        Debug.WriteLine($"Código: {codigo}, Cantidad: {cantidad}, Precio: {precio}");

    //        var productItem = await productDb.GetItemAsync(p => p.code == codigo);

    //        ItemPickedArgs itemPickedArgs = new ItemPickedArgs
    //        {
    //            product = productItem,
    //            qty_real = cantidad_sol,
    //            qty_sol = cantidad,
    //        };

    //        OnAddLine(itemPickedArgs);
    //    }
    //}
}
}
