using IRSAssistantAPI.Application.Common.Interfaces;
using IRSAssistantAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IRSAssistantAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Taxpayer> Taxpayers => Set<Taxpayer>();
    public DbSet<TaxReturn> TaxReturns => Set<TaxReturn>();
    public DbSet<IncomeSource> IncomeSources => Set<IncomeSource>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Dependent> Dependents => Set<Dependent>();
    
    // Document and chat entities
    public DbSet<TaxDocument> TaxDocuments => Set<TaxDocument>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Taxpayer
        modelBuilder.Entity<Taxpayer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Ssn).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Ssn).IsUnique();
        });

        // Configure TaxReturn
        modelBuilder.Entity<TaxReturn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaxpayerId).IsRequired();
            entity.Property(e => e.TaxYear).IsRequired();
            entity.Property(e => e.FilingStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AGI).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalIncome).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxableIncome).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Deductions).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxLiability).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxCredits).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxPaid).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Refund).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.BalanceDue).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Taxpayer)
                .WithMany(e => e.TaxReturns)
                .HasForeignKey(e => e.TaxpayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TaxpayerId, e.TaxYear }).IsUnique();
        });

        // Configure IncomeSource
        modelBuilder.Entity<IncomeSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReturnId).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.TaxReturn)
                .WithMany(e => e.IncomeSources)
                .HasForeignKey(e => e.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ReturnId, e.Type });
        });

        // Configure Property
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaxpayerId).IsRequired();
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PurchaseYear).IsRequired();
            entity.Property(e => e.PurchasePrice).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrentValue).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.MortgageBalance).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.RentalIncome).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Expenses).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Taxpayer)
                .WithMany(e => e.Properties)
                .HasForeignKey(e => e.TaxpayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TaxpayerId);
        });

        // Configure Asset
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaxpayerId).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PurchaseYear).IsRequired();
            entity.Property(e => e.PurchaseValue).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrentValue).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.AnnualReturn).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Taxpayer)
                .WithMany(e => e.Assets)
                .HasForeignKey(e => e.TaxpayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TaxpayerId);
        });

        // Configure Dependent
        modelBuilder.Entity<Dependent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaxpayerId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.Relationship).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EligibleForCredit).IsRequired();
            entity.Property(e => e.Ssn).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Taxpayer)
                .WithMany(e => e.Dependents)
                .HasForeignKey(e => e.TaxpayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TaxpayerId);
        });

        // Configure TaxDocument
        modelBuilder.Entity<TaxDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.UploadedAt).IsRequired();
            entity.Property(e => e.ProcessedAt).IsRequired();
            entity.Property(e => e.TaxpayerId).IsRequired();

            entity.HasOne(e => e.Taxpayer)
                .WithMany(e => e.TaxDocuments)
                .HasForeignKey(e => e.TaxpayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TaxpayerId, e.TaxYear });
            entity.HasIndex(e => e.DocumentType);
        });

        // Configure ChatSession
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TaxpayerId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();

            entity.HasOne(e => e.Taxpayer)
                .WithMany(e => e.ChatSessions)
                .HasForeignKey(e => e.TaxpayerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TaxpayerId, e.IsActive });
        });

        // Configure ChatMessage
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChatSessionId).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Confidence).HasColumnType("decimal(5,4)"); // 0.0000 to 1.0000
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.ChatSession)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ChatSessionId, e.CreatedAt });
        });
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}
