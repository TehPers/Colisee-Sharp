using System.IO;
using ColiseeSharp.Api.Bindings;
using ColiseeSharp.Api.Logging;
using ColiseeSharp.Logging;

namespace ColiseeSharp.Bindings {
    public sealed class LoggingModule : ConfiguredModule {

        protected override void LoadShared() {
            this.Bind<ILogWriter>().To<ConsoleLogWriter>().InSingletonScope();
            this.Bind<ILogWriter>().To<FileLogWriter>().InSingletonScope();
            this.Bind<ILogger>().To<Logger>().InSingletonScope();
        }

        protected override void LoadDebug() {
            // Console
            this.Bind<LogLevel>().ToConstant(LogLevel.Debug).WhenInjectedInto<ConsoleLogWriter>();

            // Log file
            this.Bind<LogLevel>().ToConstant(LogLevel.Trace).WhenInjectedInto<FileLogWriter>();
            this.Bind<string>().ToConstant(Path.Combine(Directory.GetCurrentDirectory(), "log-debug.txt")).WhenInjectedInto<FileLogWriter>().Named("Output");
        }

        protected override void LoadRelease() {
            // Console
            this.Bind<LogLevel>().ToConstant(LogLevel.Info).WhenInjectedInto<ConsoleLogWriter>();

            // Log file
            this.Bind<LogLevel>().ToConstant(LogLevel.Trace).WhenInjectedInto<FileLogWriter>();
            this.Bind<string>().ToConstant(Path.Combine(Directory.GetCurrentDirectory(), "log.txt")).WhenInjectedInto<FileLogWriter>().Named("Output");
        }
    }
}