using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Xml.Linq;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using FastMember;

/* 
* Autor Jackson Rosado 
* TRansforma el contenido de estructuras, coleciones y Datatables en contenido XML para 
* el intercambio de información con la base de datos.
*/
namespace ResourceBuilder.Services.Sync
{
    public static class SiscomDataTransport
    {
        /// <summary>
        /// Asigna el contenido de un datatable a una entidad, si el datatable tiene varios registro
        /// devolverá el contenido del ultimo registro 
        /// Autor: Jackson Rosado
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dtOrigen"></param>
        /// <param name="Entidad"></param>
        public static void DataTableToEntidad<T>(DataTable dtOrigen, T Entidad)
        {
            PropertyInfo[] prop1 = Entidad.GetType().GetProperties();
            foreach (DataRow row in dtOrigen.Rows)
            {
                for (int i = 0; i < prop1.Length; i++)
                {
                    if (row.Table.Columns.Contains(prop1[i].Name.ToUpper()))
                    {
                        //Name, prop1[i].GetValue(item, null).ToString());
                        if (row[prop1[i].Name.ToUpper()] != System.DBNull.Value) //&& row[prop1[i].Name.ToUpper()]!=System.DBNull
                            prop1[i].SetValue(Entidad, row[prop1[i].Name.ToUpper()], null);
                    }
                }
            }
        }

        /// <summary>
        /// Asigna el contenido de un datatable a una entidad, si el datatable tiene varios registro
        /// devolverá el contenido del ultimo registro 
        /// Autor: Jackson Rosado
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dtOrigen"></param>
        /// <param name="Entidad"></param>
        public static void RowToEntidad<T>(DataRow row, T Entidad)
        {
            PropertyInfo[] prop1 = Entidad.GetType().GetProperties();
            for (int i = 0; i < prop1.Length; i++)
            {
                if (row.Table.Columns.Contains(prop1[i].Name.ToUpper()))
                {
                    if (row[prop1[i].Name.ToUpper()] != System.DBNull.Value)
                    {
                        if (row[prop1[i].Name.ToUpper()].GetType().Name == prop1[i].PropertyType.Name)
                            prop1[i].SetValue(Entidad, row[prop1[i].Name.ToUpper()], null);
                        else
                        {
                            switch (prop1[i].PropertyType.Name)
                            {
                                case "Int64":
                                    {
                                        Int64 valor;
                                        Int64.TryParse(row[prop1[i].Name.ToUpper()].ToString(), out valor);
                                        prop1[i].SetValue(Entidad, valor, null);
                                    }
                                    break;
                                case "Int32":
                                    {
                                        Int32 valor;
                                        Int32.TryParse(row[prop1[i].Name.ToUpper()].ToString(), out valor);
                                        prop1[i].SetValue(Entidad, valor, null);
                                    }
                                    break;
                                case "Decimal":
                                    {
                                        Decimal valor;
                                        Decimal.TryParse(row[prop1[i].Name.ToUpper()].ToString(), out valor);
                                        prop1[i].SetValue(Entidad, valor, null);
                                    }
                                    break;

                                default:
                                    {
                                        if (prop1[i].PropertyType.GenericTypeArguments.Length > 0)
                                        {
                                            switch (prop1[i].PropertyType.GenericTypeArguments[0].Name)
                                            {
                                                case "Double":
                                                    Double valor;
                                                    Double.TryParse(row[prop1[i].Name.ToUpper()].ToString(), out valor);
                                                    prop1[i].SetValue(Entidad, valor, null);
                                                    break;
                                                default:
                                                    prop1[i].SetValue(Entidad, row[prop1[i].Name.ToUpper()].ToString(), null);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            prop1[i].SetValue(Entidad, row[prop1[i].Name.ToUpper()].ToString(), null);
                                        }
                                    }
                                    break;
                            }//switch (prop1[i].PropertyType.Name)
                        }//else
                    }
                }
            }
        }

        /// <summary>
        /// Transforma el contenido de una coleccion.Generic.List en XMLDocument
        /// Autor: Jackson Rosado
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="col"></param>
        /// <returns></returns>
        public static XmlDocument CollectionToXml<T>(Collection<T> col)
        {
            //XDocument doc = new XDocument();
            XmlDocument doc = new XmlDocument();
            string xml = "<root>";
            foreach (T item in col)
            {
                string cNombre = item.GetType().Name.ToUpper();
                xml += "<" + cNombre.Trim() + ">";
                PropertyInfo[] prop1 = item.GetType().GetProperties();
                //doc.Add(new XElement(cNombre), 
                for (int i = 0; i < prop1.Length; i++)
                {
                    if (prop1[i].GetValue(item, null) == null)
                    {
                        xml += "<" + prop1[i].Name.ToUpper() + "></" + prop1[i].Name.ToUpper() + ">";
                    }
                    else
                    {
                        xml += "<" + prop1[i].Name.ToUpper() + ">" + prop1[i].GetValue(item, null).ToString() + "</" + prop1[i].Name.ToUpper() + ">";
                    }
                }
                xml += "</" + cNombre.Trim() + ">";
            }
            xml += "</root>";
            xml = xml.Replace("&", "&amp;");
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>
        /// Transforma el contenido de una coleccion.Generic.List en XMLDocument
        /// Autor: Jackson Rosado
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="col"></param>
        /// <returns></returns>
        public static XmlDocument CollectionToXml<T>(System.Collections.Generic.List<T> col)
        {
            //XDocument doc = new XDocument();
            XmlDocument doc = new XmlDocument();
            string xml = "<root>";
            foreach (T item in col)
            {
                string cNombre = item.GetType().Name.ToUpper();
                xml += "<" + cNombre.Trim() + ">";
                PropertyInfo[] prop1 = item.GetType().GetProperties();
                //doc.Add(new XElement(cNombre), 
                for (int i = 0; i < prop1.Length; i++)
                {
                    if (prop1[i].GetValue(item, null) == null)
                    {
                        xml += "<" + prop1[i].Name.ToUpper() + "></" + prop1[i].Name.ToUpper() + ">";
                    }
                    else
                    {
                        if (prop1[i].PropertyType.FullName.ToString() != "System.Byte[]")
                        {
                            xml += "<" + prop1[i].Name.ToUpper() + ">" + prop1[i].GetValue(item, null).ToString() + "</" + prop1[i].Name.ToUpper() + ">";
                        }
                        else
                        {
                            xml += "<" + prop1[i].Name.ToUpper() + ">" +
                                   System.Convert.ToBase64String((byte[])prop1[i].GetValue(item, null), 0, ((byte[])prop1[i].GetValue(item, null)).Length) +
                                   "</" + prop1[i].Name.ToUpper() + ">";
                        }
                    }
                }
                xml += "</" + cNombre.Trim() + ">";
            }
            xml += "</root>";
            xml = xml.Replace("&", "&amp;");
            doc.LoadXml(xml);
            return doc;
        }

        public static XmlDocument EntidadToXml<T>(T entidad)
        {
            //XDocument doc = new XDocument();
            XmlDocument doc = new XmlDocument();
            string xml = "<root>";
            string cNombre = entidad.GetType().Name.ToUpper();
            xml += "<" + cNombre.Trim() + ">";
            PropertyInfo[] prop1 = entidad.GetType().GetProperties();
            //doc.Add(new XElement(cNombre), 
            for (int i = 0; i < prop1.Length; i++)
            {
                if (prop1[i].GetValue(entidad, null) == null)
                    xml += "<" + prop1[i].Name.ToUpper() + "> </" + prop1[i].Name.ToUpper() + ">";
                else
                    xml += "<" + prop1[i].Name.ToUpper() + ">" + prop1[i].GetValue(entidad, null).ToString() + "</" + prop1[i].Name.ToUpper() + ">";
            }
            xml += "</" + cNombre.Trim() + ">";
            xml += "</root>";
            xml = xml.Replace("&", "&amp;");
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static XDocument XmlToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        /// <summary>
        /// Transforma un Datatable a un XML 
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static XmlDocument DataTableToXml(DataTable col)
        {
            //XDocument doc = new XDocument();
            XmlDocument doc = new XmlDocument();
            string xml = "<root>";
            foreach (DataRow item in col.Rows)
            {
                string cNombre = col.TableName;
                xml += "<" + cNombre.Trim() + ">";
                //doc.Add(new XElement(cNombre), 
                foreach (DataColumn columnas in col.Columns)
                {
                    if (item[columnas] == null)
                    {
                        xml += "<" + columnas.ColumnName.ToUpper() + "></" + columnas.ColumnName.ToUpper() + ">";
                    }
                    else
                    {
                        if (columnas.GetType().FullName.ToString() != "System.Byte[]")
                        {
                            xml += "<" + columnas.ColumnName.ToUpper() + ">" + item[columnas].ToString() + "</" + columnas.ColumnName.ToUpper() + ">";
                        }
                        else
                        {
                            xml += "<" + columnas.ColumnName.ToUpper().ToUpper() + ">" +
                                   System.Convert.ToBase64String((byte[])item[columnas], 0, ((byte[])item[columnas]).Length) +
                                   "</" + columnas.ColumnName.ToUpper() + ">";
                        }
                    }
                }
                xml += "</" + cNombre.Trim() + ">";
            }
            xml += "</root>";
            xml = xml.Replace("&", "&amp;");
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>
        /// Transdforma un Datatable a XDocument que es transportable a Oracle.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static XDocument DataTableToXDocument(DataTable col)
        {
            XmlDocument xmlDocument = DataTableToXml(col);
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        /// <summary>
        /// Transforma Tabla a Archivo Texto.
        /// </summary>
        public static void WriteDataToFile(DataTable submittedDataTable, string submittedFilePath, bool includeHeader = false)
        {
            int i = 0;
            StreamWriter sw = null;
            sw = new StreamWriter(submittedFilePath, false);
            if (includeHeader)
            {
                for (i = 0; i < submittedDataTable.Columns.Count - 1; i++)
                {
                    sw.Write(submittedDataTable.Columns[i].ColumnName + ";");
                }
                sw.Write(submittedDataTable.Columns[i].ColumnName);
                sw.WriteLine();
            }
            bool inicio = true;
            foreach (DataRow row in submittedDataTable.Rows)
            {
                if (!(inicio))
                {
                    sw.WriteLine();
                }
                object[] array = row.ItemArray;
                for (i = 0; i < array.Length - 1; i++)
                {
                    sw.Write(array[i].ToString() + ";");
                }
                sw.Write(array[i].ToString());
                inicio = false;
            }
            sw.Close();
        }

        /// <summary>
        /// Lee Excel.
        /// </summary>
        //public static DataSet ImportExcelXls(string fileName, bool hasHeaders)
        //{
        //    string hdr = hasHeaders ? "Yes" : "No";
        //    string strConn;
        //    if (fileName.Substring(fileName.LastIndexOf('.')).ToLower() == ".xlsx")
        //        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;HDR=" + hdr + ";IMEX=0\"";
        //    else
        //        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties=\"Excel 8.0;HDR=" + hdr + ";IMEX=0\"";
        //    DataSet output = new DataSet();
        //    using (OleDbConnection conn = new OleDbConnection(strConn))
        //    {
        //        conn.Open();
        //        DataTable schemaTable = conn.GetOleDbSchemaTable(
        //            OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
        //        foreach (DataRow schemaRow in schemaTable.Rows)
        //        {
        //            string sheet = schemaRow["TABLE_NAME"].ToString();
        //            if (!sheet.EndsWith("_"))
        //            {
        //                try
        //                {
        //                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
        //                    cmd.CommandType = CommandType.Text;
        //                    DataTable outputTable = new DataTable(sheet.Replace("$", ""));
        //                    output.Tables.Add(outputTable);
        //                    new OleDbDataAdapter(cmd).Fill(outputTable);
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, fileName), ex);
        //                }
        //            }
        //        }
        //    }
        //    return output;
        //}

        private static readonly IDictionary<Type, IEnumerable<PropertyInfo>> _Properties =
            new Dictionary<Type, IEnumerable<PropertyInfo>>();
#pragma warning disable CS1591 // Falta el comentario XML para el tipo o miembro visible públicamente
        public static IEnumerable<T> DataTableToList<T>(this DataTable table) where T : class, new()
#pragma warning restore CS1591 // Falta el comentario XML para el tipo o miembro visible públicamente
        {
            var objType = typeof(T);
            IEnumerable<PropertyInfo> properties;

            lock (_Properties)
            {
                if (!_Properties.TryGetValue(objType, out properties))
                {
                    properties = objType.GetProperties().Where(property => property.CanWrite);
                    _Properties.Add(objType, properties);
                }
            }
            var list = new List<T>(table.Rows.Count);
            Parallel.ForEach<DataRow>(table.AsEnumerable().Skip(0), row => {
                var obj = new T();
                foreach (var prop in properties)
                {
                    if (prop != null)
                    {
                        Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        if (row.Table.Columns.Contains(prop.Name))
                        {
                            object propertyValue = (row[prop.Name] == null || row[prop.Name] is DBNull) ? null : Convert.ChangeType(row[prop.Name], t);
                            var accessors = TypeAccessor.Create(objType);
                            accessors[obj, prop.Name] = propertyValue;
                        }
                    }
                }
                var property = obj.GetType().GetProperty("IsChanged");
                if (property != null)
                    property.SetValue(obj, false, null);
                list.Add(obj);
            });
            return list;
        }
    }
}
