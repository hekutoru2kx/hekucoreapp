using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface ILoggingSettingsRepository
{
    Task<LoggingSettingsResult> GetSettingsAsync();
    Task UpdateSettingsAsync(UpdateLoggingSettingsRequest request);
}
