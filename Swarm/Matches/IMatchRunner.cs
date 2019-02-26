using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ColiseeSharp.Swarm.Matches {
    public interface IMatchRunner : IDisposable {
        /// <summary>Runs a match with the given players.</summary>
        /// <param name="clients">The players to be matched against each other.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <param name="game">The name of the game to play.</param>
        /// <param name="gameServer">The <see cref="Uri"/> for the game server.</param>
        /// <returns>The result of the match.</returns>
        Task<IMatchResult> RunMatch(IEnumerable<IGameClient> clients, string game, Uri gameServer, CancellationToken cancellationToken = default);
    }
}
