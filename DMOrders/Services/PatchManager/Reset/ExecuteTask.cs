using DMSA.Sync.Core;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using System.Diagnostics;

namespace DMOrders.Services.PatchManager.Reset
{
    public class ExecuteTask
    {
        public async Task ResetCompanies()
        {
            var companiesDb = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
            await companiesDb.DeleteAllAsync(x => x.id > 0);
        }

        public async Task ResetUsers()
        {
            var userAccessDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            await userAccessDb.DeleteAllAsync(x => x.uid > 0);
        }

        public async Task ResetAccountMoveLines()
        {
            var accountMoveLineDb = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
            await accountMoveLineDb.DropTableAsync();
        }

        public async Task ResetCreditNoteLine()
        {
            var creditNoteLineDb = new CreditNoteRequestDetailDb(App.Session.odooConnection.DbNameSqlite);
            await creditNoteLineDb.DropTableAsync();
        }

        public async Task ResetProducts()
        {
            var productsDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            await productsDb.DropTableAsync();
        }

        public async Task ResetProductLinea()
        {
            var productsLineaDb = new ProductLineaDb(App.Session.odooConnection.DbNameSqlite);
            await productsLineaDb.DropTableAsync();
        }

        public async Task ResetProductMarca()
        {
            var productsMarcaDb = new ProductMarcaDb(App.Session.odooConnection.DbNameSqlite);
            await productsMarcaDb.DropTableAsync();
        }

        public async Task ResetProductSubCategoria()
        {
            var productsSubCategoriaDb = new ProductSubcategoriaDb(App.Session.odooConnection.DbNameSqlite);
            await productsSubCategoriaDb.DropTableAsync();
        }

        public async Task ResetPriceListItem()
        {
            var priceListItemDb = new ProductPricelistItemDb(App.Session.odooConnection.DbNameSqlite);
            await priceListItemDb.DropTableAsync();
        }

        public async Task ResetStockWareHouse()
        {
            var stockWarehouseDb = new StockWareHouseDb(App.Session.odooConnection.DbNameSqlite);
            await stockWarehouseDb.DropTableAsync();
        }

        public async Task ResetResPartners()
        {
            var patchableDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
            await patchableDb.DropTableAsync();
        }

        public async Task FixDuplicatesPriceList()
        {
            var patchableDb = new ProductPricelistItemDb(App.Session.odooConnection.DbNameSqlite);
            await patchableDb.FixDuplicates();
        }

        public async Task Vaccum()
        {
            var productsDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            await productsDb.Vaccum();

            var productsPreviewDb = new ProductProductPreviewDb(App.Session.odooConnection.DbNameSqliteStatic);
            await productsPreviewDb.Vaccum();
        }

        /// <summary>
        /// Vacía tablas de sync de catálogo (promos, marcas/categorías, productos,
        /// precios, stock e imágenes preview) para poder sincronizar de cero.
        /// No toca pedidos ni clientes.
        /// </summary>
        public async Task ClearCatalogSyncDataAsync()
        {
            string db = App.Session.odooConnection.DbNameSqlite;
            string dbStatic = App.Session.odooConnection.DbNameSqliteStatic;

            async Task SafeTruncate(Func<Task> action, string name)
            {
                try
                {
                    await action();
                    Debug.WriteLine($"ClearCatalog: OK {name}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ClearCatalog: fallo {name}: {ex.Message}");
                }
            }

            // Promociones (sync chkGroup1)
            await SafeTruncate(() => new PromotionProductDetailDb(db).Truncate(), "PromotionProductDetail");
            await SafeTruncate(() => new PromotionProductDb(db).Truncate(), "PromotionProduct");
            await SafeTruncate(() => new PromoRulesDb(db).Truncate(), "PromoRules");
            await SafeTruncate(() => new PromoCentersDb(db).Truncate(), "PromoCenters");
            await SafeTruncate(() => new LoyaltyFiltersDetailsDb(db).Truncate(), "LoyaltyFiltersDetails");
            await SafeTruncate(() => new LoyaltyFiltersDb(db).Truncate(), "LoyaltyFilters");
            await SafeTruncate(() => new PosPaymentMethodDb(db).Truncate(), "PosPaymentMethod");
            await SafeTruncate(() => new PosTarjetasCanalDb(db).Truncate(), "PosTarjetasCanal");
            await SafeTruncate(() => new PromotionBenefitDb(db).Truncate(), "PromotionBenefit");
            await SafeTruncate(() => new SaleOrderPromotionsDb(db).Truncate(), "SaleOrderPromotions");

            // Marcas / categorías / maestros (chkGroup2)
            await SafeTruncate(() => new ProductMarcaDb(db).Truncate(), "ProductMarca");
            await SafeTruncate(() => new ProductCategoriaDb(db).Truncate(), "ProductCategoria");
            await SafeTruncate(() => new ProductSubcategoriaDb(db).Truncate(), "ProductSubcategoria");
            await SafeTruncate(() => new ProductLineaDb(db).Truncate(), "ProductLinea");
            await SafeTruncate(() => new ProductGrupoTipoDb(db).Truncate(), "ProductGrupoTipo");

            // Productos + UoM (chkGroup7)
            await SafeTruncate(() => new ProductProductDb(db).Truncate(), "ProductProduct");
            await SafeTruncate(() => new ProductTemplateDb(db).Truncate(), "ProductTemplate");
            await SafeTruncate(() => new UomUomDb(db).Truncate(), "UomUom");

            // Imágenes (BD estática)
            //await SafeTruncate(() => new ProductProductPreviewDb(dbStatic).Truncate(), "ProductProductPreview");

            // Precios / impuestos (chkGroup4)
            await SafeTruncate(() => new ProductPricelistItemDb(db).Truncate(), "ProductPricelistItem");
            await SafeTruncate(() => new ProductPricelistDb(db).Truncate(), "ProductPricelist");
            await SafeTruncate(() => new AccountTaxDb(db).Truncate(), "AccountTax");

            // Stock (chkGroup5)
            await SafeTruncate(() => new WmsStockQuantDb(db).Truncate(), "WmsStockQuant");
            await SafeTruncate(() => new StockQuantDb(db).Truncate(), "StockQuant");
            await SafeTruncate(() => new StockLocationDb(db).Truncate(), "StockLocation");
            await SafeTruncate(() => new StockWareHouseDb(db).Truncate(), "StockWarehouse");
        }

        /// <summary>
        /// Vacía la tabla local de personas/clientes (res_partner) y cuentas bancarias
        /// asociadas, para poder sincronizar de cero. No toca pedidos ni catálogo.
        /// </summary>
        public async Task ClearPartnersSyncDataAsync()
        {
            string db = App.Session.odooConnection.DbNameSqlite;

            async Task SafeTruncate(Func<Task> action, string name)
            {
                try
                {
                    await action();
                    Debug.WriteLine($"ClearPartners: OK {name}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ClearPartners: fallo {name}: {ex.Message}");
                }
            }

            await SafeTruncate(() => new ResPartnerDb(db).Truncate(), "ResPartner");
            ///await SafeTruncate(() => new PartnerBankDb(db).Truncate(), "PartnerBank");
        }
    }
}
