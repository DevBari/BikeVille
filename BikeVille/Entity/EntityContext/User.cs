using System;
using System.Collections.Generic;

namespace BikeVille.Entity.EntityContext;

public partial class User
{
    public int UserId { get; set; }

    public string? Title { get; set; }

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public string? Suffix { get; set; }

    public string? EmailAddress { get; set; }

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string? Role { get; set; }

    public Guid? Rowguid { get; set; }
}
