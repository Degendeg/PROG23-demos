using System.Security.Cryptography;
using System.Text;

public class EncryptionHelper
{
  private static readonly string _key;

  static EncryptionHelper()
  {
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    _key = configBuilder["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key is missing in the configuration.");
  }

  public static string Encrypt(string plainText)
  {
    using (Aes aesAlg = Aes.Create())
    {
      aesAlg.Key = Encoding.UTF8.GetBytes(_key);
      aesAlg.GenerateIV(); // Create new IV

      var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
      using (var msEncrypt = new MemoryStream())
      {
        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); // Prepend IV to the encrypted data
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
          swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
      }
    }
  }

  public static string Decrypt(string cipherText)
  {
    var fullCipher = Convert.FromBase64String(cipherText);

    using (Aes aesAlg = Aes.Create())
    {
      aesAlg.Key = Encoding.UTF8.GetBytes(_key);
      aesAlg.IV = fullCipher.Take(aesAlg.BlockSize / 8).ToArray(); // Extract IV from the encrypted data

      var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
      using (var msDecrypt = new MemoryStream(fullCipher.Skip(aesAlg.BlockSize / 8).ToArray()))
      using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
      using (var srDecrypt = new StreamReader(csDecrypt))
      {
        return srDecrypt.ReadToEnd();
      }
    }
  }
}