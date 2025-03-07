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
            try
            {
                var emailSettings = _config.GetSection("EmailSettings");

                if (emailSettings == null)
                    throw new Exception("Email settings not found in configuration.");

                string senderEmail = emailSettings["SenderEmail"];
                string smtpServer = emailSettings["SmtpServer"];
                string smtpPort = emailSettings["SmtpPort"];
                string senderPassword = emailSettings["SenderPassword"];

                if (string.IsNullOrEmpty(senderEmail) ||
                    string.IsNullOrEmpty(smtpServer) ||
                    string.IsNullOrEmpty(smtpPort) ||
                    string.IsNullOrEmpty(senderPassword))
                {
                    throw new Exception("Missing SMTP configuration details.");
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Your Company", senderEmail));
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
                try
                {
                    Console.WriteLine("🔄 Connecting to SMTP server...");

                    // ✅ Enable TLS and use the correct port
                    await client.ConnectAsync(smtpServer, int.Parse(smtpPort), MailKit.Security.SecureSocketOptions.StartTls);

                    Console.WriteLine("🔑 Authenticating SMTP credentials...");
                    await client.AuthenticateAsync(senderEmail, senderPassword);

                    Console.WriteLine("📧 Sending email...");
                    await client.SendAsync(message);

                    Console.WriteLine("✅ Email sent successfully to " + recipientEmail);
                }
                catch (SmtpCommandException smtpEx)
                {
                    Console.WriteLine($"❌ SMTP Command Error: {smtpEx.Message} (Code: {smtpEx.StatusCode})");
                    throw new Exception("SMTP command error. Check authentication and recipient address.");
                }
                catch (SmtpProtocolException protocolEx)
                {
                    Console.WriteLine($"❌ SMTP Protocol Error: {protocolEx.Message}");
                    throw new Exception("SMTP protocol error. Check if the mail server is reachable.");
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"❌ Network/Connection Error: {ioEx.Message}");
                    throw new Exception("Connection was forcibly closed by the SMTP server. Check firewall and SSL settings.");
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    Console.WriteLine("🔌 Disconnected from SMTP server.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email Sending Error: {ex.Message}");
                throw;
            }
        }


    }
}
