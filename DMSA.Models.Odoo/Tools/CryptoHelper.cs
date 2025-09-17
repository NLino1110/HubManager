using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Tools
{
    public class CryptoHelper
    {
        // Clave secreta (debe ser de 16, 24 o 32 bytes para AES)
        private static readonly string key = "MiClaveSuperSecreta12345";
        // Vector de inicialización (16 bytes)
        private static readonly string iv = "VectorInit123456";

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                string encrypted;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    // ✅ Se obtiene el contenido del MemoryStream ANTES de salir del using
                    encrypted = Convert.ToBase64String(ms.ToArray());
                }

                return encrypted;
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                using (MemoryStream ms = new MemoryStream(buffer))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
