using RecallApp_BL.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Interfaces
{
    public interface IEncryptionService
    {
        EncryptionResult Encrypt(string plaintext);
        string Decrypt(EncryptionResult encryptionResult);
    }
    
}
