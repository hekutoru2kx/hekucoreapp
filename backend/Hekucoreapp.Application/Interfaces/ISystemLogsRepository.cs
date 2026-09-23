using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface ISystemLogsRepository
{
    Task<SystemLogsPageResult> QueryAsync(SystemLogsQuery query);
}
