namespace BandBaajaVivaah.Contracts.DTO
{
    public class CreateExpenseDto
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime PaymentDate { get; set; }
        public int WeddingID { get; set; }
    }
}
