namespace BandBaajaVivaah.Contracts.DTOs;

public class WeddingDto
{
    public int WeddingID { get; set; }
    public string WeddingName { get; set; }
    public DateTime WeddingDate { get; set; }
    public decimal TotalBudget { get; set; }
    public int OwnerUserId { get; set; }
}