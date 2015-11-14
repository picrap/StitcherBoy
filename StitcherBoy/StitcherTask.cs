
using System;
using Microsoft.Build.Framework;
using StitcherBoy.Logging;
using StitcherBoy.Utility;
using StitcherBoy.Weaving;

// ReSharper disable once CheckNamespace
public abstract class StitcherTask<TSingleStitcher> : ApplicationTask<StitcherTask<TSingleStitcher>>
    where TSingleStitcher : SingleStitcher
{
    /// <summary>
    /// Gets or sets the project path (this is injected in the task).
    /// </summary>
    /// <value>
    /// The project path.
    /// </value>
    [Required]
    public string ProjectPath { get; set; }

    /// <summary>
    /// Gets or sets the assembly path (injected by task).
    /// </summary>
    /// <value>
    /// The assembly path.
    /// </value>
    public string AssemblyPath { set; get; }

    /// <summary>
    /// Gets or sets the solution path (also injected by task).
    /// </summary>
    /// <value>
    /// The solution path.
    /// </value>
    public string SolutionPath { get; set; }

    protected override bool Run()
    {
        // the weaver runs isolated, since it it is going to load other modules
        using (var taskAppDomain = new DisposableAppDomain("StitcherBoy"))
        {
            try
            {
                var stitcher = taskAppDomain.AppDomain.CreateInstanceAndUnwrap<TSingleStitcher>();
                stitcher.Logging = new RemoteLogging(Logging);
                return stitcher.Process(AssemblyPath, ProjectPath, SolutionPath);
            }
            catch (Exception e)
            {
                Logging.WriteError("Unhandled exception: {0}", e);
            }
        }
        return false;
    }
}
