using System.Security.Cryptography;
using System.Text;

namespace Housing_rental.BLL
{
    public static class PasswordHasher
    {
        public static string ComputeSha256Hash(string password, string salt)
        {
            string input = (password ?? string.Empty) + (salt ?? string.Empty);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();

                foreach (byte item in bytes)
                {
                    builder.Append(item.ToString("X2"));
                }

                return builder.ToString();
            }
        }
    }
}
