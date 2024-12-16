using System.ComponentModel.DataAnnotations;

namespace PoS_Placeholder.Server.Models.Dto;

public class CreateGiftcardDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Balance amount must be greater than zero.")]
    public decimal BalanceAmount { get; set; }
}