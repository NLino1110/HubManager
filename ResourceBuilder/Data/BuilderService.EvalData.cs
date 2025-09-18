using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using Models.DMSA.Shared.Tools;
using DMSA.Models.Security;

namespace ResourceBuilder.Data
{
    public partial class BuilderService
    {
        async Task<string> EvalData(string current_model, int year, int month, int day, int indice, bool forUpdate)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();

            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo;

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
            apiRequest.update = forUpdate;
            apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

            switch (current_model)
            {
                case "account_move":
                    {
                        ApiManager.HubAccountMove hubmanager = new ApiManager.HubAccountMove(_appSession);
                        ApiResponseOdooRpcT<account_move[]>? responseAll = null;
                        if (!forUpdate)
                        {
                            responseAll = await hubmanager.GetAccountMovesByDate(year, month, day, default_limit, indice);
                        }
                        else
                        {
                            responseAll = await hubmanager.GetAccountMovesByWriteDate(year, month, day, default_limit, indice);
                        }

                        if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                        {
                            var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                            if (resultData != null)
                            {
                                return resultData;
                            }
                        }
                    }
                    break;
                case "account_move_line":
                    {
                        ApiManager.HubAccountMoveLine hubmanager = new ApiManager.HubAccountMoveLine(_appSession);
                        //ApiResponse_account_move_line? responseAll = null;
                        ApiResponseOdooRpcT<account_move_line[]>? responseAll = null;
                        if (!forUpdate)
                        {
                            responseAll = await hubmanager.GetAccountMoveLinesByDate(year, month, day);
                        }
                        else
                        {
                            responseAll = await hubmanager.GetAccountMoveLinesByWriteDate(year, month, day);
                        }

                        if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                        {
                            var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                            if (resultData != null)
                            {
                                return resultData;
                            }
                        }
                    }
                    break;
                case "product_template":
                    {
                        ApiManager.HubProductTemplate hubmanager = new ApiManager.HubProductTemplate(_appSession);
                        //ApiResponseProduct? responseAll = null;
                        ApiResponseOdooRpcT<product_template[]>? responseAll = null;
                        if (!forUpdate)
                        {
                            responseAll = await hubmanager.GetByCreateDate(year, month, day, default_limit, indice);
                        }
                        else
                        {
                            responseAll = await hubmanager.GetByWriteDate(year, month, day);
                        }

                        if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                        {
                            var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                            if (resultData != null)
                            {
                                return resultData;
                            }
                        }
                    }
                    break;
                case "product_product":
                    {
                        ApiResponseOdooRpcT<product_product[]>? responseAll = null;
                        if (!forUpdate)
                        {
                            responseAll = await hubProductProduct.GetByCreateDate(default_limit, indice, year, month, day);
                        }
                        else
                        {
                            responseAll = await hubProductProduct.GetByWriteDate(year, month, day);
                        }

                        if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                        {
                            var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                            if (resultData != null)
                            {
                                return resultData;
                            }
                        }
                    }
                    break;
                case "res_partner":
                    {
                        ApiManager.HubPartner hubPartner = new ApiManager.HubPartner(_appSession);
                        ApiResponseOdooRpcT<res_partner[]>? responseAll = null;

                        if (!forUpdate)
                        {
                            responseAll = await hubPartner.GetByCreateDate(year, month, day);
                        }
                        else
                        {
                            responseAll = await hubPartner.GetByWriteDate(year, month, day);
                        }

                        if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                        {
                            var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                            if (resultData != null)
                            {
                                return resultData;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error obteniendo datos de " + current_model + ". Deberia obtenerse datos.");
                        }
                    }
                    break;
                case "account_journal":
                    {
                        ApiManager.HubJournal hubmanager = new ApiManager.HubJournal(_appSession);
                        ApiResponseOdooRpcT<account_journal[]>? responseAll = null;

                        if (!forUpdate)
                        {
                            responseAll = await hubmanager.GetByCreateDate(year, month, day);
                        }
                        else
                        {
                            responseAll = await hubmanager.GetByWriteDate(year, month, day);
                        }

                        if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                        {
                            var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                            if (resultData != null)
                            {
                                return resultData;
                            }
                        }
                    }
                    break;
            }

            return null;
        }
    }
}
