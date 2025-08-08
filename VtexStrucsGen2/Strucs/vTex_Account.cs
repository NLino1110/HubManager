using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucs
{
    public class vTex_Account
    {
        public int Account_Id { get; set; }
        public string Nombre { get; set; }
        public string Internal_Account_Name { get; set; }
        public string Account_Name { get; set; }
        public string Environment { get; set; }
        public string vTexApiKey { get; set; }
        public string vTexApiToken { get; set; }
        public string CampoSkuId { get; set; }
        public override string ToString()
        {
            return Account_Id.ToString().Trim() + " " + Nombre;
        }
    }
}