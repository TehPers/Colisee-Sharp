using ColiseeSharp.Api.Bindings;
using ColiseeSharp.Api.Extensions;
using ColiseeSharp.Swarm.Bindings;
using Ninject.Extensions.Factory;
using Ninject.Modules;

namespace ColiseeSharp.Bindings {
    internal class ArenaModule : ConfiguredModule {
        protected override void LoadShared() {
            this.Kernel?.Load(new INinjectModule[] {
                new LoggingModule(),
                new SwarmModule(),
                new FuncModule()
            });

            this.BindRuntimeMode<HelpRuntimeMode>("help");
        }

        protected override void LoadDebug() {

        }

        protected override void LoadRelease() {

        }
    }
}
