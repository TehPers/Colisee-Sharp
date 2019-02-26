using System.Threading.Tasks;

namespace ColiseeSharp.Api {
    public interface IRuntimeMode : IVerb {
        /// <summary>Executes this runtime mode.</summary>
        /// <param name="remainingArgs">The remaining runtime arguments.</param>
        Task Execute(string[] remainingArgs);
    }
}
