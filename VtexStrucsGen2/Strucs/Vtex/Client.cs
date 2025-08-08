using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex
{
    public class Client
    {
        public string id { get; set; }
        public string document { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string? homePhone { get; set; }
        public string? phone { get; set; }
        public Checkouttag? checkouttag { get; set; }
        public string? rclastcart { get; set; }
        public Carttag? carttag { get; set; }
        public string? rclastcartvalue { get; set; }
        public bool professional { get; set; }
        public int? academy { get; set; }
        public DateTime createdIn { get; set; }
        public DateTime? updatedIn { get; set; }
        public DateTime? birthDate { get; set; }
        public string? gender { get; set; }
    }

    public class Checkouttag
    {
        public string DisplayValue { get; set; }
        public Scores Scores { get; set; }
    }

    public class Scores
    {
        public Carrinho[] Carrinho { get; set; }
    }

    public class Carrinho
    {
        public float Point { get; set; }
        public DateTime Date { get; set; }
        public DateTime Until { get; set; }
    }

    public class Carttag
    {
        public string DisplayValue { get; set; }
        public Scores1 Scores { get; set; }
    }

    public class Scores1
    {
        public _29876[] _29876 { get; set; }
        public _29440[] _29440 { get; set; }
        public _25389[] _25389 { get; set; }
    }

    public class _29876
    {
        public float Point { get; set; }
        public DateTime Date { get; set; }
        public DateTime Until { get; set; }
    }

    public class _29440
    {
        public float Point { get; set; }
        public DateTime Date { get; set; }
        public DateTime Until { get; set; }
    }

    public class _25389
    {
        public float Point { get; set; }
        public DateTime Date { get; set; }
        public DateTime Until { get; set; }
    }
}
