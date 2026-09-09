using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Domain.Interfaces;

public interface IAppSettingsService
{
    Task<AppSettingsResult> GetSettingsAsync();
    Task UpdateSettingsAsync(UpdateAppSettingsRequest request);
}
