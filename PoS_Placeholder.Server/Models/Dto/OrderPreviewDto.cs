namespace PoS_Placeholder.Server.Models.Dto;

public class OrderPreviewDto
{
    public decimal? Tip { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxesTotal { get; set; }
    public decimal DiscountsTotal { get; set; }
    public decimal Total { get; set; }
    public List<TaxDto> Taxes { get; set; }
    
}