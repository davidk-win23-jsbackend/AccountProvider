using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccountProvider.Utilities;

public class KeyGenerator
{
    public static string GenerateRandomKey(int keySizeInBytes)
    {
        byte[] keyBytes = new byte[keySizeInBytes];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(keyBytes);
        }
        return Convert.ToBase64String(keyBytes);
    }
}
