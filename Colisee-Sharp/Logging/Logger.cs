using System;
using ColiseeSharp.Api.Logging;

namespace ColiseeSharp.Logging {
    public class Logger : ILogger {
        private readonly ILogWriter[] _logWriters;

        public Logger(ILogWriter[] logWriters) {
            this._logWriters = logWriters;
        }

        public void Log(string message, LogLevel severity) {
            foreach (ILogWriter logWriter in this._logWriters) {
                logWriter.Write(message, severity);
            }
        }

        public void Log(string message, Exception exception, LogLevel severity) {
            foreach (ILogWriter logWriter in this._logWriters) {
                logWriter.Write(message, exception, severity);
            }
        }
    }
}