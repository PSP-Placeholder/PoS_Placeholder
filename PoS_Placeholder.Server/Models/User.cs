using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using PoS_Placeholder.Server.Models.Enum;

namespace PoS_Placeholder.Server.Models;

public class User : IdentityUser
{
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string LastName { get; set; }
    
    [Required]
    public AvailabilityStatus AvailabilityStatus { get; set; }
    
    [Required]
    public int BusinessId { get; set; }

    [ForeignKey("BusinessId")]
    public Business Business { get; set; }

    // Navigation Properties
    public ICollection<Order> Orders { get; set; }

    public virtual ICollection<UserWorkTime> UserWorkTimes { get; set; }

}