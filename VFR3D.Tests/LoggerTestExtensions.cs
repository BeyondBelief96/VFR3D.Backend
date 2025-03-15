using Microsoft.Extensions.Logging;
using NSubstitute;

namespace VFR3D.Tests
{
    public static class LoggerTestExtensions
    {
        public static void AnyLogOfType(this ILogger logger, LogLevel level)
        {
            logger.Log(level, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}
