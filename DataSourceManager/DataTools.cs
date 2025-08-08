using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSourceManager
{
    public class DataTools
    {
        public static T MapToObject<T>(IDataRecord record) where T : new()
        {
            T obj = new T();
            for (int i = 0; i < record.FieldCount; i++)
            {
                string columnName = record.GetName(i);

                //Debug.WriteLine(columnName);

                // Buscar la propiedad sin distinción de mayúsculas y minúsculas
                var property = typeof(T).GetProperties()
                                        .FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

                if (property != null && record[i] != DBNull.Value)
                {
                    //property.SetValue(obj, record[i]);
                    try
                    {
                        object value = Convert.ChangeType(record[i], property.PropertyType);
                        property.SetValue(obj, value);
                    }
                    catch (InvalidCastException)
                    {
                        // Maneja la excepción si no se puede hacer la conversión
                        throw new ArgumentException($"No se pudo convertir el valor de la columna '{columnName}' al tipo '{property.PropertyType.Name}'.");
                    }
                }
            }
            return obj;
        }

        //public static T MapToObject<T>(IDataRecord record) where T : new()
        //{
        //    T obj = new T();
        //    for (int i = 0; i < record.FieldCount; i++)
        //    {
        //        string columnName = record.GetName(i);
        //        var property = typeof(T).GetProperty(columnName);
        //        if (property != null && record[i] != DBNull.Value)
        //        {
        //            property.SetValue(obj, record[i]);
        //        }
        //    }
        //    return obj;
        //}
    }
}
