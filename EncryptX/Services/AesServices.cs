using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EncryptX.Services
{
    public class AesService
    {
        // Aceasta functie cripteaza un text folosind o parola
        public string EncryptText(string plainText, string password)
        {
            // 1. Generam sarea (Salt) si derivam cheia
            byte[] salt = GenerateRandomEntropy();
            byte[] iv = GenerateRandomEntropy(16); // IV trebuie sa fie 16 bytes

            using (var passwordDerive = new Rfc2898DeriveBytes(password, salt, 50000))
            {
                byte[] key = passwordDerive.GetBytes(32); // AES-256

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var ms = new MemoryStream())
                    {
                        // Scriem sarea si IV-ul la inceputul rezultatului (necriptate)
                        ms.Write(salt, 0, salt.Length);
                        ms.Write(iv, 0, iv.Length);

                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }

                        // Returnam totul ca un sir Base64 (litere si cifre)
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        // Aceasta functie decripteaza textul
        public string DecryptText(string encryptedBase64, string password)
        {
            byte[] cipherBytesTotal = Convert.FromBase64String(encryptedBase64);

            using (var msIn = new MemoryStream(cipherBytesTotal))
            {
                // Citim sarea (32 bytes) si IV-ul (16 bytes) de la inceput
                byte[] salt = new byte[32];
                byte[] iv = new byte[16];
                msIn.Read(salt, 0, 32);
                msIn.Read(iv, 0, 16);

                using (var passwordDerive = new Rfc2898DeriveBytes(password, salt, 50000))
                {
                    byte[] key = passwordDerive.GetBytes(32);

                    using (var aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;

                        using (var msOut = new MemoryStream())
                        using (var cs = new CryptoStream(msIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            cs.CopyTo(msOut);
                            return Encoding.UTF8.GetString(msOut.ToArray());
                        }
                    }
                }
            }
        }

        // Functie ajutatoare pentru generare aleatoare
        private byte[] GenerateRandomEntropy(int size = 32)
        {
            var randomBytes = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}