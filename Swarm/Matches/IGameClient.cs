using System.IO;

namespace ColiseeSharp.Swarm.Matches {
    public interface IGameClient {
        /// <summary>The name of the player.</summary>
        string Name { get; }

        /// <summary>A <see cref="Stream"/> containing the entire client compressed into a single .tar archive, with the Dockerfile at the root of the archive.</summary>
        Stream Source { get; }
    }

    public class GameClient : IGameClient {
        public string Name { get; }
        public Stream Source { get; }

        public GameClient(string name, Stream source) {
            this.Name = name;
            this.Source = source;
        }
    }
}