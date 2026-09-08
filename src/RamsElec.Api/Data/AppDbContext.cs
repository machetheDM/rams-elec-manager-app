using Microsoft.EntityFrameworkCore;
using RamsElec.Shared.Models;

namespace RamsElec.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<CompanyInfo> CompanyInfos => Set<CompanyInfo>();
    public DbSet<BankPaymentNotification> BankPaymentNotifications => Set<BankPaymentNotification>();
    public DbSet<PaymentMatch> PaymentMatches => Set<PaymentMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Bank payment notifications
        modelBuilder.Entity<BankPaymentNotification>(entity =>
        {
            entity.ToTable("bank_payment_notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceEmailId).HasColumnName("source_email_id");
            entity.Property(e => e.RawSubject).HasColumnName("raw_subject");
            entity.Property(e => e.RawBody).HasColumnName("raw_body");
            entity.Property(e => e.PayerName).HasColumnName("payer_name");
            entity.Property(e => e.Reference).HasColumnName("reference");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.BankName).HasColumnName("bank_name");
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(e => e.Reference);
            entity.HasIndex(e => e.Amount);
        });

        // Payment matches for manager review
        modelBuilder.Entity<PaymentMatch>(entity =>
        {
            entity.ToTable("payment_matches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.BankPaymentId).HasColumnName("bank_payment_id");
            entity.Property(e => e.Confidence).HasColumnName("confidence").HasColumnType("decimal(3,2)");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.MatchedBy).HasColumnName("matched_by");
            entity.Property(e => e.ReviewReason).HasColumnName("review_reason");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.Invoice)
                  .WithMany()
                  .HasForeignKey(e => e.InvoiceId);
            entity.HasOne(e => e.BankPayment)
                  .WithMany()
                  .HasForeignKey(e => e.BankPaymentId);
        });

        // Invoice
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).HasColumnName("invoice_number");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.Status).HasConversion<string>().HasColumnName("status");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(12,2)");
            entity.Property(e => e.VatRate).HasColumnName("vat_rate").HasColumnType("decimal(5,4)");
            entity.Property(e => e.VatAmount).HasColumnName("vat_amount").HasColumnType("decimal(12,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(12,2)");
            entity.Property(e => e.PdfUrl).HasColumnName("pdf_url");
            entity.Property(e => e.SentVia).HasConversion<string>().HasColumnName("sent_via");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.ViewedAt).HasColumnName("viewed_at");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.DueDate);

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Invoices)
                  .HasForeignKey(e => e.CustomerId);

            entity.HasOne(e => e.Job)
                  .WithMany(j => j.Invoices)
                  .HasForeignKey(e => e.JobId);

            entity.HasMany(e => e.LineItems)
                  .WithOne()
                  .HasForeignKey(li => li.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // InvoiceLineItem
        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.ToTable("invoice_line_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(12,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(12,2)");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.InvoiceId);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.Amount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Method).HasConversion<string>();
            entity.Property(e => e.MatchedConfidence).HasColumnName("matched_confidence").HasColumnType("decimal(3,2)");
            entity.Property(e => e.MatchedBy).HasColumnName("matched_by");
            entity.Property(e => e.BankNotification).HasColumnName("bank_notification");
            entity.Property(e => e.PayerName).HasColumnName("payer_name");
            entity.Property(e => e.RecordedAt).HasColumnName("recorded_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.InvoiceId);
            entity.HasIndex(e => e.Reference);

            entity.HasOne(e => e.Invoice)
                  .WithMany()
                  .HasForeignKey(e => e.InvoiceId);
        });

        // Customer - maps to existing Prisma table
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Job - maps to existing Prisma table
        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.TechnicianId).HasColumnName("technician_id");
            entity.Property(e => e.ServiceType).HasColumnName("service_type");
            entity.Property(e => e.EstimatedCost).HasColumnName("estimated_cost").HasColumnType("decimal(12,2)");
            entity.Property(e => e.ActualCost).HasColumnName("actual_cost").HasColumnType("decimal(12,2)");
            entity.Property(e => e.ScheduledDate).HasColumnName("scheduled_date");
            entity.Property(e => e.CompletedDate).HasColumnName("completed_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Jobs)
                  .HasForeignKey(e => e.CustomerId);
        });

        // CompanyInfo - singleton settings record
        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.ToTable("company_info");
            entity.HasKey(e => e.Id);
        });
    }
}
