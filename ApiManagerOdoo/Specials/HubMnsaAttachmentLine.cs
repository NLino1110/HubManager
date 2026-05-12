using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Specials;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiManager
{
    public class HubIrAttachmentLine : HubBase
    {
        string[] fields_array = new[] {
            "id",
            "url",
            "file_name",
            "file_type",
            "total_file_size_expected",
            "success_upload",
            "display_name",            
            "create_date",
            "write_date"            
            };

        public HubIrAttachmentLine(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "mnsa.attachment.line";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int[] ids)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<mnsa_attachment_line[]>?> GetItem(int Id)
        {
            var kwargs = new
            {
                limit = 5,                
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "=", Id },
                
            };
            return await SearchRead<ApiResponseOdooRpcT<mnsa_attachment_line[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<bool>?> Link(int parent_id, int attachment_id)
        { 

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",                
            };

            var kwargs = new { };

            object[] args = new object[]
            {
                    new object[] { parent_id },
                    new Dictionary<string, object>
                    {
                        {
                            "attachment_ids",
                            new object[]
                            {
                                new object[] { 4, attachment_id }
                            }
                        }
                    }
            };
            
            return await Write<ApiResponseOdooRpcT<bool>>(args, kwargs, _modelname);
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendAttachment(mnsa_attachment_line SendObject)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",                
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            JObjectExtensions.RemoveProperty(newJObject, "type");
            JObjectExtensions.RemoveProperty(newJObject, "display_name");
            JObjectExtensions.RemoveProperty(newJObject, "description");
            JObjectExtensions.RemoveProperty(newJObject, "file_size");
            JObjectExtensions.RemoveProperty(newJObject, "url");
            JObjectExtensions.RemoveProperty(newJObject, "local_url");
            JObjectExtensions.RemoveProperty(newJObject, "checksum");

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "ir.attachment");
        }

        public async Task<byte[]> DownloadFileAsync(int recordId)
        {
            string url = $"/web/content/{recordId}?download=true";

            byte[] fileContent = await GetRawBytes(url);

            return fileContent;
        }
    }
}
