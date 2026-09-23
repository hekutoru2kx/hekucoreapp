using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Domain.Interfaces;

public interface ISystemLogsService
{
    Task<SystemLogsPageResult> QueryAsync(SystemLogsQuery query);
}
