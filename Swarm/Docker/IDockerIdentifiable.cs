namespace ColiseeSharp.Swarm.Docker {
    public interface IDockerIdentifiable {
        /// <summary>The unique identifier string assigned by Docker.</summary>
        string Id { get; }
    }
}