using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IRSAssistantAPI.Infrastructure.Services;

public class SearchIndexManagementService
{
    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly ILogger<SearchIndexManagementService> _logger;
    private readonly string _searchIndexName;

    public SearchIndexManagementService(
        IConfiguration configuration,
        ILogger<SearchIndexManagementService> logger)
    {
        _logger = logger;

        var searchEndpoint = configuration["AzureSearch:Endpoint"] ?? throw new ArgumentNullException("AzureSearch:Endpoint");
        var searchKey = configuration["AzureSearch:Key"] ?? throw new ArgumentNullException("AzureSearch:Key");
        _searchIndexName = configuration["AzureSearch:IndexName"] ?? "tax-data-index";

        var credential = new AzureKeyCredential(searchKey);
        _indexClient = new SearchIndexClient(new Uri(searchEndpoint), credential);
        _searchClient = new SearchClient(new Uri(searchEndpoint), _searchIndexName, credential);
    }

    public async Task CreateTaxDataIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating tax data search index: {IndexName}", _searchIndexName);

            var index = new SearchIndex(_searchIndexName)
            {
                Fields = new List<SearchField>
                {
                    new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true },
                    new SimpleField("taxpayerId", SearchFieldDataType.String) { IsFilterable = true, IsSortable = true },
                    new SimpleField("documentType", SearchFieldDataType.String) { IsFilterable = true, IsSortable = true },
                    new SearchableField("content") { IsFilterable = true, IsSortable = true, AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                    new SimpleField("category", SearchFieldDataType.String) { IsFilterable = true, IsSortable = true },
                    new SimpleField("taxYear", SearchFieldDataType.Int32) { IsFilterable = true, IsSortable = true },
                    new SimpleField("amount", SearchFieldDataType.Double) { IsFilterable = true, IsSortable = true }
                }
            };

            await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
            _logger.LogInformation("Tax data search index created successfully: {IndexName}", _searchIndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tax data search index: {IndexName}", _searchIndexName);
            throw;
        }
    }

    public async Task<bool> IndexExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var index = await _indexClient.GetIndexAsync(_searchIndexName, cancellationToken);
            return index != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting search index: {IndexName}", _searchIndexName);
            await _indexClient.DeleteIndexAsync(_searchIndexName, cancellationToken);
            _logger.LogInformation("Search index deleted successfully: {IndexName}", _searchIndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting search index: {IndexName}", _searchIndexName);
            throw;
        }
    }

    public async Task<long> GetDocumentCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var searchOptions = new SearchOptions
            {
                IncludeTotalCount = true,
                Size = 0
            };

            var searchResults = await _searchClient.SearchAsync<SearchDocument>("*", searchOptions, cancellationToken);
            return searchResults.Value.TotalCount ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document count for index: {IndexName}", _searchIndexName);
            return 0;
        }
    }
}
