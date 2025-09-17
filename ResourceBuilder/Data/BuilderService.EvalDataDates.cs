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
        async Task<DateTime[]> EvalDataDates(string current_model, int year, int month, int day, int indice)
        {
            List<DateTime> datesResult = new List<DateTime>();

            var appSetting = ConfigurationHelper.GetAppSettings();

            AppSession _appSession = new AppSession();
            _appSession.EndPointServer = appSetting.profile.Odoo.ApiBaseAddressOdoo;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };

            //bool esActualizacion = false;
            string fechaActualizaTablet = "2021-01-01 00:00:00";

            //Console.WriteLine("Iniciando proceso:" + current_model + " " + DateTime.Now.ToString());

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

                        //if (!forUpdate)
                        //{                            
                        //responseAll = await hubmanager.GetAccountMovesByDate(apiRequest, year, month, day);
                        //}
                        //else
                        //{
                        responseAll = await hubmanager.GetAccountMovesByWriteDate_dl(year, month, day, default_limit, indice);
                        //}

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            //var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.data);

                            //if (resultData != null)
                            //{
                            //    return resultData;
                            //}
                            //foreach(var item in responseAll.data)
                            //{
                            //    datesResult.Add(item.write_date);
                            //}

                            datesResult = responseAll.result
                                .Select(m => m.write_date.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }

                        return datesResult.ToArray();
                    }
                //break;
                case "account_move_line":
                    {
                        //if (!forUpdate)
                        //{
                        ApiManager.HubAccountMoveLine hubmanager = new ApiManager.HubAccountMoveLine(_appSession);
                        var responseAll = await hubmanager.GetAccountMoveLinesByWriteDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.write_date.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }

                        //}
                        //else
                        //{

                        //    ApiManager.HubFactura hubmanager = new ApiManager.HubFactura(_appSession);
                        //    var responseAll = await hubmanager.GetAccountMoveLinesByWriteDate(apiRequest, year, month, day);

                        //    if (responseAll.data != null && responseAll.data.Length > 0)
                        //    {
                        //        var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.data);

                        //        if (resultData != null)
                        //        {
                        //            return resultData;
                        //        }
                        //    }
                        //}
                        return datesResult.ToArray();
                    }
                case "product_template":
                    {
                        ApiManager.HubProductTemplate hubmanager = new ApiManager.HubProductTemplate(_appSession);
                        var responseAll = await hubmanager.GetByWriteDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.write_date.Date)
                                .Distinct()
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
                case "product_product":
                    {
                        var responseAll = await hubProductProduct.GetByWriteDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.write_date.Date)
                                .Distinct()
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
                case "res_partner":
                    {
                        ApiManager.HubPartner hubPartner = new ApiManager.HubPartner(_appSession);
                        
                        //setHubPartner(_appSession);
                        var responseAll = await hubPartner.GetByWriteDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.write_date.Date)
                                .Distinct() // Elimina duplicados
                                .ToList();
                        }
                        return datesResult.ToArray();
                    }
                case "account_journal":
                    {
                        ApiManager.HubJournal hubmanager = new ApiManager.HubJournal(_appSession);
                        var responseAll = await hubmanager.GetByWriteDate_dl(year, month, day);

                        if (responseAll != null && responseAll?.result != null && responseAll.result.Length > 0)
                        {
                            datesResult = responseAll.result
                                .Select(m => m.write_date.Date)
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
