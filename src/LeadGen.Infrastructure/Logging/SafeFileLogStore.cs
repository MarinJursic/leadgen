using LeadGen.Core.Services;
using Microsoft.Extensions.Logging;

namespace LeadGen.Infrastructure.Logging;

public sealed class SafeFileLogStore : ILoggerProvider, IAppLogReader, IAppLogWriter
{
    private readonly object _sync = new();
    private readonly string _logDirectory;

    public SafeFileLogStore(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SafeFileLogger(this, categoryName);
    }

    public void Dispose()
    {
    }

    public Task WriteAsync(string level, string category, string message, string? correlationId, CancellationToken ct)
    {
        Write(level, category, message, correlationId, null);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> TailAsync(int take, CancellationToken ct)
    {
        var boundedTake = Math.Clamp(take, 1, 500);
        var files = Directory.Exists(_logDirectory)
            ? Directory.GetFiles(_logDirectory, "leadgen-*.log").OrderByDescending(Path.GetFileName).Take(5).ToList()
            : [];

        var lines = new List<string>();
        foreach (var file in files)
        {
            lines.AddRange(File.ReadLines(file).Reverse().Take(boundedTake));
            if (lines.Count >= boundedTake)
            {
                break;
            }
        }

        lines.Reverse();
        return Task.FromResult<IReadOnlyList<string>>(lines.TakeLast(boundedTake).ToList());
    }

    private void Write(string level, string category, string message, string? correlationId, Exception? exception)
    {
        var safeMessage = Redact(message);
        if (exception is not null)
        {
            safeMessage += " " + Redact(exception.GetType().Name + ": " + exception.Message);
        }

        var line = $"{DateTime.UtcNow:O} level={level} correlationId={correlationId ?? CorrelationContext.Current ?? "-"} category={category} message=\"{safeMessage.Replace("\"", "'")}\"";
        var file = Path.Combine(_logDirectory, $"leadgen-{DateTime.UtcNow:yyyyMMdd}.log");
        lock (_sync)
        {
            File.AppendAllText(file, line + Environment.NewLine);
        }
    }

    private static string Redact(string value)
    {
        var clean = value.Replace("\r", " ").Replace("\n", " ");
        foreach (var marker in new[] { "DEEPSEEK_API_KEY", "TAVILY_API_KEY", "HUNTER_API_KEY", "Bearer " })
        {
            clean = clean.Replace(marker, "[redacted]", StringComparison.OrdinalIgnoreCase);
        }

        return clean.Length <= 2000 ? clean : clean[..2000];
    }

    private sealed class SafeFileLogger : ILogger
    {
        private readonly SafeFileLogStore _store;
        private readonly string _category;

        public SafeFileLogger(SafeFileLogStore store, string category)
        {
            _store = store;
            _category = category;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            _store.Write(logLevel.ToString(), _category, formatter(state, exception), CorrelationContext.Current, exception);
        }
    }
}
