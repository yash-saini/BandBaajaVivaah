using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BandBaajaVivaah.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("no-reply@bandbaajavivaah.com", "Band Baaja Vivaah");
            var to = new EmailAddress(toEmail);
            var subject = "Password Reset Request";
            var plainTextContent = $"Your password reset token is: {resetToken}";
            // In a real app, you'd send a link to your frontend app's reset page
            var htmlContent = $"<strong>Your password reset token is:</strong> {resetToken}";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);
        }
    }
}
