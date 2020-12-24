// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using StitcherBoy.Logging;

namespace StitcherBoy.Utility
{
    /// <summary>
    ///     Allows to run the task as task or application (to debug)
    /// </summary>
    /// <typeparam name="TActualProgram">The type of the actual program.</typeparam>
    public abstract class ApplicationTask<TActualProgram>
        where TActualProgram : ApplicationTask<TActualProgram>
    {
        /// <summary>
        ///     Gets the logging.
        /// </summary>
        /// <value>
        ///     The logging.
        /// </value>
        protected ILogging Logging { get; private set; }

        /// <summary>
        ///     Gets the wrapped task path.
        ///     This is used when debugging inline task.
        ///     The task is named "*.task", so we call "*"
        /// </summary>
        /// <returns></returns>
        private string GetWrappedTaskPath()
        {
            var thisPath = GetType().Assembly.Location;
            var wrappedTaskPath = thisPath + ".debugTask";
            if (File.Exists(wrappedTaskPath))
                return wrappedTaskPath;
            return null;
        }

        private IEnumerable<string> GetPropertiesArguments()
        {
            foreach (var property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(string)))
            {
                var propertyValue = (string) property.GetValue(this, new object[0]);
                if (propertyValue != null)
                    yield return $"\"{property.Name}={propertyValue}\"";
            }
        }

        /// <summary>
        ///     Target task entry point
        /// </summary>
        /// <returns>
        ///     true for success
        /// </returns>
        public virtual bool Execute()
        {
            Logging = new TaskLogging(this);

            var wrappedTaskPath = GetWrappedTaskPath();
            // see if the task is just a stub, which is the case if we have a wrapped task
            // (this allows to build and debug)
            if (wrappedTaskPath == null)
                return Run(false);

            // run the application as a command-line application
            var process = new Process
            {
                StartInfo =
                {
                    FileName = wrappedTaskPath,
                    WorkingDirectory = Environment.CurrentDirectory,
                    Arguments = string.Join(" ", GetPropertiesArguments()),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                }
            };
            process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    Logging.Write(e.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        /// <summary>
        ///     Runs the task (either launched as task or application).
        /// </summary>
        /// <param name="fromExe"></param>
        /// <returns></returns>
        protected abstract bool Run(bool fromExe);

        private static Tuple<string, string> GetArgument(string arg)
        {
            arg = arg.TrimQuotes();
            var equalsIndex = arg.IndexOf('=');
            if (equalsIndex < 0)
                return null;
            var propertyName = arg.Substring(0, equalsIndex);
            var propertyValue = arg.Substring(equalsIndex + 1).TrimQuotes();
            return Tuple.Create(propertyName, propertyValue);
        }

        /// <summary>
        ///     Runs the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        protected static int Run(ApplicationTask<TActualProgram> instance, string[] args)
        {
            instance.Logging = new ConsoleLogging();
            foreach (var arg in LoadArgs(args))
            {
                var argument = GetArgument(arg);
                if (argument != null)
                {
                    var propertyInfo = instance.GetType().GetProperty(argument.Item1);
                    propertyInfo?.SetValue(instance, argument.Item2, new object[0]);
                }
            }

            return instance.Run(true) ? 0 : 1;
        }

        /// <summary>
        ///     Loads the arguments.
        ///     If there is only one argument and its name starts with "@", it contains arguments itself
        ///     If a + is after the @, then the first line contains a directory (and yes, this is convenient for debugging)
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private static IList<string> LoadArgs(string[] args)
        {
            if (args.Length != 1)
                return args;

            var arg = args[0];
            if (!PopPrefix(ref arg, "@"))
                return args;

            var setDirectory = PopPrefix(ref arg, "+");

            using (var fileStream = File.OpenText(arg))
            {
                if (setDirectory)
                {
                    var cwd = fileStream.ReadLine();
                    Directory.SetCurrentDirectory(cwd);
                }

                var lines = new List<string>();
                for (;;)
                {
                    var line = fileStream.ReadLine();
                    if (line == null)
                        break;
                    if (line == "")
                        continue;
                    lines.AddRange(line.SplitArguments());
                }

                return lines;
            }
        }

        /// <summary>
        ///     If the given string has the requested prefix, returns true and removed the prefix from the string
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