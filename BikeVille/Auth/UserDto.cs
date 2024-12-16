namespace BikeVille.Auth
{
    // Classe Data Transfer Object (DTO) per rappresentare un utente
    public class UserDto
    {
        // Titolo dell'utente (opzionale)
        public string? Title { get; set; }

        // Nome dell'utente (campo obbligatorio)
        public string FirstName { get; set; } = null!;

        // Secondo nome dell'utente (opzionale)
        public string? MiddleName { get; set; }

        // Cognome dell'utente (campo obbligatorio)
        public string LastName { get; set; } = null!;

        // Suffisso del nome (opzionale)
        public string? Suffix { get; set; }

        // Indirizzo email dell'utente (opzionale)
        public string? EmailAddress { get; set; }

        // Numero di telefono dell'utente (opzionale)
        public string? Phone { get; set; }

        // Password dell'utente (campo obbligatorio)
        public string Password { get; set; } = null!;

        // Ruolo dell'utente nel sistema (opzionale)
        public string? Role { get; set; }
    }
}
