using System;

namespace ColiseeSharp.Swarm.Docker {
    public class DockerRegistryConfiguration {
        /// <summary>The <see cref="Uri"/> for the registry server.</summary>
        public Uri ServerUri { get; }

        public DockerRegistryConfiguration(Uri serverUri) {
            this.ServerUri = serverUri;
        }

        /// <summary>Gets the registry's name for a given repository, given that it is on that registry.</summary>
        /// <param name="repository">The name of the repository, plus an optional tag.</param>
        /// <returns>The remote name of the repository</returns>
        public string GetRemoteImageName(string repository) {
            return $"{this.ServerUri}/{repository}";
        }
    }
}