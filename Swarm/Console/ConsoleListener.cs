using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ColiseeSharp.Api.Bindings;
using ColiseeSharp.Api.Logging;
using ColiseeSharp.Swarm.Matches;

namespace ColiseeSharp.Swarm.Console {
    internal class ConsoleListener : IConsoleListener {
        private readonly Dictionary<string, IConsoleCommand> _commands;
        private readonly ILogger _logger;

        public ConsoleListener(IEnumerable<INamedBinding<IConsoleCommand>> commands, ILogger logger) {
            this._commands = commands.ToDictionary(command => command.Name, command => command.Value, StringComparer.OrdinalIgnoreCase);
            this._logger = logger;
        }

        public async Task ListenForCommands(IMatchRunner runner, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested) {
                if (!(System.Console.ReadLine() is string inputLine)) {
                    break;
                }

                string[] commandParts = inputLine.Split(' ');
                string cmdName = commandParts[0];
                IEnumerable<string> cmdArgs = commandParts.Skip(1);

                // Try to get the command with that name
                if (!this._commands.TryGetValue(cmdName, out IConsoleCommand cmd)) {
                    this._logger.Log($"Unknown command: {cmdName}", LogLevel.Error);
                    continue;
                }

                // Parse and execute the command
                _ = cmd.Options.Parse(cmdArgs);

                try {
                    await cmd.Execute(runner, cancellationToken).ConfigureAwait(false);
                } catch (Exception ex) {
                    this._logger.Log($"An error occured while executing {cmdName}", ex, LogLevel.Error);
                }
            }
        }
    }
}
