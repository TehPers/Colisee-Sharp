using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ColiseeSharp.Api.Logging;
using ColiseeSharp.Swarm.Docker;
using Docker.DotNet;
using Docker.DotNet.Models;
using Ninject.Infrastructure.Language;

namespace ColiseeSharp.Swarm.Matches {
    public class MatchRunner : IMatchRunner {
        private readonly IDockerClient _docker;
        private readonly DockerRegistryConfiguration _registry;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _disposeTokenSource;

        private long _curSessionId = 0;

        public MatchRunner(IDockerClient docker, DockerRegistryConfiguration registry, ILogger logger) {
            this._docker = docker;
            this._registry = registry;
            this._logger = logger;
            this._disposeTokenSource = new CancellationTokenSource();
        }

        public async Task<IMatchResult> RunMatch(IEnumerable<IGameClient> clients, string game, Uri gameServer, CancellationToken cancellationToken = default) {
            string session = $"arena-{Interlocked.Increment(ref this._curSessionId)}";

            // Create a linked token that is cancelled whenever either this object is disposed or the provided token is cancelled
            using (CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(this._disposeTokenSource.Token, cancellationToken)) {
                IGameClient[] clientsArray = clients?.ToArray();
                if (clientsArray?.Any() != true) {
                    throw new ArgumentException("At least one client is required to run a match.", nameof(clients));
                }

                List<DockerServiceManager> clientServices = new List<DockerServiceManager>();

                // Create the service for the first client
                DockerServiceManager p1 = await this.CreateClientService(clientsArray[0], game, gameServer, session, tokenSource.Token);
                clientServices.Add(p1);

                // Get the node the client is running on
                IList<TaskResponse> v = await this._docker.Tasks.ListAsync(tokenSource.Token);
                foreach (TaskResponse task in v) {
                    this._logger.Log($"Task: {task.Name}, service: {task.ServiceID}, node: {task.NodeID}", LogLevel.Debug);
                }
                TaskResponse p1Task = v.SingleOrDefault(task => task.ServiceID == p1.Id);
                string p1Node = p1Task.NodeID;
                this._logger.Log($"Running match on node {p1Node} between {string.Join(", ", clientsArray.Select(client => client.Name))}", LogLevel.Info);

                // Start the rest of the players
                foreach (IGameClient client in clientsArray) {
                    clientServices.Add(await this.CreateClientService(client, game, gameServer, session, tokenSource.Token));
                }

                // Wait until all clients have finished, checking every 30 seconds
                while (!await this.CheckIfAllStopped(clientServices, tokenSource.Token)) {
                    await Task.Delay(TimeSpan.FromSeconds(30), tokenSource.Token);
                }
            }

            return default;
        }

        private async Task<bool> CheckIfAllStopped(IEnumerable<DockerServiceManager> services, CancellationToken cancellationToken) {
            foreach (DockerServiceManager service in services) {
                // Check each task within each service to see if the task is complete
                IEnumerable<TaskResponse> tasks = await service.ListTasks(cancellationToken);
                if (tasks.Any(task => task.DesiredState != TaskState.Shutdown || task.Status.State != TaskState.Shutdown)) {
                    return false;
                }
            }

            // All tasks are stopped
            return true;
        }

        private async Task<DockerServiceManager> CreateClientService(IGameClient client, string game, Uri server, string session, CancellationToken cancellationToken) {
            // Get the remote image name for the current image
            string localTag = $"{client.Name}:latest";
            string remoteTag = this._registry.GetRemoteImageName(localTag);

            // Build the image for the client
            this._logger.Log($"Building new client image: {remoteTag}", LogLevel.Debug);
            cancellationToken.ThrowIfCancellationRequested();
            Stream image = await this._docker.Images.BuildImageFromDockerfileAsync(client.Source, new ImageBuildParameters {
                Tags = new List<string> { remoteTag },
                Labels = new Dictionary<string, string> {
                    { "arena.client.name", client.Name }
                },
                Dockerfile = "joueur-cs/Dockerfile"
            }, cancellationToken);

            // Create the image
            // this._logger.Log($"Creating client image: {remoteTag}", LogLevel.Debug);
            // cancellationToken.ThrowIfCancellationRequested();
            // await this._docker.Images.CreateImageAsync(new ImagesCreateParameters {
            //     Repo = remoteTag
            // }, image, null, new Progress<JSONMessage>(message => this._logger.Log($"Progress on creating {remoteTag}: {message.ProgressMessage}", LogLevel.Debug)), cancellationToken);

            // Push the new image to the registry
            this._logger.Log($"Pushing client image {localTag} to {this._registry.ServerUri}", LogLevel.Debug);
            cancellationToken.ThrowIfCancellationRequested();
            await this._docker.Images.PushImageAsync(remoteTag, new ImagePushParameters {
                Tag = localTag
            }, new AuthConfig {
                ServerAddress = this._registry.ServerUri.ToString()
            }, null, cancellationToken);

            // Create the service for the current client
            this._logger.Log($"Creating service for {remoteTag}", LogLevel.Debug);
            cancellationToken.ThrowIfCancellationRequested();
            return await this._docker.CreateServiceAsync(new ServiceCreateParameters {
                Service = new ServiceSpec {
                    EndpointSpec = new EndpointSpec {
                        Ports = new List<PortConfig> {
                            new PortConfig {
                                PublishedPort = 3080,
                                TargetPort = 3080
                            }
                        }
                    },
                    TaskTemplate = new TaskSpec {
                        ContainerSpec = new ContainerSpec {
                            Image = remoteTag,
                            Args = { game, "-s", server.Host, "-p", server.Port.ToString(), "-r", session }
                        },
                        RestartPolicy = new SwarmRestartPolicy {
                            Condition = "none",
                            MaxAttempts = 0
                        }
                    },
                    Labels = new Dictionary<string, string> {
                        { "arena.client.name", client.Name }
                    }
                }
            }, cancellationToken);
        }

        public void Dispose() {
            this._disposeTokenSource.Cancel();
            this._disposeTokenSource.Dispose();
        }
    }
}