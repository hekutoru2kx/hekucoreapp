using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Services;

public class SystemLogsService : ISystemLogsService
{
    private readonly ISystemLogsRepository _repository;

    public SystemLogsService(ISystemLogsRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemLogsPageResult> QueryAsync(SystemLogsQuery query) =>
        await _repository.QueryAsync(query);
}
