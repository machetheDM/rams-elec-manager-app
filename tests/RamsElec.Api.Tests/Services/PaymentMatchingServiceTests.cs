using Microsoft.Extensions.Logging.Abstractions;
using RamsElec.Api.Services;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;
using Xunit;

namespace RamsElec.Api.Tests.Services;

public class PaymentMatchingServiceTests
{
    [Fact]
    public async Task EvaluateAsync_MatchesByInvoiceNumber()
    {
        var db = TestDbContextFactory.Create();
        var invoiceService = new InvoiceService(db);
        var quoteService = new QuoteService(db);
        var matcher = new PaymentMatchingService(db, NullLogger<PaymentMatchingService>.Instance, quoteService);

        db.Customers.Add(new Customer { Id = "c1", Name = "Test" });
        await db.SaveChangesAsync();

        var invoice = await invoiceService.CreateInvoice(new Shared.DTOs.CreateInvoiceDto
        {
            CustomerId = "c1",
            DueDate = DateTime.UtcNow,
            LineItems = [new Shared.DTOs.CreateLineItemDto { Description = "Work", Quantity = 1, UnitPrice = 500 }]
        }, "test");
        await invoiceService.MarkAsSent(invoice.Id, DeliveryChannel.Email);

        var notification = new BankPaymentNotification
        {
            Id = "bn1",
            SourceEmailId = "em1",
            Reference = invoice.InvoiceNumber,
            Amount = 500,
            BankName = "FNB"
        };

        var match = await matcher.EvaluateAsync(notification);

        Assert.True(match.Confidence >= 0.9m);
        Assert.Equal("auto_processed", match.Status);
        Assert.Equal(invoice.Id, match.InvoiceId);

        var updated = await invoiceService.GetInvoice(invoice.Id);
        Assert.Equal(InvoiceStatus.Paid, updated!.Status);
    }

    [Fact]
    public async Task EvaluateAsync_QuotePaymentReference_ConvertsToInvoice()
    {
        var db = TestDbContextFactory.Create();
        var quoteService = new QuoteService(db);
        var matcher = new PaymentMatchingService(db, NullLogger<PaymentMatchingService>.Instance, quoteService);

        db.Customers.Add(new Customer { Id = "c1", Name = "Test" });
        await db.SaveChangesAsync();

        var quote = await quoteService.CreateQuoteAsync(new Shared.DTOs.CreateQuoteDto
        {
            CustomerId = "c1",
            ExpiryDate = DateTime.UtcNow,
            LineItems = [new Shared.DTOs.CreateQuoteLineItemDto { Description = "Work", Quantity = 1, UnitPrice = 250 }]
        }, "test");

        quote = await quoteService.ApproveQuoteAsync(quote.Id) ?? quote;

        var notification = new BankPaymentNotification
        {
            Id = "bn2",
            SourceEmailId = "em2",
            Reference = quote.PaymentReference!,
            Amount = 250,
            BankName = "FNB"
        };

        var match = await matcher.EvaluateAsync(notification);

        Assert.Equal("auto_processed", match.Status);

        var convertedQuote = await quoteService.GetQuoteAsync(quote.Id);
        Assert.Equal(QuoteStatus.ConvertedToInvoice, convertedQuote!.Status);
        Assert.NotNull(convertedQuote.ConvertedInvoiceId);
    }
}
