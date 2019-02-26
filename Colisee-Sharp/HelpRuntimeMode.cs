using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColiseeSharp.Api;
using ColiseeSharp.Api.Bindings;
using ColiseeSharp.Api.Logging;
using ColiseeSharp.Extensions;
using NDesk.Options;

namespace ColiseeSharp {
    internal class HelpRuntimeMode : IRuntimeMode {
        private readonly ILogger _logger;
        private readonly IEnumerable<INamedBinding<IRuntimeMode>> _runtimeModes;
        private readonly List<string> _modes;

        public string Description { get; }
        public OptionSet Options { get; }

        public HelpRuntimeMode(ILogger logger, IEnumerable<INamedBinding<IRuntimeMode>> runtimeModes) {
            this._logger = logger;
            this._runtimeModes = runtimeModes;
            this._modes = new List<string>();

            this.Description = "Displays information regarding how this program can be executed.";
            this.Options = new OptionSet {
                { "m|mode=", "The runtime modes to get help for.", v => this._modes.Add(v) },
                { "<>", v => {
                    this._modes.Add(v);
                }}
            };
        }

        public Task Execute(string[] remainingArgs) {
            HashSet<string> modes = new HashSet<string>(this._modes?.Any() == true ? this._modes : this._runtimeModes.Select(mode => mode.Name));
            IEnumerable<string> modeUsages = this._runtimeModes.Where(mode => modes.Contains(mode.Name)).Select(mode => mode.Value.GetUsage(mode.Name));
            this._logger.Log($"Usage:\n{string.Join("\n-----\n\n", modeUsages)}", LogLevel.Info);

            return Task.CompletedTask;
        }
    }
}
