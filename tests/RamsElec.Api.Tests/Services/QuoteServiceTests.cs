using RamsElec.Api.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;
using Xunit;

namespace RamsElec.Api.Tests.Services;

public class QuoteServiceTests
{
    [Fact]
    public async Task CreateQuote_GeneratesNumberAndCalculatesTotal()
    {
        var db = TestDbContextFactory.Create();
        var service = new QuoteService(db);

        db.Customers.Add(new Customer { Id = "c1", Name = "Test" });
        await db.SaveChangesAsync();

        var dto = new CreateQuoteDto
        {
            CustomerId = "c1",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            LineItems = [new CreateQuoteLineItemDto { Description = "Work", Quantity = 2, UnitPrice = 300 }]
        };

        var quote = await service.CreateQuoteAsync(dto, "test");

        Assert.Equal(600m, quote.Total);
        Assert.StartsWith("Q-", quote.QuoteNumber);
        Assert.Equal(QuoteStatus.Draft, quote.Status);
    }

    [Fact]
    public async Task ApproveQuote_GeneratesPaymentReference()
    {
        var db = TestDbContextFactory.Create();
        var service = new QuoteService(db);

        db.Customers.Add(new Customer { Id = "c1", Name = "Test" });
        await db.SaveChangesAsync();

        var quote = await service.CreateQuoteAsync(new CreateQuoteDto
        {
            CustomerId = "c1",
            ExpiryDate = DateTime.UtcNow,
            LineItems = [new CreateQuoteLineItemDto { Description = "Work", Quantity = 1, UnitPrice = 100 }]
        }, "test");

        var approved = await service.ApproveQuoteAsync(quote.Id);

        Assert.NotNull(approved);
        Assert.Equal(QuoteStatus.Approved, approved!.Status);
        Assert.False(string.IsNullOrEmpty(approved.PaymentReference));
    }

    [Fact]
    public async Task RecordQuotePayment_And_ConvertToInvoice_CreatesPaidInvoice()
    {
        var db = TestDbContextFactory.Create();
        var service = new QuoteService(db);

        db.Customers.Add(new Customer { Id = "c1", Name = "Test" });
        await db.SaveChangesAsync();

        var quote = await service.CreateQuoteAsync(new CreateQuoteDto
        {
            CustomerId = "c1",
            ExpiryDate = DateTime.UtcNow,
            LineItems = [new CreateQuoteLineItemDto { Description = "Work", Quantity = 1, UnitPrice = 100 }]
        }, "test");

        quote = await service.ApproveQuoteAsync(quote.Id) ?? quote;
        await service.RecordQuotePaymentAsync(quote.Id, 100m, quote.PaymentReference!, "Client", null);

        var invoice = await service.ConvertToInvoiceAsync(quote.Id);

        Assert.NotNull(invoice);
        Assert.Equal(InvoiceStatus.Paid, invoice!.Status);
        Assert.Equal(100m, invoice.Total);
    }
}
