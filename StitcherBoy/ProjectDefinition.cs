#region SignReferences
// An automatic tool to presign unsigned dependencies
// https://github.com/picrap/SignReferences
#endregion
namespace SticherBoy
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using Microsoft.Build.Evaluation;

    /// <summary>
    /// Wraps a project (from Microsoft.Build)
    /// </summary>
    public class ProjectDefinition
    {
        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public Project Project { get; }

        public string TargetPath => GetEvaluatedProperty("TargetPath")?.EvaluatedValue;

        private readonly string _projectPath;

        /// <summary>
        /// Gets the evaluated property by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ProjectProperty GetEvaluatedProperty(string name)
        {
            return Project.AllEvaluatedProperties.FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDefinition"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public ProjectDefinition(string path)
        {
            _projectPath = Path.GetDirectoryName(path);
            using (var projectReader = File.OpenText(path))
            using (var xmlReader = new XmlTextReader(projectReader))
                Project = new Project(xmlReader);
        }
    }
}
