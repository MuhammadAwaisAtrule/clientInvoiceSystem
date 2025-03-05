using Microsoft.EntityFrameworkCore;
using Client_Invoice_System.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Client_Invoice_System.Services
{
    public class InvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public InvoiceService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendInvoiceToClientAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                throw new System.Exception("Client not found.");

            byte[] invoicePdf = await GenerateInvoicePdfAsync(clientId);
            string fileName = $"Invoice_{clientId}.pdf";

            await _emailService.SendInvoiceEmailAsync(client.Email, invoicePdf, fileName);
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int clientId)
        {
            var client = await _context.Clients
                .Where(c => c.ClientId == clientId)
                .Include(c => c.Resources)
                .ThenInclude(r => r.Employee)
                .FirstOrDefaultAsync();

            if (client == null)
                throw new System.Exception("Client not found!");

            var paymentProfile = await _context.PaymentProfiles.FirstOrDefaultAsync();
            if (paymentProfile == null)
                throw new System.Exception("Payment profile not found!");

            decimal totalAmount = client.Resources.Sum(r => r.ConsumedTotalHours * r.Employee.HourlyRate);

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Invoice Header
                document.Add(new Paragraph("INVOICE")
                    .SetFont(boldFont).SetFontSize(20));
                document.Add(new Paragraph($"Invoice Number: INV/{System.DateTime.Now.Year}/000{clientId}").SetFont(boldFont));
                document.Add(new Paragraph($"Date: {System.DateTime.Now:MM/dd/yyyy}").SetFont(boldFont));

                // From Section
                document.Add(new Paragraph("From: Atrule Technologies\n2nd Floor, Khawar Center, SP Chowk, Multan Pakistan"));

                // To Section
                document.Add(new Paragraph($"To: {client.Name}\n{client.Address}"));

                // Payment Instructions
                document.Add(new Paragraph("\nPayment Instructions (Wire Transfer to Pakistan Bank)")
                    .SetFont(boldFont));
                document.Add(new Paragraph($"Bank Name: {paymentProfile.AccountTitle}"));
                document.Add(new Paragraph($"Swift Code: HABBPKKA"));
                document.Add(new Paragraph($"IBAN: {paymentProfile.IBANNumber}"));
                document.Add(new Paragraph($"Branch Address: HBL IBB SADDAR BAZAR MULTAN"));
                document.Add(new Paragraph($"Beneficiary Address: 2nd Floor, Khawar Centre Near SP Chowk, Multan Cantt"));

                // Service Details Table
                Table table = new Table(3);
                table.AddHeaderCell("Description");
                table.AddHeaderCell("Quantity");
                table.AddHeaderCell("Subtotal");

                foreach (var resource in client.Resources)
                {
                    table.AddCell($"{resource.ResourceName} - {resource.Employee.Designation}");
                    table.AddCell(resource.ConsumedTotalHours.ToString());
                    table.AddCell((resource.ConsumedTotalHours * resource.Employee.HourlyRate).ToString("C"));
                }
                document.Add(table);

                // Total Calculation Section
                document.Add(new Paragraph($"\nTotal Amount: {totalAmount:C}")
                    .SetFont(boldFont));
                document.Add(new Paragraph($"Total Due By: {System.DateTime.Now.AddDays(5):MM/dd/yyyy}"));

                document.Close();
                return ms.ToArray();
            }
        }
    }
}
