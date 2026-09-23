using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Hekucoreapp.Infrastructure.Repositories;

// Reads system_logs directly via Npgsql, same as SystemLogPostgresSink writes it — the table is
// deliberately outside the EF model (see the CreateSystemLogsTable migration), so this is raw SQL
// rather than an EF query. Gated by LoggingSettingsPermission.Read at the controller — no tenant
// filter here at all (unlike gestamind, which this feature was ported from): hekucoreapp is
// single-tenant, so there is nothing to scope by.
public class SystemLogsRepository : ISystemLogsRepository
{
    private readonly string _connectionString;

    public SystemLogsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<SystemLogsPageResult> QueryAsync(SystemLogsQuery query)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var page = Math.Max(query.Page, 1);

        var whereClauses = new List<string>();
        var parameterValues = new List<(string Name, object Value)>();

        if (!string.IsNullOrEmpty(query.Category))
        {
            whereClauses.Add("category = @category");
            parameterValues.Add(("category", query.Category));
        }
        if (!string.IsNullOrEmpty(query.Level))
        {
            whereClauses.Add("level = @level");
            parameterValues.Add(("level", query.Level));
        }
        if (query.From is not null)
        {
            whereClauses.Add("timestamp >= @from");
            parameterValues.Add(("from", query.From.Value));
        }
        if (query.To is not null)
        {
            whereClauses.Add("timestamp <= @to");
            parameterValues.Add(("to", query.To.Value));
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var result = new SystemLogsPageResult();

        await using (var countCmd = connection.CreateCommand())
        {
            countCmd.CommandText = $"SELECT COUNT(*) FROM system_logs {whereSql}";
            foreach (var (name, value) in parameterValues)
                countCmd.Parameters.Add(new NpgsqlParameter(name, value));
            result.TotalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
        }

        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = $"""
                SELECT id, timestamp, level, category, message, exception, user_id, trace_id
                FROM system_logs
                {whereSql}
                ORDER BY timestamp DESC
                LIMIT @pageSize OFFSET @offset
                """;
            foreach (var (name, value) in parameterValues)
                cmd.Parameters.Add(new NpgsqlParameter(name, value));
            cmd.Parameters.Add(new NpgsqlParameter("pageSize", pageSize));
            cmd.Parameters.Add(new NpgsqlParameter("offset", (page - 1) * pageSize));

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Items.Add(new SystemLogEntryResult
                {
                    Id = reader.GetInt64(0),
                    Timestamp = reader.GetDateTime(1),
                    Level = reader.GetString(2),
                    Category = reader.GetString(3),
                    Message = reader.GetString(4),
                    Exception = reader.IsDBNull(5) ? null : reader.GetString(5),
                    UserId = reader.IsDBNull(6) ? null : reader.GetString(6),
                    TraceId = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
        }

        return result;
    }
}
