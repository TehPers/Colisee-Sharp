using ColiseeSharp.Api.Bindings;
using ColiseeSharp.Api.Extensions;
using ColiseeSharp.Swarm.Console;
using ColiseeSharp.Swarm.Matches;

namespace ColiseeSharp.Swarm.Bindings {
    public class SwarmModule : ConfiguredModule {
        protected override void LoadShared() {
            this.BindRuntimeMode<SwarmRuntimeMode>("swarm");

            this.Bind<MatchRunnerFactory>().To<MatchRunnerFactory>().InSingletonScope();
            this.Bind<IConsoleListener>().To<ConsoleListener>().InSingletonScope();
        }

        protected override void LoadDebug() {

        }

        protected override void LoadRelease() {

        }
    }
}