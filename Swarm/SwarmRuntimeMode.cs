using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ColiseeSharp.Api;
using ColiseeSharp.Api.Logging;
using ColiseeSharp.Swarm.Docker;
using ColiseeSharp.Swarm.Matches;
using Docker.DotNet;
using NDesk.Options;

namespace ColiseeSharp.Swarm {
    internal class SwarmRuntimeMode : IRuntimeMode, IDisposable {
        private static readonly Uri _nullUri = new Uri("null://null");

        private readonly ILogger _logger;
        private readonly MatchRunnerFactory _matchRunnerFactory;
        private DockerClientConfiguration _dockerConfiguration;

        public string Description { get; }
        public OptionSet Options { get; }

        public SwarmRuntimeMode(ILogger logger, MatchRunnerFactory matchRunnerFactory) {
            this._logger = logger;
            this._matchRunnerFactory = matchRunnerFactory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                this._dockerConfiguration = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                this._dockerConfiguration = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                this._dockerConfiguration = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));
            } else {
                this._dockerConfiguration = new DockerClientConfiguration(SwarmRuntimeMode._nullUri);
            }

            this.Description = "Runs the arena in swarm mode, allowing many worker nodes to work together to run games.";

            // Define options for this runtime mode
            this.Options = new OptionSet();
            this.Options.Add<string>("e|docker-endpoint=", "The docker endpoint to use.", v => {
                if (!Uri.TryCreate(v, UriKind.Absolute, out Uri endpoint)) {
                    throw new ArgumentException($"'{v}' is not a valid URI");
                }

                this._dockerConfiguration = new DockerClientConfiguration(endpoint, this._dockerConfiguration.Credentials, this._dockerConfiguration.DefaultTimeout);
            });
            this.Options.Add<double?>("t|docker-timeout=", "The default timeout for docker operations, in milliseconds.", v => {
                if (!(v is double milliseconds && v > 0)) {
                    throw new ArgumentException($"'{v}' is not a valid number of milliseconds");
                }

                this._dockerConfiguration = new DockerClientConfiguration(this._dockerConfiguration.EndpointBaseUri, this._dockerConfiguration.Credentials, TimeSpan.FromMilliseconds(milliseconds));
            });
        }

        public async Task Execute(string[] remainingArgs) {
            if (this._dockerConfiguration.EndpointBaseUri == SwarmRuntimeMode._nullUri) {
                throw new InvalidOperationException("Unable to determine the docker endpoint for this computer. Use --docker-endpoint to specify the docker endpoint to use.");
            }

            using (DockerClient docker = this._dockerConfiguration.CreateClient()) {
                this._logger.Log("Creating match runner", LogLevel.Debug);
                IMatchRunner matchRunner = await this._matchRunnerFactory.Create(docker, new DockerRegistryConfiguration(new Uri("localhost:5000")));

                using (MemoryStream m1 = new MemoryStream())
                using (MemoryStream m2 = new MemoryStream()) {
                    // Copy first file into memory
                    using (FileStream f1 = File.OpenRead("../joueur-cs.tar")) {
                        f1.CopyTo(m1);
                    }

                    // Copy second file into memory
                    using (FileStream f2 = File.OpenRead("../joueur-cs2.tar")) {
                        f2.CopyTo(m2);
                    }

                    // Reset memory stream positions
                    m1.Position = 0;
                    m2.Position = 0;

                    using (FileStream output = File.OpenWrite("./debug.tar")) {
                        m1.CopyTo(output);
                    }

                    // Reset memory stream positions
                    m1.Position = 0;
                    m2.Position = 0;

                    this._logger.Log("Creating clients", LogLevel.Debug);
                    IGameClient[] clients = {
                        new GameClient("player1", m1),
                        new GameClient("player2", m2)
                    };

                    this._logger.Log("Running sample match", LogLevel.Debug);
                    Uri address = new UriBuilder{Host = "172.17.0.1", Port = 3000}.Uri;
                    await matchRunner.RunMatch(clients, "Newtonian",address);
                }
            }
        }

        public void Dispose() {
            this._dockerConfiguration?.Dispose();
        }
    }
}
