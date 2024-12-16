using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoS_Placeholder.Server.Models;

public class DiscountArchive
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ProductFullName { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    public bool IsPercentage { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [ForeignKey("OrderId")]
    public Order Order { get; set; }
}