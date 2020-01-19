using System.Security.Cryptography;
using System.Text;

namespace ContentManagement
{
    public class MD5Hasher : IHasher
    {
        public string Hash(string source)
        {
            using (MD5 hasher = MD5.Create())
            {
                byte[] data = hasher.ComputeHash(Encoding.UTF8.GetBytes(source));

                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}