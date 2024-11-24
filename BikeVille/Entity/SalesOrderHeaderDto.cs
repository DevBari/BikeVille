using BikeVille.Entity.EntityContext;


namespace BikeVille.Entity
{
    public class SalesOrderHeaderDto
    {
        public SalesOrderHeader SalesOrderHeader { get; set; }
        public User user { get; set; }

        public string CompanyName { get; set; }
    }
}
