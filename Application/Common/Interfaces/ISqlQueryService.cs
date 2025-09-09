namespace IRSAssistantAPI.Application.Common.Interfaces;

public interface ISqlQueryService
{
    Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);
    Task<bool> ValidateQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);
    Task<string> GetTableSchemaAsync(CancellationToken cancellationToken = default);
}
