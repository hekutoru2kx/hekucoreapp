using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface IAppSettingsRepository
{
    Task<AppSettingsResult> GetSettingsAsync();
    Task UpdateSettingsAsync(UpdateAppSettingsRequest request);
}
