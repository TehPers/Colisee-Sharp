using System;

namespace ColiseeSharp.Api.Logging {
    public interface ILogWriter {
        /// <summary>Writes a log message to some form of output.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="severity">The severity of the message.</param>
        void Write(string message, LogLevel severity);

        /// <summary>Writes a log message to some form of output.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="exception">The exception to write.</param>
        /// <param name="severity">The severity of the message.</param>
        void Write(string message, Exception exception, LogLevel severity);
    }
}