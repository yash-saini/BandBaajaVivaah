namespace BandBaajaVivaah.Contracts.DTOs;

public class CreateGuestDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Side { get; set; }
    public string RSVPStatus { get; set; }
    public int WeddingID { get; set; }
}