using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ColiseeSharp.Api;
using ColiseeSharp.Api.Bindings;
using ColiseeSharp.Api.Logging;
using ColiseeSharp.Bindings;
using Ninject;

namespace ColiseeSharp {
    internal class Program {
#if DEBUG
        private const bool DEBUG = true;
#else
        private const bool DEBUG = false;
#endif

        private static async Task<int> Main(string[] args) {
            Program.InvokeInDebug(() => Console.WriteLine("TheArena v2"));

            int result = await Program.TryInRelease(async () => {
                using (StandardKernel kernel = new StandardKernel(new ArenaModule())) {
                    // Additional bindings
                    kernel.Bind<bool>().ToConstant(Program.DEBUG).Named("Debug");

                    // Initialize the logger
                    ILogger logger = kernel.Get<ILogger>();
                    logger.Log("Kernel loaded", LogLevel.Debug);

                    // Get all possible runtime modes
                    INamedBinding<IRuntimeMode>[] runtimeModes = kernel.GetAll<INamedBinding<IRuntimeMode>>().ToArray();
                    logger.Log($"Available runtime modes: {string.Join(", ", runtimeModes.Select(mode => mode.Name))}", LogLevel.Debug);
                    logger.Log($"Selected runtime mode: {(args.Length > 0 ? args[0] : "none")}", LogLevel.Debug);

                    // Check program arguments
                    if (args.Length == 0) {
                        args = new[] { "help" };
                    }

                    // Try to execute the given runtime mode
                    if (args.Length == 0) {
                        if (kernel.TryGet<IRuntimeMode>("help") is IRuntimeMode helpMode) {
                            await helpMode.Execute(new string[0]).ConfigureAwait(false);
                        } else {
                            logger.Log("Unable to find a runtime mode named 'help'", LogLevel.Warn);
                        }
                    } else if (kernel.TryGet<IRuntimeMode>(args[0]) is IRuntimeMode runtimeMode) {
                        await runtimeMode.Execute(runtimeMode.Options.Parse(args.Skip(1)).ToArray()).ConfigureAwait(false);
                    } else {
                        throw new InvalidOperationException($"Unknown runtime mode '{args[0]}', use `help` for a list of possible runtime modes");
                    }
                }

                return 0;
            }, ex => {
                // Exception occured during initialization
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine("Exeception during initialization:");
                Console.WriteLine(ex.ToString());
                return 1;
            }).ConfigureAwait(false);

            Program.InvokeInDebug(() => {
                Console.WriteLine("Done");
                Console.ReadLine();
            });

            return result;
        }

        [Conditional("DEBUG")]
        private static void InvokeInDebug(Action action) {
            action();
        }

        private static async Task<T> TryInRelease<T>(Func<Task<T>> task, Func<Exception, T> onException) {
#if !DEBUG
            try {
#endif
            return await task().ConfigureAwait(true);
#if !DEBUG
            } catch (Exception ex) {
                return onException(ex);
            }
#endif
        }
    }
}
