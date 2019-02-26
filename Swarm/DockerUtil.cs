using System;
using System.Threading.Tasks;

namespace ColiseeSharp.Swarm {
    internal static class DockerUtil {
        public static async Task DoWithCleanup<T>(Func<Task<T>> setup, Func<T, Task> body, Func<T, Task> cleanup) {
            T item = await setup().ConfigureAwait(false);

            try {
                await body(item);
            } finally {
                await cleanup(item).ConfigureAwait(false);
            }
        }
    }
}