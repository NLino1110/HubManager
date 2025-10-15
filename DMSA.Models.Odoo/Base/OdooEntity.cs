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

        /// Obtiene IDs de un Many2many desde un JToken.
        /// Acepta: [1,2,3] o cualquier JArray donde los elementos enteros serán recogidos.
        protected int[] GetIds(JToken token)
        {
            if (token == null) return Array.Empty<int>();

            if (token is JArray arr)
            {
                // Caso típico: [1,2,3]
                var ints = arr
                    .Where(t => t != null && t.Type == JTokenType.Integer)
                    .Select(t => (int)t)
                    .ToArray();

                if (ints.Length > 0) return ints;

                // Si viniera algo raro, intentamos dentro de sub-arreglos: [[1], [2], [3]] (defensivo)
                var nested = arr
                    .Where(t => t is JArray ja && ja.Count > 0 && ja[0].Type == JTokenType.Integer)
                    .Select(t => (int)((JArray)t)[0])
                    .ToArray();

                if (nested.Length > 0) return nested;
            }

            // Valor simple no esperado para M2M
            return Array.Empty<int>();
        }

        /// Establece IDs en un Many2many como JArray simple: [1,2,3]
        protected JToken SetIds(JToken current, int[] values)
        {
            if (values == null || values.Length == 0) return new JArray(); // vacío

            var ja = new JArray();
            foreach (var v in values.Where(v => v > 0))
                ja.Add(v);

            return ja;
        }
    }

}
