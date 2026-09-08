using RamsElec.Api.Services;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;
using Xunit;

namespace RamsElec.Api.Tests.Services;

public class InvoiceServiceTests
{
    [Fact]
    public async Task CreateInvoice_CalculatesTotalFromLineItems()
    {
        var db = TestDbContextFactory.Create();
        var service = new InvoiceService(db);

        db.Customers.Add(new Customer { Id = "c1", Name = "Test Customer" });
        await db.SaveChangesAsync();

        var dto = new CreateInvoiceDto
        {
            CustomerId = "c1",
            DueDate = DateTime.UtcNow.AddDays(14),
            LineItems =
            [
                new CreateLineItemDto { Description = "Labour", Quantity = 2, UnitPrice = 500, Category = "labour" },
                new CreateLineItemDto { Description = "Materials", Quantity = 1, UnitPrice = 150, Category = "materials" }
            ]
        };

        var invoice = await service.CreateInvoice(dto, "test@user.com");

        Assert.Equal(1150m, invoice.Subtotal);
        Assert.Equal(1150m, invoice.Total);
        Assert.StartsWith("INV-", invoice.InvoiceNumber);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Equal(2, invoice.LineItems.Count);
    }

    [Fact]
    public async Task MarkAsSent_UpdatesStatusAndUrl()
    {
        var db = TestDbContextFactory.Create();
        var service = new InvoiceService(db);

        db.Customers.Add(new Customer { Id = "c1", Name = "Test" });
        await db.SaveChangesAsync();

        var invoice = await service.CreateInvoice(new CreateInvoiceDto
        {
            CustomerId = "c1",
            DueDate = DateTime.UtcNow,
            LineItems = [new CreateLineItemDto { Description = "Service", Quantity = 1, UnitPrice = 100 }]
        }, "test");

        var updated = await service.MarkAsSent(invoice.Id, DeliveryChannel.Sms, "https://pdf.url");

        Assert.NotNull(updated);
        Assert.Equal(InvoiceStatus.Sent, updated!.Status);
        Assert.Equal("https://pdf.url", updated.PdfUrl);
        Assert.Equal(DeliveryChannel.Sms, updated.SentVia);
    }
}
