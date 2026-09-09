using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Application.Resources;
using Hekucoreapp.Domain.Models;
using Hekucoreapp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Hekucoreapp.Infrastructure.Repositories;

// Reads/updates the single AppSettings row — AppSettingsSeeder guarantees it exists at
// startup, so GetSettingsAsync never returns null in practice.
public class AppSettingsRepository : IAppSettingsRepository
{
    private readonly HekucoreappDbContext _context;
    private readonly IStringLocalizer<Messages> _localizer;

    public AppSettingsRepository(HekucoreappDbContext context, IStringLocalizer<Messages> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<AppSettingsResult> GetSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync()
            ?? throw new Exception(_localizer["AppSettingsNotFound"]);

        return new AppSettingsResult
        {
            RequireEmailConfirmation = settings.RequireEmailConfirmation
        };
    }

    public async Task UpdateSettingsAsync(UpdateAppSettingsRequest request)
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync()
            ?? throw new Exception(_localizer["AppSettingsNotFound"]);

        settings.RequireEmailConfirmation = request.RequireEmailConfirmation;

        await _context.SaveChangesAsync();
    }
}
