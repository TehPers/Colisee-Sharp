namespace ColiseeSharp.Api.Bindings {
    public interface INamedBinding<out T> {
        string Name { get; }
        T Value { get; }
    }
}