using Hekucoreapp.Application.DTOs;
using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Enums;
using Hekucoreapp.Domain.Enums.Permissions;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hekucoreapp.Api.Controllers;

[ApiController]
[Route("api/admin/app-settings")]
[Authorize]
public class AppSettingsController : ControllerBase
{
    private readonly IAppSettingsService _service;
    private readonly ICategoryLogger _categoryLogger;

    public AppSettingsController(IAppSettingsService service, ICategoryLogger categoryLogger)
    {
        _service = service;
        _categoryLogger = categoryLogger;
    }

    [HttpGet]
    [Authorize(Policy = nameof(AppSettingsPermission) + "." + nameof(AppSettingsPermission.Read))]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _service.GetSettingsAsync();
        return Ok(new AppSettingsDto
        {
            RequireEmailConfirmation = result.RequireEmailConfirmation,
            ContentMaxBytes = result.ContentMaxBytes,
            ContentAllowedContentTypes = result.ContentAllowedContentTypes,
            ContentMaxImageDimension = result.ContentMaxImageDimension,
            ContentAvatarMaxDimension = result.ContentAvatarMaxDimension
        });
    }

    [HttpPut]
    [Authorize(Policy = nameof(AppSettingsPermission) + "." + nameof(AppSettingsPermission.Update))]
    public async Task<IActionResult> UpdateSettings(UpdateAppSettingsDto dto)
    {
        try
        {
            await _service.UpdateSettingsAsync(new UpdateAppSettingsRequest
            {
                RequireEmailConfirmation = dto.RequireEmailConfirmation,
                ContentMaxBytes = dto.ContentMaxBytes,
                ContentAllowedContentTypes = dto.ContentAllowedContentTypes,
                ContentMaxImageDimension = dto.ContentMaxImageDimension,
                ContentAvatarMaxDimension = dto.ContentAvatarMaxDimension
            });
            return Ok();
        }
        catch (Exception ex)
        {
            _categoryLogger.LogError(LogCategory.Http, $"Unhandled exception in {nameof(AppSettingsController)}", ex);
            return BadRequest(ex.Message);
        }
    }
}
