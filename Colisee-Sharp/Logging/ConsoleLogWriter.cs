using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ColiseeSharp.Api.Logging;

namespace ColiseeSharp.Logging {
    internal class ConsoleLogWriter : AsyncLogWriter {
        private readonly LogLevel _minSeverity;
        private readonly TextWriter _output;
        private readonly ProxyConsoleWriter _consoleProxy;

        public ConsoleLogWriter(LogLevel minSeverity) {
            this._minSeverity = minSeverity;

            // Override console output
            this._output = Console.Out;
            this._consoleProxy = new ProxyConsoleWriter(this._output);
            this._consoleProxy.MessageIntercepted += this.ConsoleProxyOnMessageIntercepted;
            Console.SetOut(this._consoleProxy);
        }

        private void ConsoleProxyOnMessageIntercepted(string message) {
            this._output.WriteLine($"Console: {message}");
        }

        protected override bool ShouldLog(LogMessage message) {
            return message.Severity >= this._minSeverity;
        }

        protected override Task WriteAsync(LogMessage message) {
            ConsoleColor prevForeground = Console.ForegroundColor;
            Console.ForegroundColor = this.GetColorForLevel(message.Severity);
            this._output.WriteLine(message.ToString());
            Console.ForegroundColor = prevForeground;
            return Task.CompletedTask;
        }

        private ConsoleColor GetColorForLevel(LogLevel level) {
            switch (level) {
                case LogLevel.Trace:
                    return ConsoleColor.DarkGray;
                case LogLevel.Debug:
                    return ConsoleColor.Gray;
                case LogLevel.Info:
                    return ConsoleColor.White;
                case LogLevel.Warn:
                    return ConsoleColor.DarkYellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Critical:
                    return ConsoleColor.DarkRed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                // Restore console output
                Console.SetOut(this._output);
                this._consoleProxy.Dispose();
            }

            base.Dispose(disposing);
        }

        private class ProxyConsoleWriter : TextWriter {
            private readonly TextWriter _consoleWriter;
            public override Encoding Encoding => this._consoleWriter.Encoding;

            public ProxyConsoleWriter(TextWriter consoleWriter) {
                this._consoleWriter = consoleWriter;
            }

            public override void Write(char[] buffer, int index, int count) {
                this.MessageIntercepted?.Invoke(new string(buffer, index, count).TrimEnd('\r', '\n'));
            }

            public override void Write(char value) {
                this._consoleWriter.Write(value);
            }

            protected override void Dispose(bool disposing) {
                base.Dispose(disposing);

                // Release event handlers
                this.MessageIntercepted = null;
            }

            /// <summary>Invoked whenever a message is intercepted.</summary>
            public event Action<string> MessageIntercepted;
        }
    }
}