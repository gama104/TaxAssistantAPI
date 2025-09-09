using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure;
using Azure.Core;
using IRSAssistantAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IRSAssistantAPI.Infrastructure.Services;

public class TaxDataIndexingService
{
    private readonly SearchClient _searchClient;
    private readonly IDbContext _dbContext;
    private readonly ILogger<TaxDataIndexingService> _logger;
    private readonly string _searchIndexName;

    public TaxDataIndexingService(
        IConfiguration configuration,
        IDbContext dbContext,
        ILogger<TaxDataIndexingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;

        var searchEndpoint = configuration["AzureSearch:Endpoint"] ?? throw new ArgumentNullException("AzureSearch:Endpoint");
        var searchKey = configuration["AzureSearch:Key"] ?? throw new ArgumentNullException("AzureSearch:Key");
        _searchIndexName = configuration["AzureSearch:IndexName"] ?? "tax-data-index";

        var credential = new AzureKeyCredential(searchKey);
        _searchClient = new SearchClient(new Uri(searchEndpoint), _searchIndexName, credential);
    }

    public async Task IndexAllTaxDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting tax data indexing process");

            // Get all tax data from database
            var taxData = await GetTaxDataFromDatabase(cancellationToken);

            // Index the data
            await IndexDocumentsAsync(taxData, cancellationToken);

            _logger.LogInformation("Tax data indexing completed successfully. Indexed {Count} documents", taxData.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during tax data indexing");
            throw;
        }
    }

    private async Task<List<SearchDocument>> GetTaxDataFromDatabase(CancellationToken cancellationToken)
    {
        var documents = new List<SearchDocument>();

        // Get taxpayers
        var taxpayers = await _dbContext.Taxpayers.ToListAsync(cancellationToken);
        foreach (var taxpayer in taxpayers)
        {
            documents.Add(new SearchDocument
            {
                ["id"] = $"taxpayer_{taxpayer.Id}",
                ["taxpayerId"] = taxpayer.Id.ToString(),
                ["documentType"] = "Taxpayer",
                ["content"] = $"Taxpayer: {taxpayer.FirstName} {taxpayer.LastName}, Email: {taxpayer.Email}",
                ["category"] = "Personal Information",
                ["taxYear"] = 0,
                ["amount"] = 0.0
            });
        }

        // Get tax returns
        var taxReturns = await _dbContext.TaxReturns
            .Include(tr => tr.Taxpayer)
            .Include(tr => tr.IncomeSources)
            .ToListAsync(cancellationToken);

        foreach (var taxReturn in taxReturns)
        {
            var incomeSources = taxReturn.IncomeSources.Select(ins => $"{ins.Type}: ${ins.Amount:N2}").ToList();
            var incomeContent = string.Join(", ", incomeSources);

            documents.Add(new SearchDocument
            {
                ["id"] = $"taxreturn_{taxReturn.Id}",
                ["taxpayerId"] = taxReturn.TaxpayerId.ToString(),
                ["documentType"] = "TaxReturn",
                ["content"] = $"Tax Year {taxReturn.TaxYear}: AGI ${taxReturn.AGI:N2}, Total Income ${taxReturn.TotalIncome:N2}, Tax Liability ${taxReturn.TaxLiability:N2}. Income Sources: {incomeContent}",
                ["category"] = "Tax Return",
                ["taxYear"] = taxReturn.TaxYear,
                ["amount"] = (double)taxReturn.TotalIncome
            });
        }

        // Get properties
        var properties = await _dbContext.Properties
            .Include(p => p.Taxpayer)
            .ToListAsync(cancellationToken);

        foreach (var property in properties)
        {
            documents.Add(new SearchDocument
            {
                ["id"] = $"property_{property.Id}",
                ["taxpayerId"] = property.TaxpayerId.ToString(),
                ["documentType"] = "Property",
                ["content"] = $"{property.Type} at {property.Address}: Purchase ${property.PurchasePrice:N2} in {property.PurchaseYear}, Current Value ${property.CurrentValue:N2}, Mortgage Balance ${property.MortgageBalance:N2}",
                ["category"] = "Real Estate",
                ["taxYear"] = property.PurchaseYear,
                ["amount"] = (double)property.CurrentValue
            });
        }

        // Get assets
        var assets = await _dbContext.Assets
            .Include(a => a.Taxpayer)
            .ToListAsync(cancellationToken);

        foreach (var asset in assets)
        {
            documents.Add(new SearchDocument
            {
                ["id"] = $"asset_{asset.Id}",
                ["taxpayerId"] = asset.TaxpayerId.ToString(),
                ["documentType"] = "Asset",
                ["content"] = $"{asset.Type}: {asset.Description}, Purchase Value ${asset.PurchaseValue:N2} in {asset.PurchaseYear}, Current Value ${asset.CurrentValue:N2}",
                ["category"] = "Assets",
                ["taxYear"] = asset.PurchaseYear,
                ["amount"] = (double)asset.CurrentValue
            });
        }

        // Get dependents
        var dependents = await _dbContext.Dependents
            .Include(d => d.Taxpayer)
            .ToListAsync(cancellationToken);

        foreach (var dependent in dependents)
        {
            documents.Add(new SearchDocument
            {
                ["id"] = $"dependent_{dependent.Id}",
                ["taxpayerId"] = dependent.TaxpayerId.ToString(),
                ["documentType"] = "Dependent",
                ["content"] = $"Dependent: {dependent.Name}, {dependent.Relationship}, DOB: {dependent.DateOfBirth:yyyy-MM-dd}, Eligible for Credit: {dependent.EligibleForCredit}",
                ["category"] = "Dependents",
                ["taxYear"] = dependent.DateOfBirth.Year,
                ["amount"] = 0.0
            });
        }

        return documents;
    }

    private async Task IndexDocumentsAsync(List<SearchDocument> documents, CancellationToken cancellationToken)
    {
        const int batchSize = 100;
        
        for (int i = 0; i < documents.Count; i += batchSize)
        {
            var batch = documents.Skip(i).Take(batchSize).ToList();
            
            var batchActions = new List<IndexDocumentsAction<SearchDocument>>();
            foreach (var doc in batch)
            {
                batchActions.Add(IndexDocumentsAction.Upload(doc));
            }

            var batchIndexResult = await _searchClient.IndexDocumentsAsync(
                IndexDocumentsBatch.Create(batchActions.ToArray()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Indexed batch {BatchNumber} of {TotalBatches}", 
                (i / batchSize) + 1, 
                (int)Math.Ceiling((double)documents.Count / batchSize));
        }
    }
}
