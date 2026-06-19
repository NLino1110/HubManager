using DMSA.Models.Odoo.Inventory;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public partial class ProductProductDb
    {
        //public async Task<List<product_product>> GetByProductsTemplate(int[] product_template_ids, int filter_pricelist)
        //{
        //    await Init();

        //    await PreloadPricelistCache(filter_pricelist);
        //    await PreloadTaxeslistCache();

        //    var items = await Database.Table<uom_uom>()
        //        .Where(x => x.active == true)
        //        .ToArrayAsync();

        //    cachedUom = items.ToDictionary(uom => uom.id, uom => uom);

        //    var product_template_ids_list = product_template_ids.ToList();

        //    var products = await Database.Table<product_product>().Where(x => product_template_ids_list.Contains(x._product_tmpl_id)).ToListAsync();

        //    if (products == null || products.Count == 0)
        //        return new List<product_product>();

        //    foreach (var p in products)
        //    {
        //        p.uom_sale_display = cachedUom.TryGetValue(p._uom_sale_id, out var uom) ? uom.clave_externa : "";

        //        decimal factor_iva = 1;
        //        if (cachedTaxesList.TryGetValue(p._taxes_id, out var tax_sale))
        //        {
        //            decimal iva_tax = tax_sale; //15m;
        //            factor_iva = 1 + (iva_tax / 100m);
        //        }

        //        if (cachedProductsWithPrices.TryGetValue((p._product_tmpl_id, p._uom_sale_id), out var price))
        //        {
        //            p.list_price = (float)price;
        //        }
        //        else
        //        {
        //            p.list_price = 0;
        //        }

        //        decimal price_list_value = (decimal)p.list_price;
        //        decimal price_without_iva = Math.Round(price_list_value / factor_iva, 7);

        //        p.list_price = (float)price_without_iva;
        //    }

        //    return products;
        //}


        public async Task<List<product_product>> GetByProductsTemplate(int[] product_template_ids, int filter_pricelist)
        {
            if (product_template_ids == null || product_template_ids.Length == 0)
                return new List<product_product>();

            await Init();

            // 🔥 Cache inteligente (NO repetir)
            await PreloadPricelistCache(filter_pricelist);
            await PreloadTaxeslistCache();

            if (cachedUom.Count == 0)
            {
                var items = await Database.Table<uom_uom>()
                    .Where(x => x.active == true)
                    .ToArrayAsync();

                cachedUom = items.ToDictionary(uom => uom.id, uom => uom);
            }

            // 🔥 HashSet = lookup rápido
            var templateIdsSet = product_template_ids.ToHashSet();

            // 🔥 QUERY DIRECTA (sin ToList intermedio)
            var products = await Database.Table<product_product>()
                .Where(x => templateIdsSet.Contains(x._product_tmpl_id))
                .ToListAsync();

            if (products.Count == 0)
                return products;

            // 🔥 CACHE LOCAL (evita lookups repetidos)
            var localTaxes = cachedTaxesList;
            var localPrices = cachedProductsWithPrices;
            var localUom = cachedUom;

            foreach (var p in products)
            {
                // 🔥 UOM
                if (localUom.TryGetValue(p._uom_sale_id, out var uom))
                    p.uom_sale_display = uom.clave_externa;
                else
                    p.uom_sale_display = string.Empty;

                // 🔥 FACTOR IVA (O(1))
                decimal factor_iva = 1m;
                if (localTaxes.TryGetValue(p._taxes_id, out var tax))
                    factor_iva += tax / 100m;

                // 🔥 PRECIO BASE
                if (!localPrices.TryGetValue((p._product_tmpl_id, p._uom_sale_id), out var price))
                    price = 0m;

                // 🔥 CALCULO DIRECTO (sin doble asignación)
                if (factor_iva != 1m && price != 0m)
                {
                    // 🔥 evitar Math.Round si no es necesario
                    p.list_price = (float)(price / factor_iva);
                }
                else
                {
                    p.list_price = (float)price;
                }
            }

            return products;
        }

        public async Task<List<product_product>> GetByProductsIds(int[] product_ids, int filter_pricelist)
        {
            await Init();

            await PreloadPricelistCache(filter_pricelist);
            await PreloadTaxeslistCache();

            var items = await Database.Table<uom_uom>()
                .Where(x => x.active == true)
                .ToArrayAsync();

            cachedUom = items.ToDictionary(uom => uom.id, uom => uom);

            var ids = product_ids.ToList();
            var products = await Database.Table<product_product>().Where(x => ids.Contains(x.id)).ToListAsync();

            if (products == null || products.Count == 0)
                return new List<product_product>();

            foreach (var p in products)
            {
                p.uom_sale_display = cachedUom.TryGetValue(p._uom_sale_id, out var uom) ? uom.clave_externa : "";

                decimal factor_iva = 1;
                if (cachedTaxesList.TryGetValue(p._taxes_id, out var tax_sale))
                {
                    decimal iva_tax = tax_sale; //15m;
                    factor_iva = 1 + (iva_tax / 100m);
                }

                if (cachedProductsWithPrices.TryGetValue((p._product_tmpl_id, p._uom_sale_id), out var price))
                {
                    p.list_price = (float)price;
                }
                else
                {
                    p.list_price = 0;
                }

                decimal price_list_value = (decimal)p.list_price;
                decimal price_without_iva = Math.Round(price_list_value / factor_iva, 7);

                p.list_price = (float)price_without_iva;
            }

            return products;
        }
    }
}
