namespace BandBaajaVivaah.Contracts.DTO
{
    public class ExpenseDto
    {
        public int ExpenseID { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
