using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace Assets.Scripts.Logging
{
    public class UnityConsoleSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage();
            // You can also format the message further if needed
            Debug.Log($"[.NET API] {message}");
        }
    }

    public static class UnityConsoleSinkExtensions
    {
        public static LoggerConfiguration UnityConsole(
            this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(new UnityConsoleSink());
        }
    }
}