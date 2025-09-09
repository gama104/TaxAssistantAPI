using Azure.AI.OpenAI;
using Azure;
using IRSAssistantAPI.Application.Common.Interfaces;
using IRSAssistantAPI.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace IRSAssistantAPI.Infrastructure.Services;

public class AIAgentService : IAzureOpenAIService
{
    private readonly OpenAIClient _openAIClient;
    private readonly IDbContext _dbContext;
    private readonly DatabaseSchemaService _schemaService;
    private readonly StructuredResponseService _structuredResponseService;
    private readonly SecurityAuditService _securityAuditService;
    private readonly ILogger<AIAgentService> _logger;
    private readonly string _deploymentName;

    public AIAgentService(
        IConfiguration configuration,
        IDbContext dbContext,
        DatabaseSchemaService schemaService,
        StructuredResponseService structuredResponseService,
        SecurityAuditService securityAuditService,
        ILogger<AIAgentService> logger)
    {
        _dbContext = dbContext;
        _schemaService = schemaService;
        _structuredResponseService = structuredResponseService;
        _securityAuditService = securityAuditService;
        _logger = logger;

        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
        var key = configuration["AzureOpenAI:Key"] ?? throw new ArgumentNullException("AzureOpenAI:Key");
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4.1-mini";

        var credential = new AzureKeyCredential(key);
        _openAIClient = new OpenAIClient(new Uri(endpoint), credential);
    }

    public async Task<QueryResponseDto> ProcessQueryAsync(string userQuery, Guid sessionId, Guid? taxpayerId = null, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("AI Agent processing query: {Query}", userQuery);

            // Step 0: Security validation
            var securityCheck = ValidateQuerySecurity(userQuery);
            if (!securityCheck.IsValid)
            {
                _securityAuditService.LogSecurityViolation(
                    securityCheck.Violation,
                    userQuery,
                    taxpayerId: taxpayerId?.ToString(),
                    sessionId: sessionId.ToString()
                );

                return new QueryResponseDto
                {
                    Response = "I cannot process this request as it may contain sensitive information or potentially harmful content. Please ask questions related to tax data analysis only.",
                    ErrorMessage = securityCheck.Violation,
                    ExecutionTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                    Timestamp = DateTime.UtcNow
                };
            }

            // Step 1: Generate SQL query using AI Agent
            var sqlQuery = await GenerateSQLQueryAsync(userQuery, taxpayerId, cancellationToken);

            List<object> rawQueryResult;
            string finalSqlQuery = null;

            // Check if AI determined no database query is needed
            if (sqlQuery.Trim().Equals("NO_QUERY", StringComparison.OrdinalIgnoreCase))
            {
                rawQueryResult = new List<object>();
                finalSqlQuery = null;

                // Generate helpful response about capabilities instead of data analysis
                var capabilitiesResponse = await GenerateCapabilitiesResponseAsync(userQuery, cancellationToken);

                return new QueryResponseDto
                {
                    Response = capabilitiesResponse,
                    SqlQuery = null, // No SQL query for informational responses
                    Data = new List<object>(),
                    Confidence = 0.95m,
                    ExecutionTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                // Step 2: Validate and fix SQL query
                var validatedSqlQuery = ValidateAndFixSQLQuery(sqlQuery);

                // Step 3: Execute SQL query against database
                rawQueryResult = await ExecuteSQLQueryAsync(validatedSqlQuery, cancellationToken);
                finalSqlQuery = validatedSqlQuery;
            }

            // Step 3: Structure the response using industry standards
            var structuredResult = _structuredResponseService.ProcessQueryResult(finalSqlQuery ?? sqlQuery, rawQueryResult, userQuery);

            // Step 4: Generate intelligent response using structured data
            var response = await GenerateIntelligentResponseAsync(userQuery, structuredResult, finalSqlQuery ?? sqlQuery, cancellationToken);

            var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Log successful query execution
            _securityAuditService.LogQueryExecution(
                userQuery,
                finalSqlQuery ?? sqlQuery,
                taxpayerId?.ToString() ?? "unknown",
                sessionId.ToString(),
                true,
                rawQueryResult.Count
            );

            return new QueryResponseDto
            {
                Response = response,
                SqlQuery = finalSqlQuery, // Only set if we actually have a database query
                Data = rawQueryResult,
                Confidence = CalculateConfidence(rawQueryResult, userQuery),
                ExecutionTimeMs = executionTime,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Agent error processing query: {Query}", userQuery);

            return new QueryResponseDto
            {
                Response = "I apologize, but I encountered an error processing your request. Please try rephrasing your question.",
                ErrorMessage = ex.Message,
                ExecutionTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private string ValidateAndFixSQLQuery(string sqlQuery)
    {
        // Only fix critical reserved keyword issues that would cause SQL syntax errors
        var fixedQuery = sqlQuery
            // Fix reserved keyword aliases that cause syntax errors
            .Replace("FROM IncomeSources IS", "FROM IncomeSources inc")
            .Replace("JOIN IncomeSources IS", "JOIN IncomeSources inc")
            .Replace("FROM TaxReturns TR", "FROM TaxReturns ret")
            .Replace("JOIN TaxReturns TR", "JOIN TaxReturns ret")
            .Replace("FROM Taxpayers T", "FROM Taxpayers tax")
            .Replace("JOIN Taxpayers T", "JOIN Taxpayers tax")
            .Replace("FROM Properties P", "FROM Properties prop")
            .Replace("JOIN Properties P", "JOIN Properties prop")
            .Replace("FROM Assets A", "FROM Assets ast")
            .Replace("JOIN Assets A", "JOIN Assets ast")
            .Replace("FROM Dependents D", "FROM Dependents dep")
            .Replace("JOIN Dependents D", "JOIN Dependents dep");

        _logger.LogInformation("SQL Query validation - Original: {Original}, Fixed: {Fixed}", sqlQuery, fixedQuery);
        return fixedQuery;
    }

    private async Task<string> GenerateSQLQueryAsync(string naturalLanguageQuery, Guid? taxpayerId, CancellationToken cancellationToken)
    {
        // Get the complete database schema from our knowledge base
        var schemaDocument = _schemaService.GetDatabaseSchemaDocument();
        var schemaSummary = _schemaService.GetSchemaSummary();

        var taxpayerFilter = taxpayerId.HasValue ? $@"
IMPORTANT: This query is for a specific taxpayer (ID: {taxpayerId.Value}). 
Always include WHERE clauses to filter by TaxpayerId = '{taxpayerId.Value}' when querying taxpayer-specific data." : "";

        var systemPrompt = $@"You are an expert SQL query generator for tax data analysis. You have access to the complete database schema and knowledge base.

DATABASE STRUCTURE OVERVIEW:
{schemaSummary}

DETAILED SCHEMA DOCUMENT:
{schemaDocument}

QUERY GENERATION GUIDELINES:

1. **Table Relationships & Data Location:**
   - TaxReturns table contains the main financial data (TotalIncome, AGI, TaxableIncome, etc.)
   - IncomeSources table contains detailed income breakdowns linked to TaxReturns
   - Properties, Assets, Dependents are linked directly to Taxpayers
   - Always use proper JOINs based on foreign key relationships

2. **Income Analysis Patterns:**
   - For total income: Use TaxReturns.TotalIncome (main field) or SUM(IncomeSources.Amount) for detailed breakdown
   - For income by source: JOIN IncomeSources with TaxReturns
   - For year comparisons: Filter by TaxReturns.TaxYear

3. **Query Structure:**
   - Use exact table and column names from schema
   - Apply proper WHERE clauses for taxpayer and year filtering
   - Use appropriate aggregate functions (SUM, AVG, COUNT, MAX, MIN)
   - Handle NULL values with COALESCE or ISNULL
   - Use ORDER BY for meaningful data presentation

4. **Safe Aliases (CRITICAL):**
   - Use: 'inc' for IncomeSources, 'ret' for TaxReturns, 'tax' for Taxpayers
   - Use: 'prop' for Properties, 'ast' for Assets, 'dep' for Dependents
   - NEVER use reserved keywords: IS, AS, ON, IN, BY, OR, AND, etc.

5. **Security Requirements:**
   - ONLY generate SELECT statements
   - NEVER access SSN, passwords, or sensitive personal data
   - NEVER generate DROP, DELETE, INSERT, UPDATE, or ALTER statements
   - Always filter by TaxpayerId for specific taxpayer queries

6. **Response Format:**
   - If the question requires actual database data (income, properties, assets, etc.), generate a SQL query
   - If the question is informational/general (""what can I ask"", ""how to use"", etc.), respond with ""NO_QUERY""
   - Only generate SQL for questions that need real data from the database

{taxpayerFilter}

Generate ONLY a clean, safe SELECT query that directly answers the user's question, OR respond with ""NO_QUERY"" if no database query is needed. No explanations or comments.";

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(naturalLanguageQuery)
            },
            MaxTokens = 800,
            Temperature = 0.1f
        };
        chatCompletionsOptions.DeploymentName = _deploymentName;

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        return response.Value.Choices[0].Message.Content;
    }

    private async Task<List<object>> ExecuteSQLQueryAsync(string sqlQuery, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("AI Agent executing SQL: {SqlQuery}", sqlQuery);

            // Additional SQL security validation
            var sqlSecurityCheck = ValidateSQLSecurity(sqlQuery);
            if (!sqlSecurityCheck.IsValid)
            {
                _logger.LogWarning("SQL Security violation detected: {Violation} - SQL: {SqlQuery}", sqlSecurityCheck.Violation, sqlQuery);
                return new List<object> { new { error = "Query blocked for security reasons", details = sqlSecurityCheck.Violation } };
            }


            // Execute the SQL query against the real database
            var result = new List<object>();

            // Use Entity Framework's FromSqlRaw for raw SQL execution
            try
            {
                // Use a simple approach with raw SQL execution
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        await connection.OpenAsync(cancellationToken);
                    }

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sqlQuery;

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                }
                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing raw SQL query: {SqlQuery}", sqlQuery);
                throw;
            }

            _logger.LogInformation("AI Agent query returned {Count} results", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Agent error executing SQL: {SqlQuery}", sqlQuery);
            return new List<object> { new { error = "Query execution failed", details = ex.Message } };
        }
    }

    private async Task<string> GenerateCapabilitiesResponseAsync(string userQuery, CancellationToken cancellationToken)
    {
        // Get the database schema to provide accurate capabilities information
        var schemaSummary = _schemaService.GetSchemaSummary();

        var systemPrompt = $@"You are a helpful tax data analysis assistant. The user is asking about your capabilities or how to use the system.

DATABASE SCHEMA AVAILABLE:
{schemaSummary}

RESPONSE GUIDELINES:
1. **Be Accurate**: Only mention capabilities that are actually available based on the database schema
2. **Be Specific**: Give concrete examples of questions they can ask
3. **Be Helpful**: Provide clear, actionable guidance
4. **Be Honest**: Don't make up capabilities that don't exist

CAPABILITIES TO HIGHLIGHT:
- Income analysis (total income, income by source, year-over-year comparisons)
- Tax return data analysis (AGI, taxable income, deductions, credits)
- Property analysis (property values, locations, types)
- Asset analysis (asset values, types, descriptions)
- Dependent information (names, ages, relationships)
- Year-over-year comparisons and trends
- Tax liability and payment analysis

RESPONSE FORMAT:
- Start with a brief overview of what you can help with
- Provide 3-5 specific example questions they can ask
- Mention any limitations or requirements (like needing specific taxpayer data)
- Keep it concise and practical

TONE: Helpful, accurate, and encouraging. Focus on what they CAN do, not what they can't.";

        var userPrompt = $@"USER QUESTION: {userQuery}

TASK: Provide a helpful response about your capabilities based on the actual database schema. Give specific examples of questions they can ask about their tax data.";

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            },
            MaxTokens = 600,
            Temperature = 0.3f
        };
        chatCompletionsOptions.DeploymentName = _deploymentName;

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        return response.Value.Choices[0].Message.Content;
    }

    private async Task<string> GenerateIntelligentResponseAsync(string userQuery, StructuredQueryResult structuredResult, string sqlQuery, CancellationToken cancellationToken)
    {
        var rawDataJson = JsonSerializer.Serialize(structuredResult.RawData, new JsonSerializerOptions { WriteIndented = true });

        var systemPrompt = $@"You are an expert tax data analysis assistant. Analyze the query results and provide a clear, concise response that directly answers the user's question.

RESPONSE GUIDELINES:
1. **Be Direct**: Answer the specific question asked, don't over-explain
2. **Use Data**: Reference actual numbers and values from the results
3. **Be Concise**: Keep responses brief and to the point
4. **Professional Tone**: Use clear, professional language
5. **Client-Friendly**: Never mention technical details like SQL, database queries, or system restrictions
6. **Handle Missing Data Gracefully**: If data is unavailable, simply state that the information is not available without technical explanations

RESPONSE FORMAT:
- Start with the direct answer to the question
- Include specific data points from the results
- Add brief context or insights if helpful
- If data is missing, say ""The information for [specific time period/data] is not currently available""
- Keep it under 200 words unless complex analysis is needed

TONE: Professional, direct, and helpful. Avoid technical jargon or system details. Focus on the user's tax information, not how it's retrieved.";

        var userPrompt = $@"USER QUESTION: {userQuery}

QUERY RESULTS: {rawDataJson}

TASK: Provide a direct, concise answer to the user's question based on the query results. Be specific with numbers and data points. If no data is available, simply state that the information is not currently available without mentioning technical details.";

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            },
            MaxTokens = 800,
            Temperature = 0.2f
        };
        chatCompletionsOptions.DeploymentName = _deploymentName;

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        return response.Value.Choices[0].Message.Content;
    }


    private decimal CalculateConfidence(List<object> queryResult, string userQuery)
    {
        if (queryResult == null || queryResult.Count == 0)
            return 0.5m;

        // Simple confidence based on result count and data quality
        return queryResult.Count > 0 ? 0.95m : 0.5m;
    }


    private SecurityValidationResult ValidateQuerySecurity(string userQuery)
    {
        var query = userQuery.ToLowerInvariant();

        // Basic security checks
        var dangerousPatterns = new[]
        {
            "drop", "delete", "insert", "update", "alter", "create", "exec", "execute",
            "ssn", "password", "admin", "root", "system", "hack", "exploit"
        };

        foreach (var pattern in dangerousPatterns)
        {
            if (query.Contains(pattern))
            {
                return new SecurityValidationResult
                {
                    IsValid = false,
                    Violation = $"Potentially dangerous content detected: '{pattern}'"
                };
            }
        }

        // Check query length
        if (query.Length > 1000)
        {
            return new SecurityValidationResult
            {
                IsValid = false,
                Violation = "Query is too long"
            };
        }

        return new SecurityValidationResult { IsValid = true };
    }

    private SecurityValidationResult ValidateSQLSecurity(string sqlQuery)
    {
        var query = sqlQuery.ToUpperInvariant();

        // Only allow SELECT statements
        if (!query.TrimStart().StartsWith("SELECT"))
        {
            return new SecurityValidationResult
            {
                IsValid = false,
                Violation = "Only SELECT statements are allowed"
            };
        }

        // Check for dangerous keywords
        var dangerousKeywords = new[] { "DROP", "DELETE", "INSERT", "UPDATE", "ALTER", "CREATE", "EXEC", "EXECUTE" };

        foreach (var keyword in dangerousKeywords)
        {
            if (query.Contains(keyword))
            {
                return new SecurityValidationResult
                {
                    IsValid = false,
                    Violation = $"Dangerous SQL keyword detected: '{keyword}'"
                };
            }
        }

        return new SecurityValidationResult { IsValid = true };
    }

}

public class SecurityValidationResult
{
    public bool IsValid { get; set; }
    public string Violation { get; set; } = string.Empty;
}
