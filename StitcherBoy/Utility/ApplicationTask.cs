// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Logging;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// Allows to run the task as task or application (to debug)
    /// </summary>
    /// <typeparam name="TActualProgram">The type of the actual program.</typeparam>
    public abstract class ApplicationTask<TActualProgram> : Task
            where TActualProgram : ApplicationTask<TActualProgram>
    {
        /// <summary>
        /// Gets the logging.
        /// </summary>
        /// <value>
        /// The logging.
        /// </value>
        protected ILogging Logging { get; private set; }

        private const string BuildIDKey = "StitcherBoy.BuildID";

        /// <summary>
        /// Gets the build identifier.
        /// </summary>
        /// <value>
        /// The build identifier.
        /// </value>
        protected Guid BuildID
        {
            get
            {
                var buildIDObject = BuildEngine4.GetRegisteredTaskObject(BuildIDKey, RegisteredTaskObjectLifetime.Build);
                if (buildIDObject != null)
                    return (Guid)buildIDObject;
                return BuildID = Guid.NewGuid();
            }
            set
            {
                BuildEngine4.RegisterTaskObject(BuildIDKey, value, RegisteredTaskObjectLifetime.Build, false);
            }
        }

        private const string BuildDateKey = "StitcherBoy.BuildDate";

        /// <summary>
        /// Gets or sets the build date.
        /// </summary>
        /// <value>
        /// The build date.
        /// </value>
        protected DateTime BuildDate
        {
            get
            {
                var buildDateObject = BuildEngine4.GetRegisteredTaskObject(BuildDateKey, RegisteredTaskObjectLifetime.Build);
                if (buildDateObject != null)
                    return (DateTime)buildDateObject;
                return BuildDate = DateTime.UtcNow;
            }
            set
            {
                BuildEngine4.RegisterTaskObject(BuildDateKey, value, RegisteredTaskObjectLifetime.Build, false);
            }
        }

        /// <summary>
        /// Gets the wrapped task path.
        /// This is used when debugging inline task.
        /// The task is named "*.task", so we call "*"
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
            foreach (var property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType == typeof(string)))
            {
                var propertyValue = (string)property.GetValue(this, new object[0]);
                if (propertyValue != null)
                    yield return $"\"{property.Name}={propertyValue}\"";
            }
        }

        /// <summary>
        /// Target task entry point
        /// </summary>
        /// <returns>
        /// true for success
        /// </returns>
        public override bool Execute()
        {
            Logging = new TaskLogging(this);

            var wrappedTaskPath = GetWrappedTaskPath();
            // see if the task is just a stub, which is the case if we have a wrapped task
            // (this allows to build and debug)
            if (wrappedTaskPath == null)
                return Run();

            // run the application as a command-line application
            var process = new Process
            {
                StartInfo =
                {
                    FileName = wrappedTaskPath,
                    Arguments = string.Join(" ",GetPropertiesArguments()),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                }
            };
            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
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
        /// Runs the task (either launched as task or application).
        /// </summary>
        protected abstract bool Run();

        private static Tuple<string, string> GetArgument(string arg)
        {
            if (arg.StartsWith("\"") && arg.EndsWith("\""))
                arg = arg.Substring(1, arg.Length - 2);
            var equalsIndex = arg.IndexOf('=');
            if (equalsIndex < 0)
                return null;
            var propertyName = arg.Substring(0, equalsIndex);
            var propertyValue = arg.Substring(equalsIndex + 1);
            return Tuple.Create(propertyName, propertyValue);
        }

        /// <summary>
        /// Runs the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        protected static int Run(ApplicationTask<TActualProgram> instance, string[] args)
        {
            instance.Logging = new ConsoleLogging();
            foreach (var arg in args)
            {
                var argument = GetArgument(arg);
                if (argument != null)
                {
                    var propertyInfo = instance.GetType().GetProperty(argument.Item1);
                    propertyInfo?.SetValue(instance, argument.Item2, new object[0]);
                }
            }
            return instance.Run() ? 0 : 1;
        }
    }
}
