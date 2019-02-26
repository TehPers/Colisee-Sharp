using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ColiseeSharp.Api.Logging;

namespace ColiseeSharp.Logging {
    internal abstract class AsyncLogWriter : ILogWriter, IDisposable {
        private readonly ConcurrentQueue<LogMessage> _messages;
        private bool _disposed = false;

        protected AsyncLogWriter() {
            this._messages = new ConcurrentQueue<LogMessage>();

            // Start consuming messages
            Task.Run(this.Consume);
        }

        private async Task Consume() {
            while (!this._disposed) {
                // Try to grab a message
                if (this._messages.TryDequeue(out LogMessage message)) {
                    // Write the message
                    await this.WriteAsync(message).ConfigureAwait(false);
                }
            }
        }

        public void Write(string message, LogLevel severity) {
            LogMessage logMessage = new LogMessage(message, severity);

            // Check if the message should be logged
            if (!this.ShouldLog(logMessage)) {
                return;
            }

            // Queue the message
            this._messages.Enqueue(logMessage);
        }

        public void Write(string message, Exception exception, LogLevel severity) {
            this.Write(string.IsNullOrEmpty(message) ? exception.ToString() : $"{message}: {exception}", severity);
        }

        /// <summary>Checks if a message should be written or not.</summary>
        /// <param name="message">The message to write.</param>
        /// <returns>True if the message should be writen, false otherwise.</returns>
        protected abstract bool ShouldLog(LogMessage message);

        /// <summary>Writes a message to the output.</summary>
        /// <param name="message">The message to write.</param>
        protected abstract Task WriteAsync(LogMessage message);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases managed and unmanaged resources.</summary>
        /// <param name="disposing">True if this object is being disposed, false if it's being finalized.</param>
        protected virtual void Dispose(bool disposing) {
            this._disposed = true;
        }

        protected readonly struct LogMessage {
            public string Message { get; }
            public LogLevel Severity { get; }
            public DateTime UtcTime { get; }

            public LogMessage(string message, LogLevel severity) {
                this.Message = message;
                this.Severity = severity;
                this.UtcTime = DateTime.UtcNow;
            }

            public override string ToString() {
                return $"[{this.UtcTime:yyyy-MM-dd HH:mm:ss.ff}] [{this.Severity}] {this.Message}";
            }
        }
    }
}