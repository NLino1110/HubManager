using CommunityToolkit.Maui.Alerts;
using DMCobranzas.Services.PatchManager.Reset;
using DMOrders.Controls.Tools;
using DMSA.Models.Odoo.Abstract;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMCobranzas.Services.PatchManager
{
    public class PatchRunner
    {
        public async Task PatchExecuter(OdooConnection ConnectionItem, ContentPage page)
        {
            Preferences.Set("patch_require_update", false);

            //Preferences.Set("_patch_v0_" + ConnectionItem.DbName, false);
            //Preferences.Set("_patch_v1_" + ConnectionItem.DbName, false);
            //Preferences.Set("_patch_v2_" + ConnectionItem.DbName, false);
            //Preferences.Set("_patch_v3_" + ConnectionItem.DbName, false);
            //Preferences.Set("_patch_v4_" + ConnectionItem.DbName, false);
            //Preferences.Set("_patch_v5_" + ConnectionItem.DbName, false);
            //Preferences.Set("_patch_v6_" + ConnectionItem.DbName, false);

            //await UITools.ShowLoadingPopup(page);
            //await UITools.SetNotifyLoadingPopup("Optimizando base...");
            //await PatchExecuter_Vacuum(ConnectionItem);
            //await UITools.SetNotifyLoadingPopup("Aplicando parches...");
            //await PatchExecuter_v0(ConnectionItem);
            //await PatchExecuter_v1(ConnectionItem);
            await PatchExecuter_v2(ConnectionItem);
            //await PatchExecuter_v3(ConnectionItem);
            //await PatchExecuter_v4(ConnectionItem);
            //await PatchExecuter_v5(ConnectionItem);
            //await PatchExecuter_v6(ConnectionItem);
            //+ parches
            //await UITools.HideLoadingPopup();
        }

        private void ResetFullPatches()
        {
            Preferences.Remove("patch_require_update");
        }
        private async Task PatchExecuter_Vacuum(OdooConnection ConnectionItem)
        {
            var currentVersion = AppInfo.Current.Version;
            var patchVersion = new Version(1, 0, 0, 9);

            ExecuteTask executeTask = new ExecuteTask();
            await executeTask.Vaccum();

            Preferences.Set("patch_require_update", false);
        }

        private async Task PatchExecuter_v0(OdooConnection ConnectionItem)
        {
            var currentVersion = AppInfo.Current.Version;
            var patchVersion = new Version(1, 0, 0, 9);

            string patch_name = "_patch_v0_" + ConnectionItem.DbName;

            bool patch_applied = Preferences.Get(patch_name, false);

            if (!patch_applied)
            {
                ExecuteTask executeTask = new ExecuteTask();

                await executeTask.ResetCompanies();
                await executeTask.ResetUsers();

                OdooConnectionDb _database = new OdooConnectionDb();
                await _database.DeleteAllAsync(x => x.Id > 0);
                await _database.InitDefault();

                Preferences.Set(patch_name, true);
                Preferences.Set("patch_require_update", false);
                Debug.WriteLine(patch_name + " ==== aplicado");
                await Toast.Make("Patch applied: " + patch_name).Show();
            }
        }

        private async Task PatchExecuter_v1(OdooConnection ConnectionItem)
        {
            string patch_name = "_patch_v1_" + ConnectionItem.DbName;

            bool patch_applied = Preferences.Get(patch_name, false);

            if (!patch_applied)
            {
                ExecuteTask executeTask = new ExecuteTask();
                await executeTask.ResetProducts();
                await executeTask.ResetProductMarca();
                await executeTask.ResetProductLinea();
                await executeTask.ResetProductSubCategoria();

                Preferences.Set(patch_name, true);
                Preferences.Set("patch_require_update", true);
                Debug.WriteLine(patch_name + " ==== aplicado");
                await Toast.Make("Patch applied: " + patch_name).Show();
            }
        }

        private async Task PatchExecuter_v2(OdooConnection ConnectionItem)
        {
            string patch_name = "_patch_v2_" + ConnectionItem.DbName;

            bool patch_applied = Preferences.Get(patch_name, false);

            if (!patch_applied)
            {
                ExecuteTask executeTask = new ExecuteTask();
                await executeTask.ResetAccountMoves();
                await executeTask.ResetAccountMoveLines();

                Preferences.Set(patch_name, true);
                Preferences.Set("patch_require_update", true);
                Debug.WriteLine(patch_name + " ==== aplicado");
                await Toast.Make("Patch applied: " + patch_name).Show();
            }
        }
    }
}
