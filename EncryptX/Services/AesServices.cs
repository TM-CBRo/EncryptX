using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EncryptX.Services
{
    public class AesService
    {
        // ==========================================
        // PARTEA 1: TEXT (O aveai deja)
        // ==========================================
        public string EncryptText(string plainText, string password)
        {
            byte[] salt = GenerateRandomEntropy();
            byte[] iv = GenerateRandomEntropy(16);

            using (var passwordDerive = new Rfc2898DeriveBytes(password, salt, 50000))
            {
                byte[] key = passwordDerive.GetBytes(32);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var ms = new MemoryStream())
                    {
                        ms.Write(salt, 0, salt.Length);
                        ms.Write(iv, 0, iv.Length);

                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public string DecryptText(string encryptedBase64, string password)
        {
            byte[] cipherBytesTotal = Convert.FromBase64String(encryptedBase64);

            using (var msIn = new MemoryStream(cipherBytesTotal))
            {
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

        // ==========================================
        // PARTEA 2: FISIERE 
        // ==========================================
        public void EncryptFile(string inputFile, string outputFile, string password)
        {
            byte[] salt = GenerateRandomEntropy();
            byte[] iv = GenerateRandomEntropy(16);

            using (var passwordDerive = new Rfc2898DeriveBytes(password, salt, 50000))
            {
                byte[] key = passwordDerive.GetBytes(32);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        // Scriem Sarea si IV-ul la inceputul fisierului
                        fsOut.Write(salt, 0, salt.Length);
                        fsOut.Write(iv, 0, iv.Length);

                        using (var cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        using (var fsIn = new FileStream(inputFile, FileMode.Open))
                        {
                            // Copiem datele prin tunelul de criptare
                            fsIn.CopyTo(cs);
                        }
                    }
                }
            }
        }

        public void DecryptFile(string inputFile, string outputFile, string password)
        {
            using (var fsIn = new FileStream(inputFile, FileMode.Open))
            {
                byte[] salt = new byte[32];
                byte[] iv = new byte[16];

                // Citim Sarea si IV-ul de la inceput
                fsIn.Read(salt, 0, salt.Length);
                fsIn.Read(iv, 0, iv.Length);

                using (var passwordDerive = new Rfc2898DeriveBytes(password, salt, 50000))
                {
                    byte[] key = passwordDerive.GetBytes(32);

                    using (var aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;

                        using (var cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        using (var fsOut = new FileStream(outputFile, FileMode.Create))
                        {
                            cs.CopyTo(fsOut);
                        }
                    }
                }
            }
        }

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