namespace PoS_Placeholder.Server.Models.DTOs
{
    public class BusinessDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        // Simplified navigation property, avoiding circular references
        public ICollection<int> UserIds { get; set; }
    }
}
