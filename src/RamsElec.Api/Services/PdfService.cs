using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Services;

public class PdfService
{
    private readonly CompanyInfo _company = new();

    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            // Navy header band
            column.Item().Background("#1B2A4A").Padding(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INVOICE").Bold().FontSize(28).FontColor(Colors.White);
                    col.Item().PaddingTop(8).Text(_company.Name).FontSize(11).FontColor(Colors.White);
                    col.Item().Text($"REG: {_company.Registration}").FontSize(8).FontColor("#B0BEC5");
                    col.Item().Text($"TAX NO: {_company.TaxNumber}").FontSize(8).FontColor("#B0BEC5");
                    col.Item().Text(_company.Address).FontSize(8).FontColor("#B0BEC5");
                    col.Item().Text($"Email: {_company.Email}").FontSize(8).FontColor("#B0BEC5");
                    col.Item().Text($"Contact: {_company.Phone}").FontSize(8).FontColor("#B0BEC5");
                });

                row.ConstantItem(100).AlignRight().AlignMiddle()
                    .Text("R@E").Bold().FontSize(32).FontColor("#FFC107");
            });

            // Invoice meta row
            column.Item().PaddingTop(15).Row(row =>
            {
                // Bill To
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILL TO").Bold().FontSize(9).FontColor("#1B2A4A");
                    col.Item().PaddingTop(4).Text(invoice.Customer?.Name ?? "—").Bold().FontSize(11);
                    col.Item().Text(invoice.Customer?.Address ?? "");
                    if (!string.IsNullOrEmpty(invoice.Customer?.Phone))
                        col.Item().Text($"Tel: {invoice.Customer.Phone}");
                    if (!string.IsNullOrEmpty(invoice.Customer?.Email))
                        col.Item().Text($"Email: {invoice.Customer.Email}");
                });

                // Invoice details
                row.ConstantItem(180).AlignRight().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Invoice #").Bold().FontSize(9).FontColor("#1B2A4A");
                        r.ConstantItem(100).AlignRight().Text(invoice.InvoiceNumber).Bold();
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Date").Bold().FontSize(9).FontColor("#1B2A4A");
                        r.ConstantItem(100).AlignRight().Text(invoice.CreatedAt.ToString("dd/MM/yyyy"));
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Due Date").Bold().FontSize(9).FontColor("#1B2A4A");
                        r.ConstantItem(100).AlignRight().Text(invoice.DueDate.ToString("dd/MM/yyyy"));
                    });
                });
            });

            // Job description
            if (invoice.Job != null)
            {
                column.Item().PaddingTop(10)
                    .Background("#F5F5F5").Padding(8)
                    .Text(text =>
                    {
                        text.Span("JOB: ").Bold().FontSize(9);
                        text.Span(invoice.Job.Description ?? invoice.Job.Title);
                    });
            }
        });
    }

    private void ComposeContent(IContainer container, Invoice invoice)
    {
        container.PaddingVertical(15).Column(column =>
        {
            // Line items table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(45);    // QTY
                    columns.RelativeColumn();       // Description
                    columns.ConstantColumn(100);    // Unit Price
                    columns.ConstantColumn(100);    // Amount
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Background("#1B2A4A").Padding(6)
                        .Text("QTY").Bold().FontSize(9).FontColor(Colors.White);
                    header.Cell().Background("#1B2A4A").Padding(6)
                        .Text("DESCRIPTION").Bold().FontSize(9).FontColor(Colors.White);
                    header.Cell().Background("#1B2A4A").Padding(6).AlignRight()
                        .Text("UNIT PRICE").Bold().FontSize(9).FontColor(Colors.White);
                    header.Cell().Background("#1B2A4A").Padding(6).AlignRight()
                        .Text("AMOUNT").Bold().FontSize(9).FontColor(Colors.White);
                });

                // Data rows
                var isAlternate = false;
                foreach (var item in invoice.LineItems.OrderBy(li => li.SortOrder))
                {
                    var bgColor = isAlternate ? "#F8F9FA" : "#FFFFFF";

                    table.Cell().Background(bgColor).Padding(6)
                        .Text(item.Quantity.ToString("G")).FontSize(10);
                    table.Cell().Background(bgColor).Padding(6)
                        .Text(item.Description).FontSize(10);
                    table.Cell().Background(bgColor).Padding(6).AlignRight()
                        .Text(item.UnitPrice.ToString("N2")).FontSize(10);
                    table.Cell().Background(bgColor).Padding(6).AlignRight()
                        .Text(item.Total.ToString("N2")).FontSize(10);

                    isAlternate = !isAlternate;
                }
            });

            // Totals
            column.Item().PaddingTop(10).AlignRight().Width(200).Column(totals =>
            {
                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").FontSize(10);
                    row.ConstantItem(100).AlignRight().Text($"R{invoice.Subtotal:N2}").FontSize(10);
                });

                if (_company.IsVatRegistered)
                {
                    totals.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"VAT ({_company.VatRate:P0}):").FontSize(10);
                        row.ConstantItem(100).AlignRight().Text($"R{invoice.VatAmount:N2}").FontSize(10);
                    });
                }

                totals.Item().PaddingTop(4).BorderTop(1).BorderColor("#1B2A4A").PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text("TOTAL:").Bold().FontSize(12);
                    row.ConstantItem(100).AlignRight().Text($"R{invoice.Total:N2}").Bold().FontSize(12);
                });
            });

            // Divider
            column.Item().PaddingVertical(15).LineHorizontal(2).LineColor("#C62828");

            // Payment details
            column.Item().Column(payment =>
            {
                payment.Item().Text("PAYMENT METHODS").Bold().FontSize(10).FontColor("#1B2A4A");
                payment.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(120);
                        cols.RelativeColumn();
                    });

                    void AddRow(string label, string value)
                    {
                        table.Cell().Padding(3).Text(label).Bold().FontSize(9);
                        table.Cell().Padding(3).Text(value).FontSize(9);
                    }

                    AddRow("Bank:", _company.BankName);
                    AddRow("Account Name:", _company.AccountName);
                    AddRow("Account Number:", _company.AccountNumber);
                    AddRow("Branch Code:", _company.BranchCode);
                    AddRow("Reference:", invoice.InvoiceNumber);
                });

                payment.Item().PaddingTop(6)
                    .Background("#FFF8E1").Padding(8)
                    .Text(text =>
                    {
                        text.Span("Please use ").FontSize(9);
                        text.Span(invoice.InvoiceNumber).Bold().FontSize(9);
                        text.Span(" as your EFT payment reference.").FontSize(9);
                    });
            });

            // Terms
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                column.Item().PaddingTop(10).Column(terms =>
                {
                    terms.Item().Text("TERMS & CONDITIONS").Bold().FontSize(9).FontColor("#1B2A4A");
                    terms.Item().PaddingTop(4).Text(invoice.Notes).FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().PaddingBottom(5)
                .Text("Thank you for your business").Italic().FontSize(12).FontColor("#1B2A4A");
            col.Item().AlignCenter()
                .Text($"{_company.Phone} | {_company.Email}").FontSize(8).FontColor(Colors.Grey.Medium);
            col.Item().AlignCenter().Text(text =>
            {
                text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
