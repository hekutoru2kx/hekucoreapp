using Hekucoreapp.Application.DTOs;
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

    public AppSettingsController(IAppSettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = nameof(AppSettingsPermission) + "." + nameof(AppSettingsPermission.Read))]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _service.GetSettingsAsync();
        return Ok(new AppSettingsDto { RequireEmailConfirmation = result.RequireEmailConfirmation });
    }

    [HttpPut]
    [Authorize(Policy = nameof(AppSettingsPermission) + "." + nameof(AppSettingsPermission.Update))]
    public async Task<IActionResult> UpdateSettings(UpdateAppSettingsDto dto)
    {
        try
        {
            await _service.UpdateSettingsAsync(new UpdateAppSettingsRequest
            {
                RequireEmailConfirmation = dto.RequireEmailConfirmation
            });
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
