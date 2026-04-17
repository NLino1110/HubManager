using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tareas;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ApiManager
{
    public class HubProjectTask : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "project_id",
                "display_name",
                "duration_tracking",
                "access_url",
                "access_token",
                "stage_id",
                "tag_ids",
                "state",
                "is_closed",
                "create_date",
                "write_date",
                "date_end",
        };

        public HubProjectTask(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "project.task";
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<ProjectTask[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<ProjectTask[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<ProjectTask[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<ProjectTask[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<ProjectTask[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<ProjectTask[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<ProjectTask[]>?> GetByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<ProjectTask[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<ProjectTask[]>?> GetByNameUser(string task_name, int user_id)
        {
            //Este metodo busca por nombre el projectTask
            // el projectTask representa a la tarea del día, es decir
            // todos los vendedores serán asignados a la misma tarea
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "name", "=", task_name },
                new object[] { "user_ids", "=", user_id },
            };
            return await SearchRead<ApiResponseOdooRpcT<ProjectTask[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<int>?> Create(ProjectTask sale_Order)
        {
            var kwargs = new { };
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver()
            };

            // Serializar incluyendo propiedades marcadas con JsonIgnore si el resolver lo permite
            var serialized = JsonConvert.SerializeObject(sale_Order, settings);

            var newJObject = JObject.Parse(serialized);

            // Debug: JSON original
            Debug.WriteLine("[HubProjectTask] JSON before cleanup: " + newJObject.ToString(Newtonsoft.Json.Formatting.None));

            // Asegurar user_ids: muchos endpoints esperan lista, no solo user_id
            if ((newJObject["user_ids"] == null || newJObject["user_ids"].Type != JTokenType.Array))
            {
                if (sale_Order.user_ids != null && sale_Order.user_ids.Length > 0)
                {
                    newJObject["user_ids"] = JArray.FromObject(sale_Order.user_ids);
                }
                else if (sale_Order.user_id > 0)
                {
                    newJObject["user_ids"] = new JArray(sale_Order.user_id);
                }
            }

            // Si existe 'id' y es 0, quitarlo (create no debe enviar id=0)
            if (newJObject["id"] != null)
            {
                if (int.TryParse(newJObject["id"].ToString(), out int idVal) && idVal == 0)
                {
                    JObjectExtensions.RemoveProperty(newJObject, "id");
                }
            }

            // Quitar propiedades internas o flags que no deben enviarse
            JObjectExtensions.RemoveProperty(newJObject, "create_user");
            JObjectExtensions.RemoveProperty(newJObject, "display_username");
            JObjectExtensions.RemoveProperty(newJObject, "is_synchronized");
            JObjectExtensions.RemoveProperty(newJObject, "date_synchronized");
            JObjectExtensions.RemoveProperty(newJObject, "parent_id");
            JObjectExtensions.RemoveProperty(newJObject, "project_id");
            JObjectExtensions.RemoveProperty(newJObject, "stage_id");
            JObjectExtensions.RemoveProperty(newJObject, "user_id");
            JObjectExtensions.RemoveProperty(newJObject, "id_sync");

            // Eliminar propiedades de la UI / computadas que no existen en el modelo Odoo
            JObjectExtensions.RemoveProperty(newJObject, "state_view");
            JObjectExtensions.RemoveProperty(newJObject, "res_company_display");
            JObjectExtensions.RemoveProperty(newJObject, "motivo_display");
            JObjectExtensions.RemoveProperty(newJObject, "res_partner_display");

            // Renombrar campos internos si existen
            JObjectExtensions.RenameProperty(newJObject, "_partner_id", "partner_id");
            JObjectExtensions.RenameProperty(newJObject, "_company_id", "company_id");
            JObjectExtensions.RenameProperty(newJObject, "_warehouse_id", "warehouse_id");
            JObjectExtensions.RenameProperty(newJObject, "_currency_id", "currency_id");
            JObjectExtensions.RenameProperty(newJObject, "_center_id", "center_id");
            JObjectExtensions.RenameProperty(newJObject, "stage_id_", "stage_id");
            JObjectExtensions.RenameProperty(newJObject, "project_id_", "project_id");

            // Eliminar siempre la propiedad 'state' al crear una tarea.
            // Evita enviar valores locales inválidos que el servidor valida estrictamente.
            JObjectExtensions.RemoveProperty(newJObject, "state");

            // Eliminar fechas con valor por defecto (0001-01-01...) para no enviar valores inválidos
            string[] dateProps = new[] { "date_assign", "date_deadline", "create_date", "write_date", "date_end" };
            foreach (var dp in dateProps)
            {
                if (newJObject[dp] != null)
                {
                    var val = newJObject[dp].ToString();
                    if (val.StartsWith("0001") || string.IsNullOrWhiteSpace(val))
                    {
                        JObjectExtensions.RemoveProperty(newJObject, dp);
                    }
                }
            }

            // Debug: JSON final a enviar
            Debug.WriteLine("[HubProjectTask] JSON after cleanup: " + newJObject.ToString(Newtonsoft.Json.Formatting.None));

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }
    }
}
