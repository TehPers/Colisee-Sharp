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
using ICSharpCode.SharpZipLib.Tar; 

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
            this._logger.Log(gameServer.Port.ToString(), LogLevel.Debug);

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

                this._logger.Log(clientsArray.Count().ToString(), LogLevel.Debug);
                // Start the rest of the players
                foreach (IGameClient client in clientsArray)
                    this._logger.Log(client.Name, LogLevel.Debug);

                foreach (IGameClient client in clientsArray) {
                    this._logger.Log(client.Name, LogLevel.Debug);
                    if(client == p1) continue;
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
            // This is just building an image and returning a stream of that image I think.
            this._logger.Log($"Building new client image: {remoteTag}", LogLevel.Debug);
            cancellationToken.ThrowIfCancellationRequested();
            Stream image = await this._docker.Images.BuildImageFromDockerfileAsync(client.Source, new ImageBuildParameters {
                Tags = new List<string> { remoteTag },
                Labels = new Dictionary<string, string> {
                    { "arena.client.name", client.Name }
                },
            }, cancellationToken);
            
            // Compressing the image given back from the build to a .tar so we can pass it back to docker.
            this._logger.Log($"Compressing", LogLevel.Debug);
            FileStream imageFile = File.Create("./client");
            image.CopyTo(imageFile);
            imageFile.Close();
            Stream outImage = new MemoryStream();
            TarOutputStream tarImageStream = new TarOutputStream(outImage);
            TarEntry tarItem = TarEntry.CreateEntryFromFile("./client");
            tarImageStream.PutNextEntry(tarItem);

            // Create the image
            // Creating the image so that it actually shows up as a docker image.
            // If you do docker image ls -a the image should now show.
            // FromSrc = "-" means from the stream we give.
            // Tag is the tag we want it to have.
            this._logger.Log($"Creating client image: {remoteTag}", LogLevel.Debug);
            cancellationToken.ThrowIfCancellationRequested();
            await this._docker.Images.CreateImageAsync(new ImagesCreateParameters {
                FromSrc = "-",
                Tag = "latest"
            }, outImage, null, new Progress<JSONMessage>(message => this._logger.Log($"Progress on creating {remoteTag}: {message.ProgressMessage}", LogLevel.Debug)), cancellationToken);
            // Clean up the temp file I created for doing the wierd shit to make it work.
            File.Delete("./client");

            // Push the new image to the registry
            this._logger.Log($"Pushing client image {localTag} to {this._registry.ServerUri}", LogLevel.Debug);
            cancellationToken.ThrowIfCancellationRequested();
            // The name should be repo/name.
            // Then we have the tag of it in the params
            await this._docker.Images.PushImageAsync($"{this._registry.ServerUri}/{client.Name}", new ImagePushParameters {
                Tag = "latest"
            }, new AuthConfig {
                ServerAddress = this._registry.ServerUri.ToString()
            }, new Progress<JSONMessage>(message => this._logger.Log($"Progress on creating {remoteTag}: {message.ProgressMessage}", LogLevel.Debug)), cancellationToken);

            // Create the service for the current client
            this._logger.Log($"Creating service for {remoteTag}", LogLevel.Debug);
            cancellationToken.ThrowIfCancellationRequested();
            return await this._docker.CreateServiceAsync(
                new ServiceCreateParameters {
                    Service = new ServiceSpec {
                        Name = session + "-" +client.Name,
                        Labels = new Dictionary<string, string> {
                            {"arena.client.name", client.Name}
                        },
                        TaskTemplate = new TaskSpec {
                            Networks = new List<NetworkAttachmentConfig>() {
                                new NetworkAttachmentConfig{Target = "arena_arena"}
                            },
                            ContainerSpec = new ContainerSpec {
                                Image = remoteTag,
                                Args = new List<string>{ game, "-s", server.Host, "-p", server.Port.ToString(), "-r", session }
                            },
                            RestartPolicy = new SwarmRestartPolicy {
                                Condition = "none",
                                MaxAttempts = 0
                            }
                        },
                    }
                }, cancellationToken);
        }

        public void Dispose() {
            this._disposeTokenSource.Cancel();
            this._disposeTokenSource.Dispose();
        }
    }
}