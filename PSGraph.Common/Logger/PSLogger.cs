using System;
using Microsoft.Extensions.Logging;
using System.Management.Automation;

public class PSLogger : ILogger
{
    private readonly PSCmdlet _cmdlet;

    public PSLogger(PSCmdlet cmdlet)
    {
        _cmdlet = cmdlet;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId,
        TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);

        switch (logLevel)
        {
            case LogLevel.Debug:
                _cmdlet.WriteDebug(message);
                break;
            case LogLevel.Information:
                _cmdlet.WriteVerbose(message);
                break;
            case LogLevel.Warning:
                _cmdlet.WriteWarning(message);
                break;
            case LogLevel.Error:
                _cmdlet.WriteError(new ErrorRecord(exception ?? new Exception(message), "Error", ErrorCategory.NotSpecified, null));
                break;
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) => null;
}
