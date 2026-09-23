using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Domain.Interfaces;

public interface ILoggingSettingsService
{
    Task<LoggingSettingsResult> GetSettingsAsync();
    Task UpdateSettingsAsync(UpdateLoggingSettingsRequest request);
}
