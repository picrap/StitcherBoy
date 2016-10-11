#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Weaving
{
    using System;
    using System.Reflection;
    using MSBuild;

    /// <summary>
    /// Compatibility
    /// </summary>
    /// <seealso cref="StitcherBoy.Weaving.MSBuild.ProjectStitcher" />
    [Obsolete("Use ProjectStitcher instead")]
    public abstract class SingleStitcher : ProjectStitcher
    {
        /// <summary>
        /// Processes the specified module.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected override bool Process(ProjectStitcherContext context)
        {
            var stitcherContext = new StitcherContext();
            foreach (var fieldInfo in context.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                fieldInfo.SetValue(stitcherContext, fieldInfo.GetValue(context));
            return Process(stitcherContext);
        }

        /// <summary>
        /// Processes the assembly from context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected abstract bool Process(StitcherContext context);
    }

    /// <summary>
    /// <see cref="SingleStitcher"/> context.
    /// </summary>
    /// <seealso cref="StitcherBoy.Weaving.MSBuild.ProjectStitcherContext" />
    public class StitcherContext : ProjectStitcherContext
    { }
}
