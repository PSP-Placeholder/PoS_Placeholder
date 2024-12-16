using System.ComponentModel.DataAnnotations;

namespace PoS_Placeholder.Server.Models;

public class Business
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(30)]
    [Phone]
    public string Phone { get; set; }
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Street { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string City { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Region { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Country { get; set; }
    
    // Navigation properties
    public ICollection<User> Users { get; set; }
    public ICollection<Product> Products { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<Giftcard> Giftcards { get; set; }
}