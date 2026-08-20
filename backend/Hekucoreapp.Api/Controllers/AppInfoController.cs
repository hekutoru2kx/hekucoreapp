using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hekucoreapp.Infrastructure.Data;

namespace Hekucoreapp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly HekucoreappDbContext _context;

    public AppInfoController(HekucoreappDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var appInfo = await _context.AppInfos.FirstOrDefaultAsync();
        if (appInfo == null) return NotFound();
        return Ok(appInfo);
    }
}