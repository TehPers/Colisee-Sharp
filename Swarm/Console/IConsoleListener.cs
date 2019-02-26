using System.Threading;
using System.Threading.Tasks;
using ColiseeSharp.Swarm.Matches;

namespace ColiseeSharp.Swarm.Console {
    public interface IConsoleListener {
        Task ListenForCommands(IMatchRunner runner, CancellationToken cancellationToken = default);
    }
}