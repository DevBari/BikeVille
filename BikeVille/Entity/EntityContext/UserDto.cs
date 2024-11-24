namespace BikeVille.Entity.EntityContext
{
    public class UserDto
    {
      

        public string? Title { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;

        public string? Suffix { get; set; }

        public string? EmailAddress { get; set; }

        public string? Phone { get; set; }

        public string Password { get; set; } = null!;

        public string? Role { get; set; }

        
    }
}
