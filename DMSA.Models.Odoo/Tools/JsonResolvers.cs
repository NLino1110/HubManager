using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Tools
{
    //public static class JsonResolvers
    //{

    //}

    public class IgnorePropertyResolver : DefaultContractResolver
    {
        private readonly string[] _propertiesToIgnore;

        public IgnorePropertyResolver(params string[] propertiesToIgnore)
        {
            _propertiesToIgnore = propertiesToIgnore;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // Verifica si la propiedad debe ser ignorada
            if (_propertiesToIgnore.Contains(property.PropertyName))
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }

    public class IncludeJsonIgnoreResolver : DefaultContractResolver
    {
        public string[] _propertiesToIgnore;

        public IncludeJsonIgnoreResolver()
        {
            
        }

        public IncludeJsonIgnoreResolver(string[] propertiesToIgnore)
        {
            _propertiesToIgnore = propertiesToIgnore;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            Console.WriteLine(member.Name);

            if (_propertiesToIgnore!= null && _propertiesToIgnore.Length > 0)
            {
                foreach (string propertyName in _propertiesToIgnore) 
                {
                    if (member.Name == propertyName) 
                    {
                        return property;
                    }
                }
            }

            // Ignora la anotación [JsonIgnore]
            property.Ignored = false;

            return property;
        }
    }
}
