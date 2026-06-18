using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;

namespace DMOrders.Pages.Fragments.Orders
{
    public partial class Crud
    {
        private bool IsSaving = false;

        private async Task<sale_order> new_SaveOrder()
        {
            if (IsSaving) return null; // evita doble guardado
            IsSaving = true;

            try
            {
                var orderLines = OrderLines.ToList(); // snapshot seguro
                var orderPromotions = saleOrderPromotions;

                var saleOrderDb = new SaleOrderDb(App.Session.odooConnection.DbNameSqlite);
                var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

                sale_order targetOrder;

                bool isNew = CurrentSaleOrder == null;

                int warehouseId = 0;
                int partner_invoice_id = ((res_partner)ddfAddress.SelectedItem).id;

                StockWareHouseDb stockWareHouseDb = new StockWareHouseDb(App.Session.odooConnection.DbNameSqlite);
                var warehouseList = await stockWareHouseDb.GetDefaultByResCenter(App.Session.res_center.id);
                if (warehouseList != null && warehouseList.Count > 0)
                {
                    warehouseId = warehouseList[0].id;
                }

                if (isNew)
                {
                    string new_id_referencia = GenerarCodigo(CurrentPartner.name, await saleOrderDb.GetNextSecuentialId());

                    targetOrder = new sale_order
                    {
                        _partner_id = _CurrentPartner.id,
                        _company_id = CurrentCompany.id,
                        date_order = DateTime.Now,
                        mobile_create_date = DateTime.Now,
                        _center_id = App.Session.res_center.id,
                        _warehouse_id = warehouseId,
                        sale_channel = App.Session.odooConnection.sale_channel_default,
                        id_referencia = new_id_referencia,
                        _pricelist_id = CurrentPriceList.id,
                        amount_total = Total,
                        amount_tax = Impuesto,
                        amount_untaxed = Subtotal,
                        state = "draft",
                        partner_display_name = CurrentPartner?.name,
                        partner_display_address = CurrentPartner?.street,
                        partner_display_status = (CurrentPartner != null ? (CurrentPartner.active ? "Activo" : "Inactivo") : string.Empty),
                        partner_sale_id = App.Session.CurrentUserFront.partner_id,
                        _partner_invoice_id = partner_invoice_id,
                        _partner_shipping_id = partner_invoice_id,
                        note2 = Note2,
                        external_create_uid = App.Session.CurrentUserFront.uid,
                        external_guid = Guid.NewGuid().ToString("N"),
                        mobile_sync = true
                    };

                    if (await saleOrderDb.InsertAsync(targetOrder) <= 0)
                    {
                        await Toast.Make("Error al crear la orden").Show();
                        return null;
                    }

                    CurrentSaleOrder = targetOrder;
                }
                else
                {
                    targetOrder = CurrentSaleOrder;

                    targetOrder.write_date = DateTime.Now;
                    targetOrder._center_id = App.Session.res_center.id;
                    targetOrder._warehouse_id = warehouseId;
                    targetOrder.sale_channel = App.Session.odooConnection.sale_channel_default;
                    targetOrder._partner_invoice_id = partner_invoice_id;
                    targetOrder._partner_shipping_id = partner_invoice_id;
                    targetOrder._pricelist_id = CurrentPriceList.id;
                    targetOrder.amount_total = Total;
                    targetOrder.amount_tax = Impuesto;
                    targetOrder.amount_untaxed = Subtotal;
                    targetOrder.note2 = Note2;

                    targetOrder.promotion_ids = orderPromotions?
                        .Where(x => x != null)
                        .Select(x => x.promotion_id)
                        .Distinct()
                        .ToArray()
                        ?? Array.Empty<int>();

                    if (await saleOrderDb.UpdateAsync(targetOrder) <= 0)
                    {
                        await Toast.Make("Error al actualizar la orden").Show();
                        return null;
                    }

                    await ResetPromotions(targetOrder);
                }

                int ordinal = 1;
                targetOrder.order_line = new List<OrderLineWrapper>();

                // ELIMINAR SOLO LO QUE YA NO EXISTE
                var dbLines = await saleOrderLineDb.GetItemsAsync(targetOrder.id);

                var currentIds = orderLines
                    .Where(x => x.id != 0)
                    .Select(x => x.id)
                    .ToHashSet();

                foreach (var dbLine in dbLines)
                {
                    if (!currentIds.Contains(dbLine.id))
                    {
                        await saleOrderLineDb.DeleteAsync(dbLine);
                    }
                }

                // INSERT / UPDATE SEGURO
                foreach (var orderLine in orderLines)
                {
                    orderLine._order_id = targetOrder.id;
                    orderLine.sequence = ordinal;

                    if (orderLine.id == 0)
                        await saleOrderLineDb.InsertAsync(orderLine);
                    else
                        await saleOrderLineDb.UpdateAsync(orderLine);

                    targetOrder.order_line.Add(new OrderLineWrapper(orderLine));
                    ordinal++;
                }

                await SavePromotions(true, false);

                try
                {
                    var mainPage = (MainPageTab)App.Current.MainPage;
                    mainPage.SelectTab("orders");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al cambiar pestaña: {ex.Message}");
                }

                await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();
                return targetOrder;
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task<sale_order> SaveOrder()
        {
            var orderLines = OrderLines;
            var orderPromotions = saleOrderPromotions;
            var saleOrderDb = new SaleOrderDb(App.Session.odooConnection.DbNameSqlite);
            var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

            sale_order targetOrder;

            bool isNew = CurrentSaleOrder == null;

            int warehouseId = 0;
            int partner_invoice_id = ((res_partner)ddfAddress.SelectedItem).id;

            StockWareHouseDb stockWareHouseDb = new StockWareHouseDb(App.Session.odooConnection.DbNameSqlite);
            var warehouseList = await stockWareHouseDb.GetDefaultByResCenter(App.Session.res_center.id);
            if (warehouseList != null && warehouseList.Count > 0)
            {
                warehouseId = warehouseList[0].id;
            }

            if (isNew)
            {
                string new_id_referencia = GenerarCodigo(CurrentPartner.name, await saleOrderDb.GetNextSecuentialId());

                targetOrder = new sale_order
                {
                    _partner_id = _CurrentPartner.id,
                    _company_id = CurrentCompany.id,
                    date_order = DateTime.Now,
                    mobile_create_date = DateTime.Now,
                    _center_id = App.Session.res_center.id,
                    _warehouse_id = warehouseId,
                    sale_channel = App.Session.odooConnection.sale_channel_default,
                    id_referencia = new_id_referencia,
                    _pricelist_id = CurrentPriceList.id,
                    amount_total = Total,
                    amount_tax = Impuesto,
                    amount_untaxed = Subtotal,
                    state = "draft",
                    partner_display_name = CurrentPartner?.name,
                    partner_display_address = CurrentPartner?.street,
                    partner_display_status = (CurrentPartner != null ? (CurrentPartner.active ? "Activo" : "Inactivo") : string.Empty),
                    partner_sale_id = App.Session.CurrentUserFront.partner_id,
                    _partner_invoice_id = partner_invoice_id,
                    _partner_shipping_id = partner_invoice_id,
                    note2 = Note2,
                    external_create_uid = App.Session.CurrentUserFront.uid,
                    external_guid = Guid.NewGuid().ToString("N"),
                    mobile_sync = true
                };

                if (await saleOrderDb.InsertAsync(targetOrder) <= 0)
                {
                    await Toast.Make("Error al crear la orden").Show();
                    return null;
                }

                CurrentSaleOrder = targetOrder;
                //CurrentSaleOrder = targetOrder;
            }
            else
            {
                targetOrder = CurrentSaleOrder;

                targetOrder.write_date = DateTime.Now;
                targetOrder._center_id = App.Session.res_center.id;
                targetOrder._warehouse_id = warehouseId;
                targetOrder.sale_channel = App.Session.odooConnection.sale_channel_default;
                targetOrder._partner_invoice_id = partner_invoice_id;
                targetOrder._partner_shipping_id = partner_invoice_id;
                targetOrder._pricelist_id = CurrentPriceList.id;
                targetOrder.amount_total = Total;
                targetOrder.amount_tax = Impuesto;
                targetOrder.amount_untaxed = Subtotal;
                targetOrder.note2 = Note2;

                targetOrder.promotion_ids = orderPromotions?
                    .Where(x => x != null)
                    .Select(x => x.promotion_id)
                    .Distinct()
                    .ToArray()
                    ?? Array.Empty<int>();

                if (await saleOrderDb.UpdateAsync(targetOrder) <= 0)
                {
                    await Toast.Make("Error al actualizar la orden").Show();
                    return null;
                }

                // Eliminar líneas anteriores antes de insertar las nuevas
                await saleOrderLineDb.DeleteItemOfParent(targetOrder);
                //await saleOrderPromoDb.DeleteItemOfParent(targetOrder);
                await ResetPromotions(targetOrder);
            }

            int ordinal = 1;

            targetOrder.order_line = new List<OrderLineWrapper>();

            // Asignar el ID de la orden a las líneas y guardar
            foreach (var orderLine in orderLines)
            {
                orderLine._order_id = targetOrder.id;
                orderLine.sequence = ordinal;
                await saleOrderLineDb.InsertAsync(orderLine);

                //Datos referenciales
                targetOrder.order_line.Add(new OrderLineWrapper(orderLine));

                ordinal++;
            }

            await SavePromotions(true, false);

            try
            {
                var mainPage = (MainPageTab)App.Current.MainPage;
                mainPage.SelectTab("orders");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cambiar a la pestaña Pedidos: {ex.Message}");
            }

            await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();
            return targetOrder;
        }
    }
}
