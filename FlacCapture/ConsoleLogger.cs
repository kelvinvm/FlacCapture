using System;
using Microsoft.Extensions.Logging;

namespace FlacCapture;

/// <summary>
/// Simple console logger implementation for container environments
/// </summary>
public class ConsoleLogger : ILogger
{
    private readonly string _categoryName;
 private readonly LogLevel _minLevel;

    public ConsoleLogger(string categoryName, LogLevel minLevel = LogLevel.Information)
  {
        _categoryName = categoryName;
        _minLevel = minLevel;
    }

 public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    public void Log<TState>(
     LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

     string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string level = logLevel switch
        {
LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
        LogLevel.Information => "INFO ",
          LogLevel.Warning => "WARN ",
 LogLevel.Error => "ERROR",
      LogLevel.Critical => "CRIT ",
            _ => "UNKN "
        };

        string message = formatter(state, exception);

// Write to console (Docker will capture this)
      Console.WriteLine($"[{timestamp}] [{level}] {_categoryName}: {message}");

        if (exception != null)
        {
 Console.WriteLine($"[{timestamp}] [{level}] Exception: {exception.GetType().Name}: {exception.Message}");
   Console.WriteLine($"[{timestamp}] [{level}] StackTrace: {exception.StackTrace}");
     }
    }
}

/// <summary>
/// Factory for creating console loggers
/// </summary>
public class ConsoleLoggerFactory
{
    private readonly LogLevel _minLevel;

    public ConsoleLoggerFactory(LogLevel minLevel = LogLevel.Information)
    {
     _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName)
    {
     return new ConsoleLogger(categoryName, _minLevel);
    }
}
