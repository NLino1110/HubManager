//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace AppManagerOdoo
{
    using DMSA.Models.Odoo.Native;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public partial class AbstractionTool
    {        
        public AbstractionTool()
        {
            //Console.WriteLine("Iniciado");
        }

        public TResult TransformFull<TInput, TResult, TInputData>
            (
                TInput input, 
                IEnumerable<TInputData> moves, 
                List<Dictionary<string, string>> fieldsMapper
            ) where TInput : class where TResult : class
        {
            //Se ejecuta la asignación automática estructura Odoo => estructura de aplicación
            var _resultTranform = BuildFullStruct(moves, fieldsMapper);
            
            //Se almacena en json los resultados
            Type? type = typeof(TResult);
            string json = JsonConvert.SerializeObject(_resultTranform, type, null);

            //Se transforma desde un tipo a otro por medio de Json
            var resultTranform = Transform<TInput, TResult>(input);

            //Obtener por ejemplo el tipo del campo "data" (FacNotaCreditoCab[]) y realizar la conversión 
            // json y asignarla al campo resultTranform.data
            //resultTranform.data = JsonConvert.DeserializeObject<FacNotaCreditoCab[]>(json);

            PropertyInfo dataProperty = typeof(TResult).GetProperty("data");

            if (dataProperty != null)
            {
                var dataType = dataProperty.PropertyType;
                var data = JsonConvert.DeserializeObject(json, dataType);                                
                dataProperty.SetValue(resultTranform, data, null);
            }

            return resultTranform;
        }

        public TResult Transform<TInput, TResult>(TInput input) where TInput : class where TResult : class
        {
            string _intermediate_ = JsonConvert.SerializeObject(input);
            var resultTranform = JsonConvert.DeserializeObject<TResult>(_intermediate_);
            return resultTranform;
        }

        public List<Dictionary<string, object>> BuildFullStruct<T>(IEnumerable<T> moves, List<Dictionary<string, string>> fieldsMapper)
        {
            //fieldsMapper_FAC_NOTACREDITO_CAB
            var _resultTranform = BuildMatch(ConvertToDictionaryList<T>(moves), fieldsMapper);

            return _resultTranform;

            ////string json = JsonConvert.SerializeObject(_resultTranform);

            //////ApiResponseOdoo resultTest = (ApiResponseOdoo)resultNative;
            //////ApiResponseOdoo resultTest2 = resultNative as ApiResponseOdoo;

            ////// Tipo que deseas instanciar
            ////Type tipoObjeto = typeof(T);

            ////object resultTranform = Activator.CreateInstance(tipoObjeto);

            //////var resultTranform = new ApiResponse_OBTENER_FAC_NOTACREDITO_CAB();

            ////string _intermediate_ = JsonConvert.SerializeObject(resultNative);

            ////resultTranform = JsonConvert.DeserializeObject<ApiResponse_OBTENER_FAC_NOTACREDITO_CAB>(_intermediate_);

            //////ApiResponse_OBTENER_FAC_NOTACREDITO_CAB
            ////resultTranform.data = JsonConvert.DeserializeObject<FacNotaCreditoCab[]>(json);

            ////return resultTranform;
        }


        public List<Dictionary<string, object>> ConvertToDictionaryList<T>(IEnumerable<T> moves)
        {
            var dictionaryList = new List<Dictionary<string, object>>();
            var properties = typeof(T).GetProperties();

            foreach (var move in moves)
            {
                var dictionary = new Dictionary<string, object>();

                foreach (var property in properties)
                {
                    var value = property.GetValue(move);

                    // Convertir valores nulos a DBNull.Value
                    if (value == null)
                    {
                        value = DBNull.Value;
                    }

                    dictionary[property.Name] = value;
                }

                dictionaryList.Add(dictionary);
            }

            return dictionaryList;
        }

        public List<Dictionary<string, object>> BuildMatch(List<Dictionary<string, object>> sourceData, List<Dictionary<string, string>> fieldsMapper)
        {
            List<Dictionary<string, object>> jsonDestino = new List<Dictionary<string, object>>();

            foreach (var objetoOrigen in sourceData)
            {
                var objetoDestino = new Dictionary<string, object>();
                Console.WriteLine(objetoOrigen);

                foreach (var mapeoItem in fieldsMapper)
                {
                    if (mapeoItem["source_list"] != "none")
                    {
                        var typeObj = objetoOrigen[mapeoItem["source_list"]].GetType();

                        if(typeObj.IsArray)
                        {

                        }

                        Array objectEval = (Array) objetoOrigen[mapeoItem["source_list"]];
                        
                        //var objectEval = (List<Dictionary<string, object>>) objetoOrigen[mapeoItem["source_list"]];
                        //var objectEval = objetoOrigen[mapeoItem["source_list"]];

                        foreach (var objectEvalItem in objectEval)
                        {
                            Type tipo = objectEvalItem.GetType();
                            
                            //PropertyInfo propiedad = tipo.GetProperty("id");
                            //object valor = propiedad.GetValue(objectEvalItem);

                            var atributos = new Dictionary<string, object>();
                            foreach (var kvp in tipo.GetProperties())
                            {
                                atributos[kvp.Name] = kvp.GetValue(objectEvalItem);
                            }

                            objetoDestino[mapeoItem["target_field"]] = AnalizarValor(mapeoItem["source_field"], atributos);
                        }
                    }
                    else
                    {
                        if (mapeoItem["source_field"] != "none")
                        {                            
                            if (mapeoItem.ContainsKey("numeric") && mapeoItem["numeric"] == "true")
                            {
                                if (mapeoItem.ContainsKey("integer") && mapeoItem["integer"] == "true")
                                {
                                    objetoDestino[mapeoItem["target_field"]] = ToInteger(AnalizarValor(mapeoItem["source_field"], objetoOrigen).ToString());
                                }
                                else
                                {
                                    objetoDestino[mapeoItem["target_field"]] = ToDouble(AnalizarValor(mapeoItem["source_field"], objetoOrigen).ToString());
                                }
                            }
                            else
                            {
                                //if (mapeoItem.ContainsKey("datetime") && mapeoItem["datetime"] == "true")
                                //{
                                //    objetoDestino[mapeoItem["target_field"]] = mapeoItem["source_field"];
                                //}
                                //else
                                //{
                                    objetoDestino[mapeoItem["target_field"]] = AnalizarValor(mapeoItem["source_field"], objetoOrigen).ToString();
                                //}
                            }
                        }
                        else
                        {                            
                            objetoDestino[mapeoItem["target_field"]] = mapeoItem["target_value"];
                        }
                    }
                }

                jsonDestino.Add(objetoDestino);
            }

            return jsonDestino;
        }

        private object AnalizarValor(string valor, Dictionary<string, object> contexto = null)
        {
            // Patrón de expresión regular para encontrar nombres de campo entre llaves {}
            var patronCampos = @"\{([a-zA-Z_][a-zA-Z0-9_]*)\}";

            // Buscar todas las coincidencias de nombres de campo en la cadena
            var camposEncontrados = new List<string>();
            foreach (Match match in Regex.Matches(valor, patronCampos))
            {
                camposEncontrados.Add(match.Groups[1].Value);
            }

            if (camposEncontrados.Count > 0)
            {
                // Si se encontraron campos entre llaves {}, reemplazarlos con sus valores
                foreach (var campo in camposEncontrados)
                {
                    if (contexto != null && contexto.ContainsKey(campo))
                    {
                        valor = valor.Replace("{" + campo + "}", contexto[campo].ToString());
                    }
                    else
                    {
                        // En caso de que el campo no esté en el contexto, dejarlo sin cambios
                    }
                }
                return valor;
            }
            else
            {
                // Si no se encontraron campos entre llaves {}, asumir que es un campo único
                return valor;
            }
        }

        private object ToDouble(string valor)
        {
            double numero;
            if (double.TryParse(valor, out numero))
            {
                return numero;
            }
            else
            {
                // Si no se puede convertir a número, devuelve el valor original
                return valor;
            }
        }

        private object ToInteger(string valor)
        {
            int numero;
            if (int.TryParse(valor, out numero))
            {
                return numero;
            }
            else
            {
                // Si no se puede convertir a número, devuelve el valor original
                return valor;
            }
        }
    }
}
