using IRSAssistantAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IRSAssistantAPI.Infrastructure.Services;

public class DatabaseSchemaService
{
    private readonly ILogger<DatabaseSchemaService> _logger;

    public DatabaseSchemaService(ILogger<DatabaseSchemaService> logger)
    {
        _logger = logger;
    }

    public string GetDatabaseSchemaDocument()
    {
        var schema = new
        {
            DatabaseName = "IRSAssistantDb",
            Description = "Tax data analysis database for IRS Assistant application",
            Tables = new[]
            {
                new
                {
                    Name = "Taxpayers",
                    Description = "Core taxpayer information and personal details",
                    Columns = new[]
                    {
                        new { Name = "Id", Type = "Guid", Description = "Primary key, unique identifier" },
                        new { Name = "FirstName", Type = "string", Description = "Taxpayer's first name" },
                        new { Name = "LastName", Type = "string", Description = "Taxpayer's last name" },
                        new { Name = "Email", Type = "string", Description = "Primary email address" },
                        new { Name = "PhoneNumber", Type = "string", Description = "Contact phone number" },
                        new { Name = "Ssn", Type = "string", Description = "Social Security Number (encrypted)" },
                        new { Name = "CreatedAt", Type = "DateTime", Description = "Account creation timestamp" },
                        new { Name = "LastLoginAt", Type = "DateTime?", Description = "Last login timestamp" },
                        new { Name = "IsActive", Type = "bool", Description = "Account active status" }
                    },
                    Relationships = new[] { "One-to-Many with TaxReturns", "One-to-Many with Properties", "One-to-Many with Assets", "One-to-Many with Dependents" }
                },
                new
                {
                    Name = "TaxReturns",
                    Description = "Annual tax return summaries and financial data",
                    Columns = new[]
                    {
                        new { Name = "Id", Type = "Guid", Description = "Primary key, unique identifier" },
                        new { Name = "TaxpayerId", Type = "Guid", Description = "Foreign key to Taxpayers table" },
                        new { Name = "TaxYear", Type = "int", Description = "Tax year (e.g., 2023, 2022)" },
                        new { Name = "FilingStatus", Type = "string", Description = "Filing status (Single, Married Filing Joint, etc.)" },
                        new { Name = "AGI", Type = "decimal", Description = "Adjusted Gross Income" },
                        new { Name = "TotalIncome", Type = "decimal", Description = "Total income before adjustments" },
                        new { Name = "TaxableIncome", Type = "decimal", Description = "Taxable income after deductions" },
                        new { Name = "Deductions", Type = "decimal", Description = "Total deductions claimed" },
                        new { Name = "TaxLiability", Type = "decimal", Description = "Total tax liability" },
                        new { Name = "TaxCredits", Type = "decimal", Description = "Total tax credits" },
                        new { Name = "TaxPaid", Type = "decimal", Description = "Total tax paid" },
                        new { Name = "Refund", Type = "decimal", Description = "Refund amount" },
                        new { Name = "BalanceDue", Type = "decimal", Description = "Balance due amount" },
                        new { Name = "CreatedAt", Type = "DateTime", Description = "Record creation timestamp" },
                        new { Name = "UpdatedAt", Type = "DateTime?", Description = "Last update timestamp" }
                    },
                    Relationships = new[] { "Many-to-One with Taxpayers", "One-to-Many with IncomeSources" }
                },
                new
                {
                    Name = "IncomeSources",
                    Description = "Detailed income breakdown by source and category",
                    Columns = new[]
                    {
                        new { Name = "Id", Type = "Guid", Description = "Primary key, unique identifier" },
                        new { Name = "ReturnId", Type = "Guid", Description = "Foreign key to TaxReturns table" },
                        new { Name = "Type", Type = "string", Description = "Income type (Wages, Interest, Dividends, Capital Gains, Rental Income, Business Income)" },
                        new { Name = "Amount", Type = "decimal", Description = "Income amount for this source" },
                        new { Name = "Description", Type = "string", Description = "Additional description or details" },
                        new { Name = "CreatedAt", Type = "DateTime", Description = "Record creation timestamp" },
                        new { Name = "UpdatedAt", Type = "DateTime?", Description = "Last update timestamp" }
                    },
                    Relationships = new[] { "Many-to-One with TaxReturns" }
                },
                new
                {
                    Name = "Properties",
                    Description = "Real estate properties and investment properties",
                    Columns = new[]
                    {
                        new { Name = "Id", Type = "Guid", Description = "Primary key, unique identifier" },
                        new { Name = "TaxpayerId", Type = "Guid", Description = "Foreign key to Taxpayers table" },
                        new { Name = "Address", Type = "string", Description = "Property address" },
                        new { Name = "Type", Type = "string", Description = "Property type (Primary Residence, Rental Property, Commercial)" },
                        new { Name = "PurchaseYear", Type = "int", Description = "Year property was purchased" },
                        new { Name = "PurchasePrice", Type = "decimal", Description = "Original purchase price" },
                        new { Name = "CurrentValue", Type = "decimal", Description = "Current estimated value" },
                        new { Name = "MortgageBalance", Type = "decimal", Description = "Remaining mortgage balance" },
                        new { Name = "RentalIncome", Type = "decimal?", Description = "Annual rental income (if applicable)" },
                        new { Name = "Expenses", Type = "decimal?", Description = "Property-related expenses" },
                        new { Name = "Notes", Type = "string", Description = "Additional notes or comments" },
                        new { Name = "CreatedAt", Type = "DateTime", Description = "Record creation timestamp" },
                        new { Name = "UpdatedAt", Type = "DateTime?", Description = "Last update timestamp" }
                    },
                    Relationships = new[] { "Many-to-One with Taxpayers" }
                },
                new
                {
                    Name = "Assets",
                    Description = "Non-real estate assets and investments",
                    Columns = new[]
                    {
                        new { Name = "Id", Type = "Guid", Description = "Primary key, unique identifier" },
                        new { Name = "TaxpayerId", Type = "Guid", Description = "Foreign key to Taxpayers table" },
                        new { Name = "Type", Type = "string", Description = "Asset type (Stock Portfolio, Crypto, Car, 401k, Roth IRA, etc.)" },
                        new { Name = "Description", Type = "string", Description = "Asset description or name" },
                        new { Name = "PurchaseYear", Type = "int", Description = "Year asset was acquired" },
                        new { Name = "PurchaseValue", Type = "decimal", Description = "Original purchase or contribution value" },
                        new { Name = "CurrentValue", Type = "decimal", Description = "Current market value" },
                        new { Name = "AnnualReturn", Type = "decimal?", Description = "Annual return percentage" },
                        new { Name = "Notes", Type = "string", Description = "Additional notes or comments" },
                        new { Name = "CreatedAt", Type = "DateTime", Description = "Record creation timestamp" },
                        new { Name = "UpdatedAt", Type = "DateTime?", Description = "Last update timestamp" }
                    },
                    Relationships = new[] { "Many-to-One with Taxpayers" }
                },
                new
                {
                    Name = "Dependents",
                    Description = "Tax dependents and family members",
                    Columns = new[]
                    {
                        new { Name = "Id", Type = "Guid", Description = "Primary key, unique identifier" },
                        new { Name = "TaxpayerId", Type = "Guid", Description = "Foreign key to Taxpayers table" },
                        new { Name = "Name", Type = "string", Description = "Dependent's full name" },
                        new { Name = "DateOfBirth", Type = "DateTime", Description = "Dependent's date of birth" },
                        new { Name = "Relationship", Type = "string", Description = "Relationship to taxpayer (Daughter, Son, Parent, etc.)" },
                        new { Name = "EligibleForCredit", Type = "bool", Description = "Whether dependent is eligible for tax credits" },
                        new { Name = "Ssn", Type = "string", Description = "Dependent's Social Security Number (encrypted)" },
                        new { Name = "Notes", Type = "string", Description = "Additional notes or comments" },
                        new { Name = "CreatedAt", Type = "DateTime", Description = "Record creation timestamp" },
                        new { Name = "UpdatedAt", Type = "DateTime?", Description = "Last update timestamp" }
                    },
                    Relationships = new[] { "Many-to-One with Taxpayers" }
                }
            },
            CommonQueries = new[]
            {
                new
                {
                    Question = "What was my total income last year?",
                    SQL = "SELECT SUM(Amount) as TotalIncome FROM IncomeSources WHERE ReturnId IN (SELECT Id FROM TaxReturns WHERE TaxYear = 2023)",
                    Description = "Gets total income from all sources for the most recent tax year"
                },
                new
                {
                    Question = "Show me all my properties and their current values",
                    SQL = "SELECT Address, Type, CurrentValue, MortgageBalance, (CurrentValue - MortgageBalance) as Equity FROM Properties",
                    Description = "Lists all properties with their current values and equity"
                },
                new
                {
                    Question = "What are my tax credits for dependents?",
                    SQL = "SELECT Name, Relationship, EligibleForCredit FROM Dependents WHERE EligibleForCredit = 1",
                    Description = "Shows all dependents eligible for tax credits"
                },
                new
                {
                    Question = "Compare my income between 2022 and 2023",
                    SQL = "SELECT TaxYear, SUM(Amount) as TotalIncome FROM IncomeSources WHERE ReturnId IN (SELECT Id FROM TaxReturns WHERE TaxYear IN (2022, 2023)) GROUP BY TaxYear ORDER BY TaxYear",
                    Description = "Compares total income across different tax years"
                }
            },
            QueryGuidelines = new[]
            {
                "Always use proper JOINs to connect related tables",
                "Filter by TaxpayerId when analyzing specific taxpayer data",
                "Use appropriate aggregate functions (SUM, AVG, COUNT, MAX, MIN)",
                "Include proper WHERE clauses for year filtering",
                "Use ORDER BY for meaningful data presentation",
                "Handle NULL values appropriately with COALESCE or ISNULL",
                "Use decimal precision for monetary calculations",
                "Consider tax year comparisons and trends"
            }
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    public string GetSchemaSummary()
    {
        return @"
DATABASE SCHEMA SUMMARY:
- Taxpayers: Core taxpayer information (Id, FirstName, LastName, Email, etc.)
- TaxReturns: Annual tax return summaries (Id, TaxpayerId, TaxYear, AGI, TotalIncome, etc.)
- IncomeSources: Detailed income breakdown by source (Id, ReturnId, Type, Amount, etc.)
- Properties: Real estate properties (Id, TaxpayerId, Address, Type, CurrentValue, etc.)
- Assets: Non-real estate assets (Id, TaxpayerId, Type, Description, CurrentValue, etc.)
- Dependents: Tax dependents (Id, TaxpayerId, Name, DateOfBirth, Relationship, etc.)

KEY RELATIONSHIPS:
- Taxpayers → TaxReturns (One-to-Many)
- TaxReturns → IncomeSources (One-to-Many)
- Taxpayers → Properties (One-to-Many)
- Taxpayers → Assets (One-to-Many)
- Taxpayers → Dependents (One-to-Many)

COMMON QUERY PATTERNS:
- Income analysis: JOIN TaxReturns with IncomeSources
- Property analysis: Query Properties table with rental income calculations
- Asset portfolio: Analyze Assets table with current vs purchase values
- Tax liability trends: Compare TaxReturns across years
- Dependent analysis: Query Dependents with eligibility checks
";
    }
}
