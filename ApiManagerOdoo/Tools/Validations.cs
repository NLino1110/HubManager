using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using RestSharp;

namespace ApiManagerOdoo.Tools
{
    static public class Validations
    {
        static public bool IsValidResponse(RestResponse response)
        {
            //if (response.StatusCode == HttpStatusCode.OK)
            if (response != null && response.Content != null & response.Content != "")
            {
                if (IsValidJson(response.Content))
                {
                    return true;
                }
            }
            return false;
        }
        static public bool IsValidJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            if ((input.StartsWith("{") && input.EndsWith("}")) ||
                (input.StartsWith("[") && input.EndsWith("]")))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject(input);
                    return true;
                }
                catch (JsonException)
                {
                    return false;
                }
            }

            return false;
        }

    }
}
