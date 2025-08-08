using ApiManager;
using CobranzasDMSA_Odoo.Models;
using CobranzasDMSA_Odoo.Settings.Sqlite;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Services.ApiHub
{
    public class ApiProcessor
    {
        public async Task<ApiResponse_GUARDAR_COBRO_CXC> EnviarCobro(object obj)
        {
            AccountPaymentHeaderDb _cobReciboCabDb = new AccountPaymentHeaderDb();
            AccountPaymentHeader _cobReciboCab = (AccountPaymentHeader)obj;

            //Se cambia a estado ENVIANDO
            _cobReciboCab.payment_status = DMSA.Models.CobrosEstados.ENVIANDO;
            await _cobReciboCabDb.UpdateAsync(_cobReciboCab);

            //var secuencia = await database.obtenerSecuenciaRecibo(dataItem.CODEMPRESA, App.Session.CurrentUser.codusuario, fechaActual);
            //string secuencia_final = GenerarCodigoRecibo(App.Session.CurrentUser.codusuario, dataItem.CODEMPRESA, fechaActual, secuencia.ToString());

            HubCobros hubCobros = new HubCobros(App.Session);
            //Se obtiene de la base de datos

            ApiRequest_v1 requestObject = new ApiRequest_v1();
            requestObject.uid = App.Session.CurrentUser.uid;
            requestObject.accion = "";
            requestObject.cadenaJson = "";

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ShouldSerializeContractResolver()
            };

            string cabeceraCobro = JsonConvert.SerializeObject(_cobReciboCab, jsonSerializerSettings);
            string detallesCobro = ""; //_cobReciboCab.account_payment_json; //JsonConvert.SerializeObject(itemCobro.DETALLESPAGO);

            //string detallesDocumentos = _cobReciboCab.DETALLESDOCU; //JsonConvert.SerializeObject(itemCobro.DETALLESDOCU);
            string detallesDocumentos = "";// _cobReciboCab.DETALLESDOCU; //JsonConvert.SerializeObject(itemCobro.DETALLESDOCU);
            //var fsObj = null; //JsonConvert.DeserializeObject<detallesDocumentos_forSend[]>(_cobReciboCab.account_payment_invoice_json);
            //detallesDocumentos = JsonConvert.SerializeObject(fsObj);

            Debug.WriteLine(cabeceraCobro);
            Debug.WriteLine(detallesCobro);
            Debug.WriteLine(detallesDocumentos);

            cabeceraCobro = $"\"cabeceraCobro\": {cabeceraCobro},";
            detallesCobro = $"\"detallesCobro\": {detallesCobro},";
            detallesDocumentos = $"\"detallesDocumentos\": {detallesDocumentos}";

            requestObject.cadenaJson = "{" + cabeceraCobro + detallesCobro + detallesDocumentos + "}";

            //File.WriteAllText(Path.Combine(FileSystem.AppDataDirectory, "out.json"), JsonConvert.SerializeObject(fsObj));

            var _responseSend = await hubCobros.Send(requestObject);
            //var responseSend = JsonConvert.DeserializeObject<ApiResponse_GUARDAR_COBRO_CXC>(_responseSend);

            if (_responseSend == null)
            {

            }
            else
            {
                Debug.WriteLine(_responseSend.ToString());
                if (_responseSend.exito)
                {
                    _cobReciboCab.payment_status = DMSA.Models.CobrosEstados.RECIBIDO; // "ENVIADO";
                    await _cobReciboCabDb.UpdateAsync(_cobReciboCab);
                }
            }
            //Para evaluar estructura del JSON
            //var nodex = JsonConvert.DeserializeObject<dynamic>(requestObject.cadenaJson);
            //Debug.WriteLine(nodex);
            return _responseSend;
        }

        

        //Esta clase sirve para intersectar la serialización de una clase, y permite excluir las propiedades
        // especificadas para que no sean serializadas
        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                // Excluir los campos especificados de la serialización
                if (property.PropertyName == "DETALLESNC")
                {
                    property.ShouldSerialize = instance => false;
                }

                return property;
            }
        }
    }
}
