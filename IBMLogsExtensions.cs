using System;
using System.Net;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.IBMLogs;

namespace Serilog
{
    public static class LogdnaExtensions
    {
        public static LoggerConfiguration IBMLogs(
            this LoggerSinkConfiguration sinkConfiguration,
            string ingestUrl,
            string apiKey,
            string applicationName,
            string subsystemName,
            string computerName = null,
            long? queueLimitBytes = null,
            int? logEventsInBatchLimit = null,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

            if (computerName == null) computerName = Dns.GetHostName().ToLower();

            return sinkConfiguration.Http(
                requestUri: ingestUrl,
                queueLimitBytes: queueLimitBytes,
                logEventsInBatchLimit: logEventsInBatchLimit ?? 100,
                batchSizeLimitBytes: 1000000, // max 2MB per request, extra characters for json not included 
                period: period,
                textFormatter: new IBMLogsTextFormatter(applicationName, subsystemName, computerName),
                batchFormatter: new ArrayBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new IBMLogsHttpClient(apiKey));
        }


        public static LoggerConfiguration DurableIBMLogs(
            this LoggerSinkConfiguration sinkConfiguration,
            string ingestUrl,
            string apiKey,
            string applicationName,
            string subsystemName,
            string computerName = null,
            long? bufferFileSizeLimitBytes = null,
            int? logEventsInBatchLimit = null,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

            if (computerName == null) computerName = Dns.GetHostName().ToLower();

            return sinkConfiguration.DurableHttpUsingFileSizeRolledBuffers(
                requestUri: ingestUrl,
                bufferBaseFileName: "ibmlogs-buffer",
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                logEventsInBatchLimit: logEventsInBatchLimit ?? 100,
                batchSizeLimitBytes: 1000000, // max 2MB per request, extra characters for json not included 
                period: period,
                textFormatter: new IBMLogsTextFormatter(applicationName, subsystemName, computerName),
                batchFormatter: new ArrayBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new IBMLogsHttpClient(apiKey));
        }
    }
}
