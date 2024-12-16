using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoS_Placeholder.Server.Models;

public class Giftcard
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
        
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
        
    public int BusinessId { get; set; }
    [ForeignKey("BusinessId")]
        
    public Business Business { get; set; }
}