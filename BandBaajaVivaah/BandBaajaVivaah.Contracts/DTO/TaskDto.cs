namespace BandBaajaVivaah.Contracts.DTOs;

public class TaskDto
{
    public int TaskID { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; }
}