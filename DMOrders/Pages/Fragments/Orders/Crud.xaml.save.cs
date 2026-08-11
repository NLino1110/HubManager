using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using Newtonsoft.Json;

namespace DMOrders.Pages.Fragments.Orders
{
    /// <summary>
    /// Crud.xaml.save.cs — PERSISTENCIA DEL PEDIDO EN SQLITE LOCAL
    /// ---------------------------------------------------------------------
    /// Guarda/actualiza cabecera (sale_order) y líneas (sale_order_line).
    ///
    /// Sequence / promociones (FIX):
    /// Odoo SaleOrderExtend relaciona regalos con (product_id, sequence) vía
    /// origin_gift_line_ids_offline. Por eso el sequence de líneas producto
    /// NO se renumera a 1..n al guardar (eso rompía el match al borrar/reaplicar
    /// regalos). AssignStableSequences conserva sequences de productos y solo
    /// asigna libres a líneas nuevas / gifts; remapea el JSON si hace falta.
    /// </summary>
    public partial class Crud
    {
        private bool IsSaving = false;

        /// <summary>
        /// Conserva sequence de productos (!is_gift con sequence &gt; 0).
        /// Asigna sequences libres a productos nuevos (seq 0) y a regalos.
        /// Si algún producto cambia de sequence, remapea origin_gift_line_ids_offline.
        /// </summary>
        private void AssignStableSequences(IList<sale_order_line> orderLines)
        {
            if (orderLines == null || orderLines.Count == 0)
                return;

            var productLines = orderLines.Where(l => l != null && !l.is_gift).ToList();
            var giftLines = orderLines.Where(l => l != null && l.is_gift).ToList();

            var used = new HashSet<int>();
            var remaps = new Dictionary<(int productId, int oldSeq), int>();

            // 1) Productos con sequence ya asignado: conservar (resolver colisiones)
            foreach (var line in productLines.Where(l => l.sequence > 0).OrderBy(l => l.sequence))
            {
                int oldSeq = line.sequence;
                if (used.Contains(line.sequence))
                {
                    int next = 1;
                    while (used.Contains(next)) next++;
                    line.sequence = next;
                }

                used.Add(line.sequence);

                if (oldSeq != line.sequence)
                    remaps[(line.product_id, oldSeq)] = line.sequence;
            }

            // 2) Productos nuevos (sequence 0): siguiente libre
            foreach (var line in productLines.Where(l => l.sequence <= 0))
            {
                int oldSeq = line.sequence;
                int next = 1;
                while (used.Contains(next)) next++;
                line.sequence = next;
                used.Add(next);

                if (oldSeq > 0 && oldSeq != line.sequence)
                    remaps[(line.product_id, oldSeq)] = line.sequence;
            }

            // 3) Regalos: sequences libres (solo presentación local; Odoo usa el JSON)
            foreach (var gift in giftLines)
            {
                if (gift.sequence > 0 && !used.Contains(gift.sequence))
                {
                    used.Add(gift.sequence);
                    continue;
                }

                int next = 1;
                while (used.Contains(next)) next++;
                gift.sequence = next;
                used.Add(next);
            }

            if (remaps.Count > 0)
                RemapOriginGiftOfflineSequences(giftLines, remaps);

            // Alinea JSON al sequence actual del padre (product_id + sequence viejo → actual)
            AlignGiftOriginsToCurrentParents(orderLines);
        }

        /// <summary>
        /// Actualiza sequence dentro de origin_gift_line_ids_offline cuando un padre cambió de N°.
        /// </summary>
        private static void RemapOriginGiftOfflineSequences(
            IEnumerable<sale_order_line> giftLines,
            Dictionary<(int productId, int oldSeq), int> remaps)
        {
            if (giftLines == null || remaps == null || remaps.Count == 0)
                return;

            foreach (var gift in giftLines)
            {
                if (string.IsNullOrWhiteSpace(gift.origin_gift_line_ids_offline))
                    continue;

                try
                {
                    var origins = JsonConvert.DeserializeObject<List<OriginPromoOrderLine>>(
                        gift.origin_gift_line_ids_offline);

                    if (origins == null || origins.Count == 0)
                        continue;

                    bool changed = false;
                    foreach (var origin in origins)
                    {
                        if (remaps.TryGetValue((origin.product_id, origin.sequence), out int newSeq)
                            && origin.sequence != newSeq)
                        {
                            origin.sequence = newSeq;
                            changed = true;
                        }
                    }

                    if (changed)
                        gift.origin_gift_line_ids_offline = JsonConvert.SerializeObject(origins);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RemapOriginGiftOfflineSequences: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Si el JSON del regalo apunta a un sequence que ya no existe en el padre,
        /// lo corrige al sequence actual de esa línea producto (misma product_id).
        /// Escala a muchos detalles: solo toca gifts desfasados.
        /// </summary>
        private static void AlignGiftOriginsToCurrentParents(IList<sale_order_line> orderLines)
        {
            if (orderLines == null || orderLines.Count == 0)
                return;

            var parentsByProduct = orderLines
                .Where(l => l != null && !l.is_gift)
                .GroupBy(l => l.product_id)
                .ToDictionary(g => g.Key, g => g.ToList());

            var parentKeys = new HashSet<(int productId, int sequence)>(
                orderLines
                    .Where(l => l != null && !l.is_gift)
                    .Select(l => (l.product_id, l.sequence)));

            foreach (var gift in orderLines.Where(l => l != null && l.is_gift))
            {
                if (string.IsNullOrWhiteSpace(gift.origin_gift_line_ids_offline))
                    continue;

                try
                {
                    var origins = JsonConvert.DeserializeObject<List<OriginPromoOrderLine>>(
                        gift.origin_gift_line_ids_offline);

                    if (origins == null || origins.Count == 0)
                        continue;

                    bool changed = false;
                    foreach (var origin in origins)
                    {
                        if (parentKeys.Contains((origin.product_id, origin.sequence)))
                            continue;

                        if (!parentsByProduct.TryGetValue(origin.product_id, out var parents)
                            || parents.Count == 0)
                            continue;

                        // Una sola línea de ese producto → sequence actual seguro
                        if (parents.Count == 1)
                        {
                            if (origin.sequence != parents[0].sequence)
                            {
                                origin.sequence = parents[0].sequence;
                                changed = true;
                            }
                            continue;
                        }

                        // Varias líneas del mismo SKU: preferir la que coincida por sequence;
                        // si no, la de sequence más cercano (no inventar match ambiguo fuerte).
                        var exact = parents.FirstOrDefault(p => p.sequence == origin.sequence);
                        if (exact != null)
                            continue;

                        var closest = parents
                            .OrderBy(p => Math.Abs(p.sequence - origin.sequence))
                            .First();
                        if (origin.sequence != closest.sequence)
                        {
                            origin.sequence = closest.sequence;
                            changed = true;
                        }
                    }

                    if (changed)
                        gift.origin_gift_line_ids_offline = JsonConvert.SerializeObject(origins);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"AlignGiftOriginsToCurrentParents: {ex.Message}");
                }
            }
        }

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

                AssignStableSequences(orderLines);

                foreach (var orderLine in orderLines)
                {
                    orderLine._order_id = targetOrder.id;

                    if (orderLine.id == 0)
                        await saleOrderLineDb.InsertAsync(orderLine);
                    else
                        await saleOrderLineDb.UpdateAsync(orderLine);

                    targetOrder.order_line.Add(new OrderLineWrapper(orderLine));
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

        /// <summary>
        /// Guarda el pedido local (cabecera + líneas). Usado por la UI al confirmar.
        /// En edición: borra líneas hijas y las vuelve a insertar con sequences estables.
        /// </summary>
        private async Task<sale_order> SaveOrder()
        {
            var orderLines = OrderLines;
            var orderPromotions = saleOrderPromotions;
            var saleOrderDb = new SaleOrderDb(App.Session.odooConnection.DbNameSqlite);
            var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

            sale_order targetOrder;

            bool isNew = CurrentSaleOrder == null;

            int warehouseId = 0;
            // ANTES: ((res_partner)ddfAddress.SelectedItem).id → NRE si SelectedItem era null.
            // DESPUÉS: pattern-match; aborta el guardado con mensaje claro.
            if (ddfAddress?.SelectedItem is not res_partner invoicePartner)
            {
                await Toast.Make("Seleccione una dirección de facturación.").Show();
                return null;
            }

            int partner_invoice_id = invoicePartner.id;

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

                // Eliminar líneas anteriores antes de insertar las nuevas
                await saleOrderLineDb.DeleteItemOfParent(targetOrder);
                await ResetPromotions(targetOrder);
            }

            targetOrder.order_line = new List<OrderLineWrapper>();

            AssignStableSequences(orderLines);

            foreach (var orderLine in orderLines)
            {
                orderLine._order_id = targetOrder.id;
                await saleOrderLineDb.InsertAsync(orderLine);

                targetOrder.order_line.Add(new OrderLineWrapper(orderLine));
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
