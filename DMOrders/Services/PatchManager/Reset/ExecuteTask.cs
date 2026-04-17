using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task Vaccum()
        {
            var productsDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            await productsDb.Vaccum();

            var productsPreviewDb = new ProductProductPreviewDb(App.Session.odooConnection.DbNameSqliteStatic);
            await productsPreviewDb.Vaccum();
        }
    }
}
