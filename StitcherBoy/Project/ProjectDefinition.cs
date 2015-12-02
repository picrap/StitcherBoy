// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Project
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
    public class ProjectDefinition : IReferences
    {
        private readonly Project _project;
        private readonly string _projectDirectory;

        private IEnumerable<AssemblyReference> _references;
        /// <summary>
        /// Gets the references.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        public IEnumerable<AssemblyReference> References
        {
            get
            {
                if (_references == null)
                    _references = LoadReferences().ToArray();
                return _references;
            }
        }

        /// <summary>
        /// Gets the target path.
        /// </summary>
        /// <value>
        /// The target path.
        /// </value>
        public string TargetPath => Path.Combine(_projectDirectory, GetProperty("TargetPath"));

        /// <summary>
        /// Gets the intermediate path.
        /// </summary>
        /// <value>
        /// The intermediate path.
        /// </value>
        public string IntermediatePath => Path.Combine(_projectDirectory, GetProperty("IntermediateOutputPath"), GetProperty("TargetFileName"));

        /// <summary>
        /// Gets or sets the properties keys.
        /// </summary>
        /// <value>
        /// The properties keys.
        /// </value>
        public string[] PropertiesKeys => _project.Properties.Select(p => p.Name).ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDefinition"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public ProjectDefinition(string path)
        {
            _projectDirectory = Path.GetDirectoryName(path);
            using (var projectReader = File.OpenText(path))
            using (var xmlReader = new XmlTextReader(projectReader))
                _project = new Project(xmlReader);
        }

        /// <summary>
        /// Loads the references.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<AssemblyReference> LoadReferences()
        {
            var references = _project.Items.Where(i => i.ItemType == "Reference").ToArray();
            foreach (var reference in references)
            {
                var hintPath = reference.Metadata.SingleOrDefault(m => m.Name == "HintPath");
                if (hintPath != null)
                {
                    var fullPath = Path.Combine(_projectDirectory, hintPath.EvaluatedValue);
                    yield return new AssemblyReference(fullPath);
                }
                else
                {
                    var assemblyName = new AssemblyName(reference.EvaluatedInclude);
                    yield return new AssemblyReference(assemblyName);
                }
            }
            var projectReferences = _project.Items.Where(i => i.ItemType == "ProjectReference").ToArray();
            foreach (var projectReference in projectReferences)
            {
                var projectPath = Path.Combine(_projectDirectory, projectReference.EvaluatedInclude);
                var referencedProject = new ProjectDefinition(projectPath);
                var targetPath = referencedProject.TargetPath;
                yield return new AssemblyReference(targetPath);
            }
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetProperty(string key)
        {
            var property = _project.Properties.SingleOrDefault(p => p.Name == key);
            return property?.EvaluatedValue;
        }

        /// <summary>
        /// Gets the bool property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool? GetBoolProperty(string key)
        {
            var value = GetProperty(key);
            if (value == null)
                return null;
            bool b;
            if (!bool.TryParse(value, out b))
                return null;
            return b;
        }
    }
}
