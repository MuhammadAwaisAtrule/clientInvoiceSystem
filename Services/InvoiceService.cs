using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Client_Invoice_System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
            try
            {
                var client = await _context.Clients.FindAsync(clientId);
                if (client == null)
                    throw new Exception("Client not found.");

                byte[] invoicePdf = await GenerateInvoicePdfAsync(clientId);
                string fileName = $"Invoice_{clientId}.pdf";

                await _emailService.SendInvoiceEmailAsync(client.Email, invoicePdf, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending invoice: {ex.Message}");
                throw;
            }
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int clientId)
        {
            try
            {
                var client = await _context.Clients
                    .Where(c => c.ClientId == clientId)
                    .Include(c => c.Resources)
                    .ThenInclude(r => r.Employee)
                    .FirstOrDefaultAsync();

                if (client == null)
                    throw new Exception("Client not found!");

                var paymentProfile = await _context.PaymentProfiles.FirstOrDefaultAsync();
                if (paymentProfile == null)
                    throw new Exception("Payment profile not found!");

                decimal totalAmount = client.Resources.Sum(r => r.ConsumedTotalHours * r.Employee.HourlyRate);

                using (MemoryStream ms = new MemoryStream())
                {
                    QuestPDF.Settings.License = LicenseType.Community;

                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(30);

                            // ---- HEADER (One Row, Four Columns) ----
                            page.Header().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().AlignLeft().Text("Atrule Technologies").Bold();
                                table.Cell().AlignLeft().Text("2nd Floor, Khawar Center, SP Chowk, Multan Pakistan");
                                table.Cell().AlignLeft().Text($"Client: {client.Name}\n{client.Address}\nEmail: {client.Email}\nPhone: {client.PhoneNumber}");
                                table.Cell().AlignRight().Text($"Invoice No: INV/{DateTime.Now.Year}/000{clientId}\nDate: {DateTime.Now:MM/dd/yyyy}").Bold();
                            });

                            page.Content().Column(col =>
                            {
                                col.Item().AlignCenter().Text("INVOICE").FontSize(22).Bold();
                                col.Item().PaddingBottom(10).LineHorizontal(1);

                                // ---- PAYMENT INSTRUCTIONS ----
                                col.Item().Container().PaddingBottom(5).Text("Payment Instructions (Wire Transfer)").Bold();
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(120);
                                        columns.RelativeColumn();
                                    });

                                    void AddPaymentRow(string label, string value)
                                    {
                                        table.Cell().Padding(2).Text(label).Bold();
                                        table.Cell().Padding(2).Text(value);
                                    }

                                    AddPaymentRow("Currency:", "GBP");
                                    AddPaymentRow("Bank Name:", "Habib Bank");
                                    AddPaymentRow("Swift Code:", "HABBPKKA");
                                    AddPaymentRow("Account Title:", paymentProfile.AccountTitle);
                                    AddPaymentRow("IBAN:", paymentProfile.IBANNumber);
                                    AddPaymentRow("Branch Address:", "HBL IBB SADDAR BAZAR MULTAN");
                                    AddPaymentRow("Beneficiary Address:", "2nd Floor, Khawar Centre, Multan Cantt");
                                });

                                col.Item().PaddingTop(10);

                                // ---- SERVICE DETAILS TABLE ----
                                col.Item().Container().PaddingTop(5).Text("Service Details").Bold();
                                col.Item().Container().PaddingTop(5).LineHorizontal(1);

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(); // Description
                                        columns.ConstantColumn(50); // Quantity
                                        columns.ConstantColumn(70); // Rate
                                        columns.ConstantColumn(100); // Subtotal
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Border(1).Padding(2).Text("Description").Bold();
                                        header.Cell().Border(1).Padding(2).AlignCenter().Text("Qty").Bold();
                                        header.Cell().Border(1).Padding(2).AlignCenter().Text("Rate ($)").Bold();
                                        header.Cell().Border(1).Padding(2).AlignCenter().Text("Subtotal ($)").Bold();
                                    });

                                    foreach (var resource in client.Resources)
                                    {
                                        table.Cell().Border(1).Padding(2).Text($"{resource.ResourceName} - {resource.Employee.Designation} - Monthly Contract - {DateTime.Now:MMMM yyyy}");
                                        table.Cell().Border(1).Padding(2).AlignCenter().Text(resource.ConsumedTotalHours.ToString());
                                        table.Cell().Border(1).Padding(2).AlignCenter().Text($"{resource.Employee.HourlyRate:F2}");
                                        table.Cell().Border(1).Padding(2).AlignCenter().Text($"{(resource.ConsumedTotalHours * resource.Employee.HourlyRate):F2}");
                                    }
                                });

                                col.Item().PaddingTop(5);

                                // ---- TOTAL AMOUNT & DUE DATE ----
                                col.Item().AlignRight().Column(rightCol =>
                                {
                                    rightCol.Item().Text($"Total Amount: USD {totalAmount:F2}").Bold();
                                    rightCol.Item().Text($"Total Due By: {DateTime.Now.AddDays(5):MM/dd/yyyy}");
                                });
                            });

                            // ---- FOOTER ----
                            page.Footer().AlignCenter().Text("Email: suleman@atrule.com | Web: atrule.com | Phone: +92-313-6120356").FontSize(10);
                        });
                    }).GeneratePdf(ms);

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating invoice PDF: {ex.Message}");
                throw;
            }
        }


    }
}
