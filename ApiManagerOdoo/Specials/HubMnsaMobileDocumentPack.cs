using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Specials;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubMnsaMobileDocumentPack : HubBase
    {
        static readonly string[] Fields =
        {
            "id",
            "name",
            "date_from",
            "date_to",
            "state",
            "header_count",
            "line_count",
            "attachment_id",
        };

        public HubMnsaMobileDocumentPack(AppSession setAppSession) : base(setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "mnsa.mobile.document.pack";
        }

        /// <summary>
        /// Último paquete done con ZIP. No genera el pack (el cron de las 03:00 lo arma).
        /// </summary>
        public async Task<mnsa_mobile_document_pack?> GetLatestDonePackAsync()
        {
            var kwargs = new
            {
                limit = 1,
                order = "date_to desc, id desc",
                fields = Fields
            };

            object[] args = Array.Empty<object>();
            object[] domain = new object[]
            {
                new object[] { "state", "=", "done" },
                new object[] { "attachment_id", "!=", false },
            };

            var response = await SearchRead<ApiResponseOdooRpcT<mnsa_mobile_document_pack[]>>(
                args, domain, kwargs, true);

            if (response?.result == null || response.result.Length == 0)
                return null;

            return response.result[0];
        }

        public Task DownloadAttachmentToFileAsync(int attachmentId, string destPath)
        {
            return DownloadToFileAsync($"/web/content/{attachmentId}?download=true", destPath);
        }
    }
}
