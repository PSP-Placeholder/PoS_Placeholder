using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PoS_Placeholder.Server.Models.Enum;

namespace PoS_Placeholder.Server.Models;

public class Order
{
    [Key]
    public int Id { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Tip { get; set; }

    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    [Required]
    public int BusinessId { get; set; }
    
    [ForeignKey("BusinessId")]
    public Business Business { get; set; }
    
    // Navigation Properties
    public ICollection<ProductArchive> Products { get; set; }
    public ICollection<TaxArchive> Taxes { get; set; }
    public ICollection<DiscountArchive> Discounts { get; set; }
    public ICollection<PaymentArchive> Payments { get; set; }
}