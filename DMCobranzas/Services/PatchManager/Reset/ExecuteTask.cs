using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Accounting;
using DMSA.Sync.Core.Database.Sqlite.Payments;

namespace DMCobranzas.Services.PatchManager.Reset
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

        public async Task ResetAccountMoves()
        {
            var accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
            await accountMoveDb.DropTableAsync();
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

        public async Task ResetProductsImages()
        {
            var productsDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            await productsDb.RemoveImagesContent();
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

        public async Task ResetResCenterLine()
        {
            var resCenterLineDb = new ResCenterLineDb(App.Session.odooConnection.DbNameSqlite);
            await resCenterLineDb.DropTableAsync();
        }

        public async Task Vaccum()
        {
            var productsDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            await productsDb.Vaccum();
        }
    }
}
