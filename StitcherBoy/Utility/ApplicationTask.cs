// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Logging;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Weaving.MSBuild.Extensions;

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

        /// <summary>
        /// Gets the instance of the <see cref="T:Microsoft.Build.Framework.IBuildEngine4" /> object used by the task.
        /// This override because the property <see cref="BuildEngine4"/> is not implemented on xbuild
        /// </summary>
        protected new IBuildEngine4 BuildEngine4 => BuildEngine as IBuildEngine4;

        private bool? _hasBuildEngine;

        /// <summary>
        /// Gets a value indicating whether this instance has build engine.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has build engine; otherwise, <c>false</c>.
        /// </value>
        protected bool HasBuildEngine
        {
            get
            {
                if (!_hasBuildEngine.HasValue)
                {
                    try
                    {
                        _hasBuildEngine = BuildEngine4 != null;
                    }
                    catch
                    {
                        _hasBuildEngine = false;
                    }
                }
                return _hasBuildEngine.Value;
            }
        }

        private Guid? _buildID;

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
                if (!_buildID.HasValue)
                    _buildID = HasBuildEngine ? BuildEngine4.GetRegisteredTaskObject("StitcherBoy.BuildID", Guid.NewGuid) : Guid.Empty;
                return _buildID.Value;
            }
            set { _buildID = value; }
        }

        /// <summary>
        /// Gets or sets the literal build identifier.
        /// </summary>
        /// <value>
        /// The literal build identifier.
        /// </value>
        [Obsolete("Serialization-only property. Use BuildID instead.")]
        public string LiteralBuildID
        {
            get { return BuildID.ToString(); }
            set { BuildID = Guid.Parse(value); }
        }

        private DateTime? _buildDate;

        /// <summary>
        /// Gets or sets the build date.
        /// </summary>
        /// <value>
        /// The build date.
        /// </value>
        protected DateTime BuildTime
        {
            get
            {
                if (!_buildDate.HasValue)
                    _buildDate = HasBuildEngine ? BuildEngine4.GetRegisteredTaskObject("StitcherBoy.BuildTime", () => DateTime.UtcNow) : DateTime.UtcNow;
                return _buildDate.Value;
            }
            set { _buildDate = value; }
        }

        /// <summary>
        /// Gets or sets the literal build date.
        /// </summary>
        /// <value>
        /// The literal build date.
        /// </value>
        [Obsolete("Serialization-only property. Use BuildTime instead.")]
        public string LiteralBuildTime
        {
            get { return BuildTime.ToString("s", CultureInfo.InvariantCulture); }
            set { BuildTime = DateTime.ParseExact(value, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal); }
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
        /// <param name="fromExe"></param>
        /// <returns></returns>
        protected abstract bool Run(bool fromExe);

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
            return instance.Run(true) ? 0 : 1;
        }
    }
}
