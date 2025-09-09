using IRSAssistantAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IRSAssistantAPI.Application.Common.Interfaces;

public interface IDbContext
{
    // Core entities
    DbSet<Taxpayer> Taxpayers { get; }
    DbSet<TaxReturn> TaxReturns { get; }
    DbSet<IncomeSource> IncomeSources { get; }
    DbSet<Property> Properties { get; }
    DbSet<Asset> Assets { get; }
    DbSet<Dependent> Dependents { get; }
    
    // Document and chat entities
    DbSet<TaxDocument> TaxDocuments { get; }
    DbSet<ChatSession> ChatSessions { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    
    // Database access
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
