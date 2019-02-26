using System.Threading;
using System.Threading.Tasks;
using ColiseeSharp.Api;
using ColiseeSharp.Swarm.Matches;

namespace ColiseeSharp.Swarm.Console {
    public interface IConsoleCommand : IVerb {
        /// <summary>Executes the current console command.</summary>
        /// <param name="runner"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Execute(IMatchRunner runner, CancellationToken cancellationToken = default);
    }
}