using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace ColiseeSharp.Swarm.Docker {
    public static class DockerExtensions {
        public static async Task<DockerServiceManager> CreateServiceAsync(this IDockerClient docker, ServiceCreateParameters parameters, CancellationToken cancellationToken = default) {
            ServiceCreateResponse result = await docker.Swarm.CreateServiceAsync(parameters, cancellationToken).ConfigureAwait(false);
            return new DockerServiceManager(docker, result.ID);
        }
    }
}