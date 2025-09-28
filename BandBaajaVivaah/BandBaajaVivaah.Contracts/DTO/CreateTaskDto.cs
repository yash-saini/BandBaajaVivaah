namespace BandBaajaVivaah.Contracts.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; }
    public int WeddingID { get; set; }
}