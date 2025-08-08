namespace DMSA.Models.Odoo.Tools
{
    using Newtonsoft.Json.Linq;

    public static class JObjectExtensions
    {
        public static void RenameProperty(JObject obj, string sourceProperty, string targetProperty)
        {
            if (obj == null) return;

            // Elimina el campo destino si ya existe
            if (obj[targetProperty] != null)
            {
                obj.Remove(targetProperty);
            }

            // Renombra el campo origen al destino si existe
            if (obj[sourceProperty] != null)
            {
                obj[targetProperty] = obj[sourceProperty];
                obj.Remove(sourceProperty);
            }
        }

        public static void RemoveProperty(JObject obj, string targetProperty)
        {
            if (obj == null) return;
            
            if (obj[targetProperty] != null)
            {
                obj.Remove(targetProperty);
            }
        }

        public static void RemovePropertyFromOrderLineItems(JObject jObject, string propertyName)
        {
            if (jObject.TryGetValue("order_line", out var orderLinesToken) && orderLinesToken is JArray orderLinesArray)
            {
                foreach (var item in orderLinesArray)
                {
                    // Aseguramos que sea un array tipo [0, 0, { sale_order_line }]
                    if (item is JArray innerArray && innerArray.Count > 2 && innerArray[2] is JObject lineObj)
                    {
                        lineObj.Property(propertyName)?.Remove();
                    }
                }
            }
        }
    }

}
