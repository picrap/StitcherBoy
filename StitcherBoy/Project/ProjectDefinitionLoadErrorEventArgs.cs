#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Project
{
    using System;
    /// <summary>
    /// Event args for project load error
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ProjectDefinitionLoadErrorEventArgs : EventArgs
    {
        private Exception Exception { get; }
        private ProjectDefinition ProjectDefinition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDefinitionLoadErrorEventArgs"/> class.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="projectDefinition">The project definition.</param>
        public ProjectDefinitionLoadErrorEventArgs(Exception e, ProjectDefinition projectDefinition)
        {
            Exception = e;
            ProjectDefinition = projectDefinition;
        }
    }
}
