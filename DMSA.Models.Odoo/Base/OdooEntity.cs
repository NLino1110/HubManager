using Newtonsoft.Json.Linq;

namespace DMSA.Models.Odoo.Base
{    

    public abstract class OdooEntity
    {
        /// Método reutilizable para obtener el ID entero desde un JToken
        protected int GetId(JToken token)
        {
            if (token is JArray array && array.Count > 0 && array[0].Type == JTokenType.Integer)
            {
                return (int)array[0];
            }
            else if (token is JValue value && value.Type == JTokenType.Boolean)
            {
                return 0;
            }

            return 0;
        }

        /// Método reutilizable para establecer el ID entero en un JToken (como JArray)
        protected JToken SetId(JToken current, int value)
        {
            if (current is JArray array && array.Count > 0)
            {
                array[0] = value;
                return array;
            }
            return new JArray { value };
        }
    }

}
