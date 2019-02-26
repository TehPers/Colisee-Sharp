using System;

namespace ColiseeSharp.Api.Logging {
    public interface ILogger {
        /// <summary>Logs a message, writing it to each <see cref="ILogWriter"/>.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="severity">The severity of the message.</param>
        void Log(string message, LogLevel severity);

        /// <summary>Logs a message, writing it to each <see cref="ILogWriter"/>.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="severity">The severity of the message.</param>
        void Log(string message, Exception exception, LogLevel severity);
    }
}