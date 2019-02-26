using System.Diagnostics;
using Ninject.Modules;

namespace ColiseeSharp.Api.Bindings {
    public abstract class ConfiguredModule : NinjectModule {
        public override void Load() {
            this.LoadShared();
            this.LoadDebug();
            this.LoadRelease();
        }

        /// <summary>Loads any bindings required on all build configurations.</summary>
        protected abstract void LoadShared();

        /// <summary>Loads any bindings required on the 'Debug' build configuration.</summary>
        [Conditional("DEBUG")]
        protected abstract void LoadDebug();

        /// <summary>Loads any bindings required on the 'Release' build configuration.</summary>
        [Conditional("RELEASE")]
        protected abstract void LoadRelease();
    }
}