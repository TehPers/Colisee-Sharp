using System.IO;
using ColiseeSharp.Api;
using NDesk.Options;

namespace ColiseeSharp.Extensions {
    internal static class UsageExtensions {
        /// <summary>Generates a description on how a given <see cref="IVerb"/> can be used.</summary>
        /// <param name="verb">The verb.</param>
        /// <param name="name">The name of the verb.</param>
        /// <returns>A description of how to use the verb.</returns>
        public static string GetUsage(this IVerb verb, string name) {
            return $"Usage: {name} [OPTIONS]+\n" +
                   "\n" +
                   "Options:\n" +
                   $"{verb.Options.GetUsage()}";
        }

        /// <summary>Generates a description on how a given <see cref="OptionSet"/> can be used.</summary>
        /// <param name="optionSet">The set of options.</param>
        /// <returns>A description of how to use the set of options.</returns>
        public static string GetUsage(this OptionSet optionSet) {
            using (StringWriter writer = new StringWriter()) {
                optionSet.WriteOptionDescriptions(writer);
                return writer.ToString();
            }
        }
    }
}
