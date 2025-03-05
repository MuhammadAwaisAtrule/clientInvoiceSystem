using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Client_Invoice_System.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendInvoiceEmailAsync(string recipientEmail, byte[] invoicePdf, string fileName)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your Company", emailSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress("", recipientEmail));

            message.Subject = "Your Invoice";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = "Dear Client,\n\nPlease find your invoice attached.\n\nBest regards,\nYour Company"
            };

            // Attach the invoice PDF
            bodyBuilder.Attachments.Add(fileName, invoicePdf, new ContentType("application", "pdf"));

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), false);
            await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
