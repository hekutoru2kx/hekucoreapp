using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Enums;

namespace Hekucoreapp.Infrastructure.Logging;

// No-op ICategoryLogger. Lets tests that construct HekucoreappDbContext directly satisfy its
// constructor without pulling in Moq or the real Serilog pipeline just for that.
public class NullCategoryLogger : ICategoryLogger
{
    public static readonly NullCategoryLogger Instance = new();

    public void Log(LogCategory category, LogLevel level, string message, Exception? exception = null)
    {
    }
}
