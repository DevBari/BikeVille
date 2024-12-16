namespace BikeVille
{
    public class GoogleAuthSettings
    {
        /// <summary>
        /// Il Client ID dell'applicazione Google
        /// </summary>
        public string GoogleClientId { get; set; }

        /// <summary>
        /// L'endpoint del provider di autenticazione (opzionale se necessario)
        /// </summary>
        public string Issuer { get; set; } = "https://accounts.google.com";

        /// <summary>
        /// Qualsiasi altra configurazione specifica del provider
        /// </summary>
        public string Audience { get; set; }
    }
}