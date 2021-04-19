
namespace StitcherBoy.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Arguments helper
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// Injects the specified arguments.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="o">The o.</param>
        /// <param name="args">The arguments.</param>
        public static void Inject<TObject>(this TObject o, string[] args)
        {
            Inject(o, args.ParseArguments());
        }

        /// <summary>
        /// Injects the specified arguments.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="o">The o.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Inject<TObject>(this TObject o, IDictionary<string, string> arguments)
        {
            foreach (var propertyInfo in typeof(TObject).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (propertyInfo.PropertyType != typeof(string))
                    continue;
                if (arguments.TryGetValue(propertyInfo.Name, out var value))
                    propertyInfo.SetValue(o, value);
            }
        }

        /// <summary>
        /// Parses the arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static IDictionary<string, string> ParseArguments(this string[] args)
        {
            return LoadArguments(args)
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.InvariantCultureIgnoreCase);
        }

        private static IEnumerable<KeyValuePair<string, string>> LoadArguments(string[] args)
        {
            return LoadAllArguments(args).Select(GetArgument);
        }

        private static KeyValuePair<string, string> GetArgument(string arg)
        {
            arg = arg.TrimQuotes();
            var equalsIndex = arg.IndexOf('=');
            if (equalsIndex < 0)
                return new(arg, "");
            var propertyName = arg.Substring(0, equalsIndex);
            var propertyValue = arg.Substring(equalsIndex + 1).TrimQuotes();
            return new(propertyName, propertyValue);
        }

        /// <summary>
        /// Loads the arguments.
        /// If there is only one argument and its name starts with "@", it contains arguments itself
        /// If a + is after the @, then the first line contains a directory (and yes, this is convenient for debugging)
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private static IEnumerable<string> LoadAllArguments(string[] args)
        {
            if (args.Length != 1)
                return args;

            var arg = args[0];
            if (!PopPrefix(ref arg, "@"))
                return args;

            var setDirectory = PopPrefix(ref arg, "+");

            using var fileStream = File.OpenText(arg);
            if (setDirectory)
            {
                var cwd = fileStream.ReadLine();
                Directory.SetCurrentDirectory(cwd);
            }
            var lines = new List<string>();
            for (; ; )
            {
                var line = fileStream.ReadLine();
                if (line is null)
                    break;
                if (line == "")
                    continue;
                lines.AddRange(line.SplitArguments());
            }
            return lines;
        }

        /// <summary>
        /// If the given string has the requested prefix, returns true and removed the prefix from the string
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        private static bool PopPrefix(ref string a, string prefix)
        {
            if (!a.StartsWith(prefix))
                return false;

            a = a.Substring(prefix.Length);
            return true;
        }
    }
}
