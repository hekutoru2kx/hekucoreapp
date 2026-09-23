using Hekucoreapp.Application.DTOs;
using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Enums;
using Hekucoreapp.Domain.Enums.Permissions;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hekucoreapp.Api.Controllers;

// Admin-only log viewer, backed by SystemLogsRepository's raw query against system_logs — gated
// by LoggingSettingsPermission.Read. No tenant filter (unlike gestamind, which this feature was
// ported from): hekucoreapp is single-tenant.
[ApiController]
[Route("api/admin/logs")]
[Authorize]
public class SystemLogsController : ControllerBase
{
    private readonly ISystemLogsService _service;
    private readonly ICategoryLogger _categoryLogger;

    public SystemLogsController(ISystemLogsService service, ICategoryLogger categoryLogger)
    {
        _service = service;
        _categoryLogger = categoryLogger;
    }

    [HttpGet]
    [Authorize(Policy = nameof(LoggingSettingsPermission) + "." + nameof(LoggingSettingsPermission.Read))]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? category = null,
        [FromQuery] string? level = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var result = await _service.QueryAsync(new SystemLogsQuery
            {
                Category = category,
                Level = level,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize
            });

            return Ok(MapToDto(result));
        }
        catch (Exception ex)
        {
            _categoryLogger.LogError(LogCategory.Http, $"Unhandled exception in {nameof(SystemLogsController)}", ex);
            return BadRequest(ex.Message);
        }
    }

    private static SystemLogsPageDto MapToDto(SystemLogsPageResult result) => new()
    {
        TotalCount = result.TotalCount,
        Items = result.Items.Select(i => new SystemLogEntryDto
        {
            Id = i.Id,
            Timestamp = i.Timestamp,
            Level = i.Level,
            Category = i.Category,
            Message = i.Message,
            Exception = i.Exception,
            UserId = i.UserId,
            TraceId = i.TraceId
        }).ToList()
    };
}
