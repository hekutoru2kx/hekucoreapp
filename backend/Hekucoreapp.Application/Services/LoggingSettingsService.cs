using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Services;

public class LoggingSettingsService : ILoggingSettingsService
{
    private readonly ILoggingSettingsRepository _repository;

    public LoggingSettingsService(ILoggingSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<LoggingSettingsResult> GetSettingsAsync() =>
        await _repository.GetSettingsAsync();

    public async Task UpdateSettingsAsync(UpdateLoggingSettingsRequest request) =>
        await _repository.UpdateSettingsAsync(request);
}
