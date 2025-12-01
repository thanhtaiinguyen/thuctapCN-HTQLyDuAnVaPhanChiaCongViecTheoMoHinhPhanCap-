using Microsoft.AspNetCore.Identity.UI.Services;

namespace thuctapCN.Services
{
    /// <summary>
    /// Email sender cho môi trường development - ghi log thay vì gửi email thật
    /// </summary>
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("=================================================");
            _logger.LogInformation("EMAIL SENT (Development Mode)");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Message: {Message}", htmlMessage);
            _logger.LogInformation("=================================================");

            // Trong môi trường development, chỉ log ra console
            Console.WriteLine("\n=================================================");
            Console.WriteLine($"📧 EMAIL ĐÃ GỬI (Development Mode)");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");
            Console.WriteLine("=================================================\n");

            return Task.CompletedTask;
        }
    }
}
