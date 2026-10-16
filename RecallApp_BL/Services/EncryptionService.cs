using RecallApp_BL.Services.Interfaces;
using RecallApp_BL.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        public EncryptionService(string base64Key)
        {
            _key = Convert.FromBase64String(base64Key);
            if(_key.Length != 32)
            {
                throw new ArgumentException("Encryption key must be exactly 32 bytes."); 
            }
        }
        public EncryptionResult Encrypt(string plaintext)
        {
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            byte[] nonce = RandomNumberGenerator.GetBytes(12);

            byte[] ciphertext = new byte[plaintextBytes.Length];

            byte[] tag =new byte[16];

            using AesGcm aes =new AesGcm(_key, 16);

            aes.Encrypt(nonce,plaintextBytes,ciphertext,tag);

            return new EncryptionResult
            {
                Ciphertext = ciphertext,
                Nonce = nonce,
                Tag = tag
            };
        }
        public string Decrypt(EncryptionResult encrypted)
        {
            byte[] plaintextBytes = new byte[encrypted.Ciphertext.Length];

            using AesGcm aes = new AesGcm(_key, 16);

            aes.Decrypt(encrypted.Nonce,encrypted.Ciphertext,encrypted.Tag,plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
