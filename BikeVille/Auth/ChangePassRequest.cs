namespace BikeVille.Auth
{
    // Classe per rappresentare una richiesta di cambio password
    public class ChangePassRequest
    {
        // Nuova password da impostare
        public string Password { get; set; }

        // Identificatore univoco dell'utente
        public int Id { get; set; }
    }
}
