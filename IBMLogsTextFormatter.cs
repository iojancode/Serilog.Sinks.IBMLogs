using System;
using System.IO;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.IBMLogs
{
    class IBMLogsTextFormatter : ITextFormatter
    {
        private static readonly JsonValueFormatter Instance = new JsonValueFormatter();

        private readonly string _applicationName;
        private readonly string _subsystemName;
        private readonly string _computerName;

        public IBMLogsTextFormatter(string applicationName, string subsystemName, string computerName)
        {
            if (applicationName == null) throw new ArgumentNullException(nameof(applicationName));
            if (subsystemName == null) throw new ArgumentNullException(nameof(subsystemName));
            if (computerName == null) throw new ArgumentNullException(nameof(computerName));

            _applicationName = applicationName;
            _subsystemName = subsystemName;
            _computerName = computerName;
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));

            try
            {
                var buffer = new StringWriter();
                FormatContent(logEvent, buffer);
                output.WriteLine(buffer.ToString());
            }
            catch (Exception e)
            {
                LogNonFormattableEvent(logEvent, e);
            }
        }

        private void FormatContent(LogEvent logEvent, TextWriter output)
        {
            output.Write("{\"timestamp\":");
            output.Write(logEvent.Timestamp.ToUnixTimeNanoseconds());

            output.Write(",\"severity\":");
            output.Write(GetLogLevel(logEvent.Level));

            output.Write(",\"applicationName\":");
            JsonValueFormatter.WriteQuotedJsonString(_applicationName, output);

            output.Write(",\"subsystemName\":");
            JsonValueFormatter.WriteQuotedJsonString(_subsystemName, output);

            output.Write(",\"computerName\":");
            JsonValueFormatter.WriteQuotedJsonString(_computerName, output);

            WriteTextObject(logEvent, output);

            output.Write('}');
        }

        private static int GetLogLevel(LogEventLevel level)
        {
            switch(level)
            {
                case LogEventLevel.Verbose: return 2; // Verbose
                case LogEventLevel.Debug: return 1; // Debug
                case LogEventLevel.Information: return 3; // Info
                case LogEventLevel.Warning: return 4; // Warn
                case LogEventLevel.Error: return 5; // Error
                case LogEventLevel.Fatal: return 6; // Critical
                default: return 0;
            }
        }

        private static void WriteTextObject(LogEvent logEvent, TextWriter output)
        {
            output.Write(",\"text\":{");

            output.Write("\"Message\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);

            if (logEvent.Exception != null)
            {
                output.Write(",\"Exception\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            // if (logEvent.TraceId != null && !logEvent.Properties.ContainsKey("TraceId"))
            // {
            //     output.Write(",\"TraceId\":");
            //     JsonValueFormatter.WriteQuotedJsonString(logEvent.TraceId.ToString(), output);
            // }

            // if (logEvent.SpanId != null && !logEvent.Properties.ContainsKey("SpanId"))
            // {
            //     output.Write(",\"SpanId\":");
            //     JsonValueFormatter.WriteQuotedJsonString(logEvent.SpanId.ToString(), output);
            // }

            foreach (var property in logEvent.Properties)
            {
                if (property.Key == "Message" || property.Key == "Exception") continue;

                output.Write(",");
                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                Instance.Format(property.Value, output);
            }

            output.Write('}');
        }


        private static void LogNonFormattableEvent(LogEvent logEvent, Exception e)
        {
            SelfLog.WriteLine(
                "Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
                logEvent.Timestamp.ToString("o"),
                logEvent.MessageTemplate.Text,
                e);
        }
    }
}