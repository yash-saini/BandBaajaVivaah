using System.ComponentModel.DataAnnotations;

namespace BandBaajaVivaah.Contracts.DTO
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
