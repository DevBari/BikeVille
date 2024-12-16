using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace BikeVille.CriptingDecripting
{
    // Classe per la gestione della crittografia delle password con salt
    public class SaltEncrypt
    {
        // Metodo per generare un hash della password con un salt casuale
        public static KeyValuePair<string, string> SaltEncryptPass(string sValue)
        {
            KeyValuePair<string, string> valuePairEncryption;
            byte[] bytesSalt = new byte[6];

            // Genera un salt casuale sicuro
            RandomNumberGenerator.Fill(bytesSalt);

            // Genera l'hash della password utilizzando il salt
            string hashValue = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: sValue,
                salt: bytesSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,    // Numero di iterazioni per aumentare la sicurezza
                numBytesRequested: 32));  // Lunghezza dell'hash generato

            // Crea un KeyValuePair contenente l'hash e il salt
            valuePairEncryption = new KeyValuePair<string, string>(hashValue, Convert.ToBase64String(bytesSalt));

            return valuePairEncryption;
        }

        // Metodo per verificare una password confrontandola con un hash salvato
        public static string SaltDecryptPass(string sValue, string sSalt)
        {
            // Genera l'hash della password fornita usando il salt esistente
            string hashValue = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: sValue,
                salt: Convert.FromBase64String(sSalt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,    // Numero di iterazioni per sicurezza
                numBytesRequested: 32));  // Lunghezza dell'hash generato

            return hashValue;
        }
    }
}
