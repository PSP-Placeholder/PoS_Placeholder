using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PoS_Placeholder.Server.Models.Enum;

namespace PoS_Placeholder.Server.Models;

public class PaymentArchive
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public PaymentMethod Method { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidPrice { get; set; }
    
    public string? PaymentIntentId { get; set; }
    
    public string? GiftCardId { get; set; }

    [Required]
    public int OrderId { get; set; }
    [ForeignKey("OrderId")]
    public Order Order { get; set; }
}