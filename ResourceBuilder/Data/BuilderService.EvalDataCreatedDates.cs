using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Models.DMSA.Shared.Tools;

namespace ResourceBuilder.Data
{
    public partial class BuilderService
    {
        async Task<DateTime[]> EvalDataCreatedDates(string current_model, int year, int month, int day, int indice)
        {
            List<DateTime> datesResult = new List<DateTime>();

            var appSetting = ConfigurationHelper.GetAppSettings();

            AppSession _appSession = GetAppSession();

            //AppSession _appSession = new AppSession();
            //_appSession.EndPointServer = appSetting.profile.Odoo.ApiBaseAddressOdoo;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };

            string fechaActualizaTablet = "2021-01-01 00:00:00";

            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = int.Parse(appSetting.profile.Odoo.uid);
            apiRequest.password = appSetting.profile.Odoo.Password;
            apiRequest.databasename = appSetting.profile.Odoo.Database;
            apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

            apiRequest.index = indice;
            apiRequest.update = false;//forUpdate;
            apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

            switch (current_model)
            {
                case "account_move":
                    {
                        ApiManager.HubAccountMove hubmanager = new ApiManager.HubAccountMove(_appSession);
                        ApiResponseOdooRpcT<account_move[]>? responseAll = null;
                        responseAll = await hubmanager.GetByCreateDate_dl(year, month, day, default_limit, indice);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.create_date.Value.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }

                        return datesResult.ToArray();
                    }
                case "account_move_line":
                    {
                        ApiManager.HubAccountMoveLine hubmanager = new ApiManager.HubAccountMoveLine(_appSession);
                        var responseAll = await hubmanager.GetAccountMoveLineByCreateDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.create_date.Value.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
                case "product_template":
                    {
                        ApiManager.HubProductTemplate hubmanager = new ApiManager.HubProductTemplate(_appSession);
                        var responseAll = await hubmanager.GetByCreateDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.create_date.Value.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
                case "product_product":
                    {
                        var responseAll = await hubProductProduct.GetByCreateDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.create_date.Value.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
                case "res_partner":
                    {
                        ApiManager.HubResPartner hubPartner = new ApiManager.HubResPartner(_appSession);
                        
                        //setHubPartner(_appSession);
                        //apiRequest.limit = 0;
                        //apiRequest.index = 0;
                        var responseAll = await hubPartner.GetByCreateDate_dl(year, month, day);

                        //if (responseAll != null && responseAll?.data != null && responseAll.data.Length > 0)
                        //{
                        //    datesResult = responseAll.data
                        //        .Select(m => m.create_date.Date)
                        //        .Distinct() // Elimina duplicados
                        //        .ToList();
                        //}

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.create_date.Value.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
                case "account_journal":
                    {
                        ApiManager.HubAccountJournal hubmanager = new ApiManager.HubAccountJournal(_appSession);
                        var responseAll = await hubmanager.GetByCreateDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.create_date.Value.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
            }
            return datesResult.ToArray();
        }
    }
}
