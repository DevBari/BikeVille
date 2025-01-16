using System.ComponentModel.DataAnnotations;
namespace LoginJwt.Auth
{
    // Classe per rappresentare le credenziali di accesso
    public class Credentials
    {
        // Indirizzo emai    [Required]
        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [MinLength(8)]
        // Password dell'utente
        public string Password { get; set; }
    }
}
