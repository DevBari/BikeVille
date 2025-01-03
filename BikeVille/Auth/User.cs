using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;


// Classe che rappresenta l'utente all'interno dell'applicazione
public partial class User
{
    // Identificatore univoco dell'utente
    public int UserId { get; set; }

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

    // Hash della password per la verifica dell'autenticazione
    public string PasswordHash { get; set; } = null!;

    // Salt utilizzato per la protezione della password
    public string PasswordSalt { get; set; } = null!;

    // Ruolo dell'utente nel sistema (opzionale)
    public string? Role { get; set; }

    // Identificatore globale univoco per il tracciamento delle modifiche
    public Guid Rowguid { get; set; }
}
