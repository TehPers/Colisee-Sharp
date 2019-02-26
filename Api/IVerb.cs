using NDesk.Options;

namespace ColiseeSharp.Api {
    public interface IVerb {
        /// <summary>The description of this verb.</summary>
        string Description { get; }

        /// <summary>The options that should be accepted.</summary>
        OptionSet Options { get; }
    }
}