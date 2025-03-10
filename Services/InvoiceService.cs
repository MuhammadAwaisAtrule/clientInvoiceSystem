using Microsoft.EntityFrameworkCore;
using Client_Invoice_System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using Client_Invoice_System.Models;

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
        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            try
            {
                return await _context.Invoices
                    .Include(i => i.Client)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching invoices: {ex.Message}");
                return new List<Invoice>(); 
            }
        }

        public async Task MarkInvoiceAsPaidAsync(int invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    Console.WriteLine($"⚠️ Invoice with ID {invoiceId} not found.");
                    return;
                }

                if (invoice.IsPaid)
                {
                    Console.WriteLine($"✅ Invoice {invoiceId} is already marked as Paid.");
                    return;
                }

                invoice.IsPaid = true;
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Invoice {invoiceId} marked as Paid.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error marking invoice as paid: {ex.Message}");
            }
        }
        public async Task<int> SaveInvoiceAsync(int clientId)
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

                decimal totalAmount = client.Resources.Sum(r => r.ConsumedTotalHours * r.Employee.HourlyRate);

                var invoice = new Invoice
                {
                    ClientId = clientId,
                    InvoiceDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Currency = client.Currency ?? "USD",
                    IsPaid = false
                };

                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Invoice {invoice.InvoiceId} saved successfully.");

                return invoice.InvoiceId; // Return ID to track the saved invoice
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving invoice: {ex.Message}");
                throw;
            }
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
                Console.WriteLine($"❌ Error sending invoice: {ex.Message}");
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

                // Determine culture based on client's currency
                CultureInfo culture;
                string currencySymbol = "$"; // Default to USD
                if (!string.IsNullOrEmpty(client.Currency))
                {
                    switch (client.Currency.ToUpper())
                    {
                        case "PKR":
                            culture = new CultureInfo("ur-PK");
                            currencySymbol = "₨";
                            break;
                        case "GBP":
                            culture = new CultureInfo("en-GB");
                            currencySymbol = "£";
                            break;
                        case "CAD":
                            culture = new CultureInfo("en-CA");
                            currencySymbol = "C$";
                            break;
                        default:
                            culture = new CultureInfo("en-US");
                            currencySymbol = "$";
                            break;
                    }
                }
                else
                {
                    culture = new CultureInfo("en-US");
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    QuestPDF.Settings.License = LicenseType.Community;

                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(30);

                            // ---- HEADER ----
                            page.Header().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(120);
                                    columns.ConstantColumn(180);
                                    columns.ConstantColumn(150);
                                });

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("Atrule Technologies").Bold();
                                table.Cell().Border(1).Padding(2).AlignLeft().Text("From\nAtrule Technologies,\n2nd Floor, Khawar Center, SP Chowk, Multan Pakistan");
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"To\n{client.Name}\n{client.Address}\nEmail: {client.Email}\nPhone: {client.PhoneNumber}");
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"Invoice No: INV/{DateTime.Now.Year}/000{clientId}\nDate: {DateTime.Now:MM/dd/yyyy}").Bold();
                            });

                            page.Content().Column(col =>
                            {
                                col.Item().PaddingTop(20);

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

                                    AddPaymentRow("Currency:", client.Currency ?? "USD");
                                    AddPaymentRow("Bank Name:", "Habib Bank");
                                    AddPaymentRow("Swift Code:", "HABBPKKA");
                                    AddPaymentRow("Account Title:", paymentProfile.AccountTitle);
                                    AddPaymentRow("IBAN:", paymentProfile.IBANNumber);
                                    AddPaymentRow("Branch Address:", "HBL IBB SADDAR BAZAR MULTAN");
                                    AddPaymentRow("Beneficiary Address:", "2nd Floor, Khawar Centre, Multan Cantt");
                                });

                                col.Item().PaddingTop(10);

                                // ---- SERVICE DETAILS TABLE ----
                                col.Item().Container().PaddingTop(5);

                                col.Item().PaddingTop(5);
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(60);
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(120);
                                    });

                                    table.Header(header =>
                                    {
                                        string headerColor = "#2F4F4F";

                                        header.Cell().Background(Color.FromHex(headerColor)).Padding(5)
                                            .Text(text => text.Span("Description").FontColor(Colors.White).Bold());

                                        header.Cell().Background(Color.FromHex(headerColor)).Padding(5)
                                            .Text(text => text.Span("Quantity").FontColor(Colors.White).Bold());

                                        header.Cell().Background(Color.FromHex(headerColor)).Padding(5)
                                            .Text(text => text.Span($"Rate ({currencySymbol})").FontColor(Colors.White).Bold());

                                        header.Cell().Background(Color.FromHex(headerColor)).Padding(5)
                                            .Text(text => text.Span($"Subtotal ({currencySymbol})").FontColor(Colors.White).Bold());
                                    });

                                    foreach (var resource in client.Resources)
                                    {
                                        table.Cell().ColumnSpan(4).Border(1).Padding(5).Text($"{resource.ResourceName} - {resource.Employee.Designation} - Monthly Contract - {DateTime.Now:MMMM yyyy}");

                                        table.Cell().ColumnSpan(1).Border(1).Padding(5).Text($"Calculation\nAmount in {currencySymbol}: {resource.ConsumedTotalHours} Hours X {resource.Employee.HourlyRate.ToString("C2", culture)} = {(resource.ConsumedTotalHours * resource.Employee.HourlyRate).ToString("C2", culture)}").Italic(); ;
                                       
                                        table.Cell().Border(1).Padding(5).AlignCenter().Text("1");
                                        table.Cell().Border(1).Padding(5).AlignCenter().Text($"{resource.Employee.HourlyRate.ToString("C2", culture)}");
                                        table.Cell().Border(1).Padding(5).AlignCenter().Text($"{(resource.ConsumedTotalHours * resource.Employee.HourlyRate).ToString("C2", culture)}");
                                    }

                                    // Last Section: Software Consultancy Services & Total Amount
                                    table.Cell().ColumnSpan(1).Border(1).Padding(5).Text("Software Consultancy Services").Bold();
                                    table.Cell().ColumnSpan(3).Border(1).Table(subTable =>
                                    {
                                        subTable.ColumnsDefinition(subCols =>
                                        {
                                            subCols.RelativeColumn();
                                            subCols.ConstantColumn(100);
                                        });

                                        subTable.Cell().Padding(5).Text("Total").Bold();
                                        subTable.Cell().Padding(5).AlignRight().Text($"{totalAmount.ToString("C2", culture)}").Bold();

                                        subTable.Cell().Padding(5).Text("Total Due By").Bold();
                                        subTable.Cell().Padding(5).AlignRight().Text($"{DateTime.Now.AddDays(5):MM/dd/yyyy}").Bold();
                                    });
                                });

                                col.Item().PaddingTop(5);
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
