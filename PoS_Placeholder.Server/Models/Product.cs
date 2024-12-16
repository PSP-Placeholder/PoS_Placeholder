using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PoS_Placeholder.Server.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ItemGroup { get; set; }
    
    [Required]
    public int BusinessId { get; set; }
    
    [ForeignKey("BusinessId")]
    public Business Business { get; set; }
    
    // Navigation properties
    [JsonIgnore]
    public ICollection<ProductVariation> ProductVariations { get; set; }
}