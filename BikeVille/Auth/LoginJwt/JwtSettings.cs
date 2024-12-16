namespace AuthJwt.Auth
{

    // Classe di configurazione per le impostazioni del JWT
    public class JwtSettings
    {
        // Chiave segreta utilizzata per firmare il token
        public string SecretKey { get; set; }

        // Emittente del token JWT
        public string Issuer { get; set; }

        // Destinatario previsto del token JWT
        public string Audience { get; set; }

        // Durata di validità del token espressa in minuti
        public int TokenExpirationMinutes { get; set; }
    }
}
