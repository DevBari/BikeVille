using System.ComponentModel.DataAnnotations;
namespace BikeVille.Auth
{
    // Classe per rappresentare una richiesta di cambio password
    public class ChangePassRequest
    {
        [Required]
        [MinLength(8)]
        // Nuova password da impostare
        public string Password { get; set; }


        [Required]
        //Identificatore univoco dell'utente
        public int Id { get; set; }
    }
}
