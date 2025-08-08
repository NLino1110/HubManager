using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Models.DMSA.Shared.Tools;
namespace ResourceBuilder.Data
{
    public partial class BuilderService
    {
        async Task<ApiResponseOdooRpc> EvalCount(string current_model, int year, int month, int day, bool forUpdate)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();

            AppSession _appSession = new AppSession();
            _appSession.EndPointServer = appSetting.profile.Odoo.ApiBaseAddressOdoo;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                username = appSetting.profile.Odoo.User,
                codclave = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };

            //bool esActualizacion = false;
            string fechaActualizaTablet = "2021-01-01 00:00:00";

            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = int.Parse(appSetting.profile.Odoo.uid);
            apiRequest.password = appSetting.profile.Odoo.Password;
            apiRequest.databasename = appSetting.profile.Odoo.Database;
            apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);


            //TODO: CORREGIR A LA NUEVA FORMA

            switch (current_model)
            {
                case "account_move":
                    {
                        ApiManager.HubAccountMove hubmanager = new ApiManager.HubAccountMove(_appSession);
                        ApiResponseOdooRpc? resultCount = null;
                        if (!forUpdate)
                        {
                            resultCount = await hubmanager.GetHeaderCountByDate(year, month, day);
                        }
                        else
                        {
                            resultCount = await hubmanager.GetHeaderCountByWriteDate(year, month, day);
                        }

                        return resultCount;
                    }
                case "account_move_line":
                    {
                        ApiManager.HubAccountMoveLine hubmanager = new ApiManager.HubAccountMoveLine(_appSession);
                        ApiResponseOdooRpc? resultCount = null;
                        if (!forUpdate)
                        {
                            resultCount = await hubmanager.GetAccountMoveLinesCountByDate(year, month, day);
                        }
                        else
                        {
                            resultCount = await hubmanager.GetAccountMoveLinesCountByWriteDate(year, month, day);
                        }
                        return resultCount;
                    }

                case "product_template":
                    {
                        ApiManager.HubProductTemplate hubmanager = new ApiManager.HubProductTemplate(_appSession);
                        ApiResponseOdooRpc? resultCount = null;
                        if (!forUpdate)
                        {
                            resultCount = await hubmanager.GetCountByCreateDate(year, month, day);
                        }
                        else
                        {
                            resultCount = await hubmanager.GetCountByWriteDate(year, month, day);
                        }
                        return resultCount;
                    }
                    break;
                case "product_product":
                    {                        
                        ApiResponseOdooRpc? resultCount = null;
                        if (!forUpdate)
                        {
                            resultCount = await hubProductProduct.GetCountByCreateDate(year, month, day);
                        }
                        else
                        {
                            resultCount = await hubProductProduct.GetCountByWriteDate(year, month, day);
                        }
                        return resultCount;
                    }
                    break;
                case "res_partner":
                    {
                        ApiManager.HubPartner hubPartner = new ApiManager.HubPartner(_appSession);
                       
                        //setHubPartner(_appSession);
                        ApiResponseOdooRpc? resultCount = null;
                        if (!forUpdate)
                        {
                            resultCount = await hubPartner.GetCountByCreateDate(year, month, day);
                        }
                        else
                        {
                            resultCount = await hubPartner.GetCountByWriteDate(year, month, day);
                        }
                        return resultCount;
                    }
                case "account_journal":
                    {

                        ApiManager.HubJournal hubmanager = new ApiManager.HubJournal(_appSession);
                        ApiResponseOdooRpc? resultCount = null;
                        if (!forUpdate)
                        {
                            resultCount = await hubmanager.GetCountByCreateDate(year, month, day);
                        }
                        else
                        {
                            resultCount = await hubmanager.GetCountByWriteDate(year, month, day);
                        }
                        return resultCount;
                    }
            }

            return null;
        }
    }
}
