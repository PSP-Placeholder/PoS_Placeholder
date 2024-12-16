using PoS_Placeholder.Server.Models.Enum;

namespace PoS_Placeholder.Server.Models.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } // IdentityUser's primary key

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public AvailabilityStatus AvailabilityStatus { get; set; }

        public int BusinessId { get; set; } // Avoid embedding the full Business object
    }
}
