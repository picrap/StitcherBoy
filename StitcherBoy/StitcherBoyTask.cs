#region SignReferences
// An automatic tool to presign unsigned dependencies
// https://github.com/picrap/SignReferences
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using BuildTasker;
using Microsoft.Build.Framework;
using SticherBoy;
using StitcherBoy;
using StitcherBoy.Utility;

/// <summary>
/// Main entry point for module.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once CheckNamespace
public class StitcherBoyTask : Tasker<StitcherBoyTask>
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
    /// Gets or sets the solution path (also injected by task).
    /// </summary>
    /// <value>
    /// The solution path.
    /// </value>
    public string SolutionPath { get; set; }

    /// <summary>
    /// Command-line entry point.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static void Main(string[] args) => Instance.Run(args);

    public override void Run()
    {
        AppDomainUtility.InvokeSeparated<Stitcher>(s => s.Run(ProjectPath));
    }
}
