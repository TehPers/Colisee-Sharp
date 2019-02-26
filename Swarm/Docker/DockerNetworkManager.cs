using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace ColiseeSharp.Swarm.Docker {
    public class DockerNetworkManager : IDockerIdentifiable, IDisposable {
        private readonly IDockerClient _docker;

        public string Id { get; }

        public DockerNetworkManager(IDockerClient docker, string networkId) {
            this._docker = docker;
            this.Id = networkId;
        }

        public Task Delete(CancellationToken cancellationToken = default) {
            return this._docker.Networks.DeleteNetworkAsync(this.Id, cancellationToken);
        }

        public Task<NetworkResponse> Inspect(CancellationToken cancellationToken = default) {
            return this._docker.Networks.InspectNetworkAsync(this.Id, cancellationToken);
        }

        public void Dispose() {
            // TODO: Change this to run asynchronously - may need to wait for .NET Core 3.0's IAsyncDisposable + support from Ninject
            this.Dispose(true);
        }

        ~DockerNetworkManager() {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Delete().Wait();
            }
        }
    }
}
