using ApiManager;
using DMCobranzas.Models;
using DMCobranzas.Settings.Sqlite;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.DMCobranzas;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Services.ApiHub
{
    public class ApiProcessor
    {

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
