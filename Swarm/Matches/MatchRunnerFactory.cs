using System.Threading;
using System.Threading.Tasks;
using ColiseeSharp.Api.Logging;
using ColiseeSharp.Swarm.Docker;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace ColiseeSharp.Swarm.Matches {
    public class MatchRunnerFactory {
        private readonly ILogger _logger;

        public MatchRunnerFactory(ILogger logger) {
            this._logger = logger;
        }

        /// <summary>Creates and initializes a match runner.</summary>
        /// <param name="docker">The docker client.</param>
        /// <param name="registry">The registry configuration to use when pushing client images. This is necessary to allow worker nodes to run the clients.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public async Task<IMatchRunner> Create(IDockerClient docker, DockerRegistryConfiguration registry, CancellationToken cancellationToken = default) {
            // Verify that this is a swarm manager
            cancellationToken.ThrowIfCancellationRequested();
            SwarmInspectResponse swarmInfo = await docker.Swarm.InspectSwarmAsync(cancellationToken).ConfigureAwait(false);
            this._logger.Log($"Swarm ID: {swarmInfo.ID}", LogLevel.Debug);

            // Create the match runner
            return new MatchRunner(docker, registry, this._logger);
        }
    }
}