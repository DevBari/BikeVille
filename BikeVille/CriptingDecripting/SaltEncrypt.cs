using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace BikeVille.CriptingDecripting
{
    public class SaltEncrypt
    {
        public static KeyValuePair<string, string> SaltEncryptPass(string sValue)
        {
            KeyValuePair<string, string> valuePairEncryption;
            byte[] bytesSalt = new byte[6];

            RandomNumberGenerator.Fill(bytesSalt);
            string hashValue = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: sValue,
                salt: bytesSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));


            valuePairEncryption = new KeyValuePair<string, string>(hashValue, Convert.ToBase64String(bytesSalt));



            return valuePairEncryption;
        }

        public static string SaltDecryptPass(string sValue, string sSalt)
        {
            string hashValue = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: sValue,
                salt: Convert.FromBase64String(sSalt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return hashValue;
        }
    }
}
