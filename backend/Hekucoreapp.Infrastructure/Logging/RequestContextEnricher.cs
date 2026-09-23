using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Security.Claims;

namespace Hekucoreapp.Infrastructure.Logging;

// Tags every log event with UserId/TraceId off the current HttpContext. No TenantId here —
// hekucoreapp is single-tenant (unlike gestamind, which this feature was ported from), so there
// is nothing to filter the log viewer by beyond category/level/time.
public class RequestContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return;

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", httpContext.TraceIdentifier));
    }
}
