using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    public class ABase
    {
        public void set_to_token(JToken token, int value)
        {            
            if (token is JArray array && array.Count > 0)
            {
                array[0] = value;
            }
            else if (token is JValue)
            {
                token = new JArray { value };
            }
            else
            {
                token = new JArray { value };
            }
        }

        public int get_from_token (JToken token)
        {            
            if (token is JArray array && array.Count > 0)
            {
                return array[0].Type == JTokenType.Integer ? (int)array[0] : 0;
            }

            else if (token is JValue value && value.Type == JTokenType.Boolean)
            {
                return 0;
            }
            return 0;
        }
    }
}
