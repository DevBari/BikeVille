using BikeVille.Entity.EntityContext;
using Microsoft.EntityFrameworkCore;

namespace BikeVille.Auth.AuthContext;

// Classe per la gestione del contesto del database
public partial class AdventureWorksLt2019usersInfoContext : DbContext
{
    // Costruttore che accetta le opzioni di configurazione del contesto
    public AdventureWorksLt2019usersInfoContext(DbContextOptions<AdventureWorksLt2019usersInfoContext> options)
        : base(options)
    {
    }

    // Tabella virtuale che rappresenta gli utenti nel database
    public virtual DbSet<User> Users { get; set; }

    // Configurazione del modello del database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurazione della tabella "User"
        modelBuilder.Entity<User>(entity =>
        {
               entity.ToTable("User", "dbo");

            // Configurazione delle proprietà della tabella "User"
            entity.HasKey(u => u.UserId); // Imposta UserId come PK
            entity.Property(e => e.UserId).HasColumnName("UserID"); // Mappa la proprietà UserId alla colonna "UserID"
            entity.Property(e => e.EmailAddress).HasMaxLength(50);  // Limita l'email a 50 caratteri
            entity.Property(e => e.FirstName).HasMaxLength(50);     // Limita il nome a 50 caratteri
            entity.Property(e => e.LastName).HasMaxLength(50);      // Limita il cognome a 50 caratteri
            entity.Property(e => e.MiddleName).HasMaxLength(50);    // Limita il secondo nome a 50 caratteri
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(128)        // Limita l'hash della password a 128 caratteri
                .IsUnicode(false);        // Disabilita i caratteri Unicode
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(10);        // Limita il salt della password a 10 caratteri
                 
            entity.Property(e => e.Phone).HasMaxLength(25);         // Limita il numero di telefono a 25 caratteri
            entity.Property(e => e.Role)
                .HasMaxLength(10);         // Limita il ruolo a 10 caratteri        // Imposta una lunghezza fissa
            entity.Property(e => e.Suffix).HasMaxLength(10);        // Limita il suffisso a 10 caratteri
            entity.Property(e => e.Title).HasMaxLength(8);          // Limita il titolo a 8 caratteri
            entity.Property(e => e.Rowguid).IsRequired();           // Imposta Rowguid come obbligatorio
        });

        // Metodo parziale per ulteriori configurazioni personalizzate
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
