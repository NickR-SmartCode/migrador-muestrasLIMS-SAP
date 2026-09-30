using DTO;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Helpers
{
    public class Encrypter
    {
        private readonly string _hash;

        public Encrypter(IOptions<GlobalParameters> options)
        {
            _hash = options.Value.ProjectHash;
        }

        public string Encrypt(string dato)
        {
            byte[] data = Encoding.UTF8.GetBytes(dato);

            using MD5 md5 = MD5.Create();
            using TripleDES tripDES = TripleDES.Create();

            tripDES.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_hash));
            tripDES.Mode = CipherMode.ECB;

            ICryptoTransform transform = tripDES.CreateEncryptor();
            byte[] result = transform.TransformFinalBlock(data, 0, data.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string dato)
        {
            dato = dato.Replace(" ", "+");
            byte[] data = Convert.FromBase64String(dato);

            using MD5 md5 = MD5.Create();
            using TripleDES tripDES = TripleDES.Create();

            tripDES.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_hash));
            tripDES.Mode = CipherMode.ECB;

            ICryptoTransform transform = tripDES.CreateDecryptor();
            byte[] result = transform.TransformFinalBlock(data, 0, data.Length);

            return Encoding.UTF8.GetString(result);
        }

        public string ObtenerSHA256String(string texto)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            byte[] hash = sha256.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}
