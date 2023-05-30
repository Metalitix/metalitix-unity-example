using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Metalitix.Scripts.Runtime.Logger.Extensions
{
    public static class AESHelper
    {
        private static string KEY = "25vgCN01Kcla7YNFjB1NA2eeg6aGgF8V";
        
        public static string AESDecrypt(string encryptedPhrase)
        {
            var strings = encryptedPhrase.Split(":");
            var iv = strings[0];
            var phrase = strings[1];
            
            using Aes aesAlgorithm = Aes.Create();

            try
            {
                aesAlgorithm.Mode = CipherMode.CBC;
                aesAlgorithm.Padding = PaddingMode.PKCS7;
                aesAlgorithm.Key = Encoding.UTF8.GetBytes(KEY);
                aesAlgorithm.IV = ConvertHexStringToByteArray(iv);

                ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor();

                byte[] cipher = ConvertHexStringToByteArray(phrase);

                using MemoryStream memoryStream = new MemoryStream(cipher);
                using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                using StreamReader streamReader = new StreamReader(cryptoStream);
                return streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data; 
        }
    }
}