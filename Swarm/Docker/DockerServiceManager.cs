using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace ColiseeSharp.Swarm.Docker {
    public class DockerServiceManager : IDockerIdentifiable, IDisposable {
        private readonly IDockerClient _docker;

        public string Id { get; }

        public DockerServiceManager(IDockerClient docker, string serviceId) {
            this._docker = docker;
            this.Id = serviceId;
        }

        public Task Remove(CancellationToken cancellationToken = default) {
            return this._docker.Swarm.RemoveServiceAsync(this.Id, cancellationToken);
        }

        public Task<SwarmService> Inspect(CancellationToken cancellationToken = default) {
            return this._docker.Swarm.InspectServiceAsync(this.Id, cancellationToken);
        }

        public async Task<IEnumerable<TaskResponse>> ListTasks(CancellationToken cancellationToken = default) {
            IList<TaskResponse> allTasks = await this._docker.Tasks.ListAsync(cancellationToken).ConfigureAwait(false);
            return allTasks.Where(task => task.ServiceID == this.Id);
        }

        public void Dispose() {
            // TODO: Change this to run asynchronously - may need to wait for .NET Core 3.0's IAsyncDisposable + support from Ninject
            this.Dispose(true);
        }

        ~DockerServiceManager() {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Remove().Wait();
            }
        }
    }
}