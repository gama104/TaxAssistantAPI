using IRSAssistantAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IRSAssistantAPI.Infrastructure.Services;

public class StructuredResponseService
{
    private readonly ILogger<StructuredResponseService> _logger;

    public StructuredResponseService(ILogger<StructuredResponseService> logger)
    {
        _logger = logger;
    }

    public StructuredQueryResult ProcessQueryResult(string sqlQuery, List<object> rawData, string userQuery)
    {
        try
        {
            _logger.LogInformation("Processing query result for SQL: {SqlQuery}", sqlQuery);

            // Analyze the query type and structure the response accordingly
            var queryType = AnalyzeQueryType(sqlQuery, userQuery);
            var structuredData = StructureDataBasedOnQueryType(rawData, queryType);
            var metadata = GenerateMetadata(sqlQuery, rawData, queryType);

            return new StructuredQueryResult
            {
                QueryType = queryType,
                Data = structuredData,
                Metadata = metadata,
                RawData = rawData, // Keep raw data for debugging
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query result");
            return new StructuredQueryResult
            {
                QueryType = QueryType.Unknown,
                Data = new List<StructuredDataItem>(),
                Metadata = new QueryMetadata { Error = ex.Message },
                RawData = rawData,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    private QueryType AnalyzeQueryType(string sqlQuery, string userQuery)
    {
        var lowerSql = sqlQuery.ToLower();
        var lowerQuery = userQuery.ToLower();

        // Income analysis queries
        if (lowerSql.Contains("sum") && lowerSql.Contains("income") ||
            lowerQuery.Contains("income") && (lowerQuery.Contains("total") || lowerQuery.Contains("last year")))
        {
            return QueryType.IncomeAnalysis;
        }

        // Property analysis queries
        if (lowerSql.Contains("properties") || lowerSql.Contains("property") ||
            lowerQuery.Contains("property") || lowerQuery.Contains("real estate"))
        {
            return QueryType.PropertyAnalysis;
        }

        // Asset analysis queries
        if (lowerSql.Contains("assets") || lowerSql.Contains("asset") ||
            lowerQuery.Contains("asset") || lowerQuery.Contains("investment"))
        {
            return QueryType.AssetAnalysis;
        }

        // Dependent analysis queries
        if (lowerSql.Contains("dependents") || lowerSql.Contains("dependent") ||
            lowerQuery.Contains("dependent") || lowerQuery.Contains("child"))
        {
            return QueryType.DependentAnalysis;
        }

        // Tax liability queries
        if (lowerSql.Contains("tax") && (lowerSql.Contains("liability") || lowerSql.Contains("refund")) ||
            lowerQuery.Contains("tax") && (lowerQuery.Contains("liability") || lowerQuery.Contains("refund")))
        {
            return QueryType.TaxLiabilityAnalysis;
        }

        // Comparison queries
        if (lowerSql.Contains("group by") || lowerQuery.Contains("compare"))
        {
            return QueryType.ComparisonAnalysis;
        }

        // Trend analysis queries
        if (lowerSql.Contains("order by") && lowerSql.Contains("year") ||
            lowerQuery.Contains("trend") || lowerQuery.Contains("growth"))
        {
            return QueryType.TrendAnalysis;
        }

        return QueryType.Unknown;
    }

    private List<StructuredDataItem> StructureDataBasedOnQueryType(List<object> rawData, QueryType queryType)
    {
        return queryType switch
        {
            QueryType.IncomeAnalysis => StructureIncomeData(rawData),
            QueryType.PropertyAnalysis => StructurePropertyData(rawData),
            QueryType.AssetAnalysis => StructureAssetData(rawData),
            QueryType.DependentAnalysis => StructureDependentData(rawData),
            QueryType.TaxLiabilityAnalysis => StructureTaxLiabilityData(rawData),
            QueryType.ComparisonAnalysis => StructureComparisonData(rawData),
            QueryType.TrendAnalysis => StructureTrendData(rawData),
            _ => StructureGenericData(rawData)
        };
    }

    private List<StructuredDataItem> StructureIncomeData(List<object> rawData)
    {
        var structuredData = new List<StructuredDataItem>();

        foreach (var item in rawData)
        {
            var structuredItem = new StructuredDataItem();

            // Handle Dictionary<string, object> from raw SQL queries
            if (item is Dictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    var key = kvp.Key.ToLower();
                    var value = kvp.Value;

                    switch (key)
                    {
                        case "totalincome":
                        case "amount":
                            structuredItem.FinancialAmount = Convert.ToDecimal(value ?? 0);
                            break;
                        case "year":
                        case "taxyear":
                            structuredItem.Year = Convert.ToInt32(value ?? 0);
                            break;
                        case "type":
                            structuredItem.Category = value?.ToString() ?? "";
                            break;
                        case "percentage":
                            structuredItem.Percentage = Convert.ToDecimal(value ?? 0);
                            break;
                        case "growth":
                        case "growthrate":
                            structuredItem.GrowthRate = Convert.ToDecimal(value ?? 0);
                            break;
                    }
                }
            }
            else
            {
                // Handle regular objects with properties
                var properties = item.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    var key = prop.Name.ToLower();

                    switch (key)
                    {
                        case "totalincome":
                        case "amount":
                            structuredItem.FinancialAmount = Convert.ToDecimal(value ?? 0);
                            break;
                        case "year":
                        case "taxyear":
                            structuredItem.Year = Convert.ToInt32(value ?? 0);
                            break;
                        case "type":
                            structuredItem.Category = value?.ToString() ?? "";
                            break;
                        case "percentage":
                            structuredItem.Percentage = Convert.ToDecimal(value ?? 0);
                            break;
                        case "growth":
                        case "growthrate":
                            structuredItem.GrowthRate = Convert.ToDecimal(value ?? 0);
                            break;
                    }
                }
            }

            structuredData.Add(structuredItem);
        }

        return structuredData;
    }

    private List<StructuredDataItem> StructurePropertyData(List<object> rawData)
    {
        var structuredData = new List<StructuredDataItem>();

        foreach (var item in rawData)
        {
            var properties = item.GetType().GetProperties();
            var structuredItem = new StructuredDataItem();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                var key = prop.Name.ToLower();

                switch (key)
                {
                    case "address":
                        structuredItem.Address = value?.ToString() ?? "";
                        break;
                    case "type":
                        structuredItem.Category = value?.ToString() ?? "";
                        break;
                    case "currentvalue":
                        structuredItem.CurrentValue = Convert.ToDecimal(value ?? 0);
                        break;
                    case "purchaseprice":
                        structuredItem.PurchaseValue = Convert.ToDecimal(value ?? 0);
                        break;
                    case "mortgagebalance":
                        structuredItem.MortgageBalance = Convert.ToDecimal(value ?? 0);
                        break;
                    case "rentalincome":
                        structuredItem.RentalIncome = Convert.ToDecimal(value ?? 0);
                        break;
                    case "equity":
                        structuredItem.Equity = Convert.ToDecimal(value ?? 0);
                        break;
                }
            }

            structuredData.Add(structuredItem);
        }

        return structuredData;
    }

    private List<StructuredDataItem> StructureAssetData(List<object> rawData)
    {
        var structuredData = new List<StructuredDataItem>();

        foreach (var item in rawData)
        {
            var properties = item.GetType().GetProperties();
            var structuredItem = new StructuredDataItem();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                var key = prop.Name.ToLower();

                switch (key)
                {
                    case "type":
                        structuredItem.Category = value?.ToString() ?? "";
                        break;
                    case "description":
                        structuredItem.Description = value?.ToString() ?? "";
                        break;
                    case "currentvalue":
                        structuredItem.CurrentValue = Convert.ToDecimal(value ?? 0);
                        break;
                    case "purchasevalue":
                        structuredItem.PurchaseValue = Convert.ToDecimal(value ?? 0);
                        break;
                    case "annualreturn":
                        structuredItem.AnnualReturn = Convert.ToDecimal(value ?? 0);
                        break;
                }
            }

            structuredData.Add(structuredItem);
        }

        return structuredData;
    }

    private List<StructuredDataItem> StructureDependentData(List<object> rawData)
    {
        var structuredData = new List<StructuredDataItem>();

        foreach (var item in rawData)
        {
            var properties = item.GetType().GetProperties();
            var structuredItem = new StructuredDataItem();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                var key = prop.Name.ToLower();

                switch (key)
                {
                    case "name":
                        structuredItem.Name = value?.ToString() ?? "";
                        break;
                    case "relationship":
                        structuredItem.Relationship = value?.ToString() ?? "";
                        break;
                    case "dateofbirth":
                        if (DateTime.TryParse(value?.ToString(), out var dob))
                            structuredItem.DateOfBirth = dob;
                        break;
                    case "eligibleforcredit":
                        structuredItem.EligibleForCredit = Convert.ToBoolean(value ?? false);
                        break;
                    case "estimatedcredit":
                        structuredItem.EstimatedCredit = Convert.ToDecimal(value ?? 0);
                        break;
                }
            }

            structuredData.Add(structuredItem);
        }

        return structuredData;
    }

    private List<StructuredDataItem> StructureTaxLiabilityData(List<object> rawData)
    {
        var structuredData = new List<StructuredDataItem>();

        foreach (var item in rawData)
        {
            var properties = item.GetType().GetProperties();
            var structuredItem = new StructuredDataItem();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                var key = prop.Name.ToLower();

                switch (key)
                {
                    case "taxyear":
                        structuredItem.Year = Convert.ToInt32(value ?? 0);
                        break;
                    case "totalincome":
                        structuredItem.FinancialAmount = Convert.ToDecimal(value ?? 0);
                        break;
                    case "agi":
                        structuredItem.AGI = Convert.ToDecimal(value ?? 0);
                        break;
                    case "taxableincome":
                        structuredItem.TaxableIncome = Convert.ToDecimal(value ?? 0);
                        break;
                    case "taxliability":
                        structuredItem.TaxLiability = Convert.ToDecimal(value ?? 0);
                        break;
                    case "taxcredits":
                        structuredItem.TaxCredits = Convert.ToDecimal(value ?? 0);
                        break;
                    case "taxpaid":
                        structuredItem.TaxPaid = Convert.ToDecimal(value ?? 0);
                        break;
                    case "refund":
                        structuredItem.Refund = Convert.ToDecimal(value ?? 0);
                        break;
                    case "effectiverate":
                        structuredItem.EffectiveRate = Convert.ToDecimal(value ?? 0);
                        break;
                }
            }

            structuredData.Add(structuredItem);
        }

        return structuredData;
    }

    private List<StructuredDataItem> StructureComparisonData(List<object> rawData)
    {
        // For comparison queries, structure based on the comparison type
        return StructureGenericData(rawData);
    }

    private List<StructuredDataItem> StructureTrendData(List<object> rawData)
    {
        // For trend queries, structure with time-based data
        return StructureGenericData(rawData);
    }

    private List<StructuredDataItem> StructureGenericData(List<object> rawData)
    {
        var structuredData = new List<StructuredDataItem>();

        foreach (var item in rawData)
        {
            var structuredItem = new StructuredDataItem();

            // Handle Dictionary<string, object> from raw SQL queries
            if (item is Dictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    var key = kvp.Key.ToLower();
                    var value = kvp.Value;

                    // Generic mapping for unknown data types
                    if (key.Contains("amount") || key.Contains("value") || key.Contains("income"))
                    {
                        structuredItem.FinancialAmount = Convert.ToDecimal(value ?? 0);
                    }
                    else if (key.Contains("year"))
                    {
                        structuredItem.Year = Convert.ToInt32(value ?? 0);
                    }
                    else if (key.Contains("type") || key.Contains("category"))
                    {
                        structuredItem.Category = value?.ToString() ?? "";
                    }
                    else if (key.Contains("name"))
                    {
                        structuredItem.Name = value?.ToString() ?? "";
                    }
                }
            }
            else
            {
                // Handle regular objects with properties
                var properties = item.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    var key = prop.Name.ToLower();

                    // Generic mapping for unknown data types
                    if (key.Contains("amount") || key.Contains("value") || key.Contains("income"))
                    {
                        structuredItem.FinancialAmount = Convert.ToDecimal(value ?? 0);
                    }
                    else if (key.Contains("year"))
                    {
                        structuredItem.Year = Convert.ToInt32(value ?? 0);
                    }
                    else if (key.Contains("type") || key.Contains("category"))
                    {
                        structuredItem.Category = value?.ToString() ?? "";
                    }
                    else if (key.Contains("name"))
                    {
                        structuredItem.Name = value?.ToString() ?? "";
                    }
                }
            }

            structuredData.Add(structuredItem);
        }

        return structuredData;
    }

    private QueryMetadata GenerateMetadata(string sqlQuery, List<object> rawData, QueryType queryType)
    {
        return new QueryMetadata
        {
            QueryType = queryType.ToString(),
            RecordCount = rawData.Count,
            GeneratedAt = DateTime.UtcNow,
            SqlQuery = sqlQuery,
            HasFinancialData = rawData.Any(item =>
                item.GetType().GetProperties().Any(p =>
                    p.Name.ToLower().Contains("amount") ||
                    p.Name.ToLower().Contains("value") ||
                    p.Name.ToLower().Contains("income"))),
            DataQuality = rawData.Count > 0 ? "Good" : "No Data"
        };
    }
}

public class StructuredQueryResult
{
    public QueryType QueryType { get; set; }
    public List<StructuredDataItem> Data { get; set; } = new();
    public QueryMetadata Metadata { get; set; } = new();
    public List<object> RawData { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

public class StructuredDataItem
{
    // Financial data
    public decimal FinancialAmount { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal PurchaseValue { get; set; }
    public decimal MortgageBalance { get; set; }
    public decimal RentalIncome { get; set; }
    public decimal Equity { get; set; }
    public decimal TaxLiability { get; set; }
    public decimal TaxCredits { get; set; }
    public decimal TaxPaid { get; set; }
    public decimal Refund { get; set; }
    public decimal AGI { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal EstimatedCredit { get; set; }
    public decimal AnnualReturn { get; set; }
    public decimal Percentage { get; set; }
    public decimal GrowthRate { get; set; }

    // Categorical data
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Address { get; set; } = "";
    public string Relationship { get; set; } = "";

    // Temporal data
    public int Year { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Boolean data
    public bool EligibleForCredit { get; set; }
}

public class QueryMetadata
{
    public string QueryType { get; set; } = "";
    public int RecordCount { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string SqlQuery { get; set; } = "";
    public bool HasFinancialData { get; set; }
    public string DataQuality { get; set; } = "";
    public string Error { get; set; } = "";
}

public enum QueryType
{
    Unknown,
    IncomeAnalysis,
    PropertyAnalysis,
    AssetAnalysis,
    DependentAnalysis,
    TaxLiabilityAnalysis,
    ComparisonAnalysis,
    TrendAnalysis
}
