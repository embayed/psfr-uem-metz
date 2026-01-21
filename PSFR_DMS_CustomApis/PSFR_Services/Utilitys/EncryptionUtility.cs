using System.Security.Cryptography;
using System.Text;

namespace PSFR_Services.Utilitys
{
    public class EncryptionUtility
    {
        private const string _Key = "J9rjU+t1HRFB+hHdyIl9XWHTm0DykG8L";
        private const string _IV = "E1S2N3E4T50=";
        public static string Decrypt(string encryptedText)
        {
            try
            {
                using TripleDES tripleDes = TripleDES.Create();
                tripleDes.Key = Convert.FromBase64String(_Key);
                tripleDes.IV = Convert.FromBase64String(_IV);

                ICryptoTransform decryptor = tripleDes.CreateDecryptor();

                byte[] encryptedBytes = Convert.FromBase64String(encryptedText.Replace('-', '+').Replace('_', '/'));

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Write);
                cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                cryptoStream.FlushFinalBlock();

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Encrypt(string text)
        {
            using TripleDES tripleDes = TripleDES.Create();
            tripleDes.Key = Convert.FromBase64String(_Key);
            tripleDes.IV = Convert.FromBase64String(_IV);

            ICryptoTransform encryptor = tripleDes.CreateEncryptor();

            byte[] plainBytes = Encoding.UTF8.GetBytes(text);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memoryStream.ToArray());
        }
        public static string? HashPasswordSHA1(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] data = SHA1.HashData(bytes);
            return BinaryToHex(data);
        }

        private static string? BinaryToHex(byte[] data)
        {
            if (data == null) return null;

            char[] array = new char[checked(data.Length * 2)];
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                array[2 * i] = NibbleToHex((byte)(b >> 4));
                array[2 * i + 1] = NibbleToHex((byte)(b & 0xFu));
            }

            return new string(array);
        }

        private static char NibbleToHex(byte nibble)
        {
            return (char)(nibble < 10 ? nibble + 48 : nibble - 10 + 65);
        }
    }
}