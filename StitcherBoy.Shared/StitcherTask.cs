// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT

using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using StitcherBoy.Logging;
using StitcherBoy.Utility;
using StitcherBoy.Weaving;

// ReSharper disable once CheckNamespace
/// <summary>
/// Base task for stitchers
/// </summary>
/// <typeparam name="TSingleStitcher">The type of the single stitcher.</typeparam>
public abstract class StitcherTask<TSingleStitcher> : ApplicationTask<StitcherTask<TSingleStitcher>>
    where TSingleStitcher : IStitcher
{
    /// <summary>
    /// Gets or sets the project path (this is injected in the task).
    /// </summary>
    /// <value>
    /// The project path.
    /// </value>
    public string ProjectPath { get; set; }

    /// <summary>
    /// Gets or sets the assembly path (injected by task).
    /// </summary>
    /// <value>
    /// The assembly path.
    /// </value>
    public string AssemblyPath { set; get; }

    /// <summary>
    /// Gets or sets the reference path.
    /// </summary>
    /// <value>
    /// The reference path.
    /// </value>
    public string ReferencePath { set; get; }

    /// <summary>
    /// Gets or sets the reference copy local paths.
    /// </summary>
    /// <value>
    /// The reference copy local paths.
    /// </value>
    public string ReferenceCopyLocalPaths { set; get; }

    /// <summary>
    /// Gets or sets the assembly snk path.
    /// </summary>
    /// <value>
    /// The assembly snk file path.
    /// </value>
    public string AssemblyOriginatorKeyFile { set; get; }

    /// <summary>
    /// Indicates if the assembly has to be signedd.
    /// </summary>
    /// <value>
    /// The sign assembly.
    /// </value>
    public string SignAssembly { set; get; }

    /// <summary>
    /// Gets or sets the solution path (also injected by task).
    /// </summary>
    /// <value>
    /// The solution path.
    /// </value>
    public string SolutionPath { get; set; }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public string Configuration { get; set; }

    /// <summary>
    /// Gets or sets the platform.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public string Platform { get; set; }

    /// <summary>
    /// Runs the task (either launched as task or application).
    /// </summary>
    /// <param name="fromExe"></param>
    /// <returns></returns>
    protected override bool Run(bool fromExe)
    {
        try
        {
            var parameters = new StringDictionary();
            foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).Where(p => p.PropertyType == typeof(string)))
                parameters[propertyInfo.Name] = (string)propertyInfo.GetValue(this);

            // when run from a .debugTask exe, there is no need to wrap in a separate AppDomain
            if (fromExe)
            {
                var singleStitcher = (IStitcher)Activator.CreateInstance(typeof(TSingleStitcher));
                singleStitcher.Logging = Logging;
                return singleStitcher.Process(parameters, BuildID, BuildTime, GetType().Assembly.Location);
            }

            // the weaver runs isolated, since it it is going to load other modules
            var type = typeof(TSingleStitcher);
            var assemblyPath = type.Assembly.Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            var thisAssemblyBytes = File.ReadAllBytes(assemblyPath);
            using (var taskAppDomain = new DisposableAppDomain("StitcherBoy", assemblyDirectory))
            {
                var sticherProcessor = taskAppDomain.AppDomain.CreateInstanceAndUnwrap<StitcherProcessor>();
                taskAppDomain.AppDomain.Load(thisAssemblyBytes);
                sticherProcessor.Logging = new RemoteLogging(Logging);
                sticherProcessor.Load(type.FullName);
                return sticherProcessor.Process(parameters, BuildID, BuildTime, GetType().Assembly.Location);
            }
        }
        catch (Exception e)
        {
            Logging.WriteError("Unhandled exception: {0}", e);
        }
        return false;
    }
}
