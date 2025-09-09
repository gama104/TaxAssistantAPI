using IRSAssistantAPI.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IRSAssistantAPI.Infrastructure.Services;

public class SqlQueryService : ISqlQueryService
{
    private readonly string _connectionString;
    private readonly ILogger<SqlQueryService> _logger;

    public SqlQueryService(IConfiguration configuration, ILogger<SqlQueryService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string not found");
        _logger = logger;
    }

    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
    {
        var results = new List<Dictionary<string, object>>();

        try
        {
            _logger.LogInformation("Executing SQL query: {SqlQuery}", sqlQuery);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand(sqlQuery, connection);
            command.CommandTimeout = 30; // 30 second timeout

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value ?? DBNull.Value;
                }
                results.Add(row);
            }

            _logger.LogInformation("Query executed successfully, returned {RowCount} rows", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL query: {SqlQuery}", sqlQuery);
            throw;
        }

        return results;
    }

    public async Task<bool> ValidateQueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic validation - check for dangerous keywords
            var dangerousKeywords = new[] { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "CREATE", "TRUNCATE" };
            var upperQuery = sqlQuery.ToUpperInvariant();
            
            foreach (var keyword in dangerousKeywords)
            {
                if (upperQuery.Contains(keyword))
                {
                    _logger.LogWarning("Query contains potentially dangerous keyword: {Keyword}", keyword);
                    return false;
                }
            }

            // Try to parse the query
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            using var command = new SqlCommand("SET PARSEONLY ON", connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            
            command.CommandText = sqlQuery;
            await command.ExecuteNonQueryAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query validation failed: {SqlQuery}", sqlQuery);
            return false;
        }
    }

    public async Task<string> GetTableSchemaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schemaQuery = @"
                SELECT 
                    t.TABLE_NAME,
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.IS_NULLABLE,
                    c.CHARACTER_MAXIMUM_LENGTH
                FROM INFORMATION_SCHEMA.TABLES t
                INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION";

            var results = await ExecuteQueryAsync(schemaQuery, cancellationToken);
            
            var schema = results.GroupBy(r => r["TABLE_NAME"])
                .Select(g => new
                {
                    TableName = g.Key,
                    Columns = g.Select(c => new
                    {
                        ColumnName = c["COLUMN_NAME"],
                        DataType = c["DATA_TYPE"],
                        IsNullable = c["IS_NULLABLE"],
                        MaxLength = c["CHARACTER_MAXIMUM_LENGTH"]
                    })
                });

            return System.Text.Json.JsonSerializer.Serialize(schema, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table schema");
            return "{}";
        }
    }
}
