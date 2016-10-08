// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Project
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using dnlib.DotNet;
    using Microsoft.Build.Evaluation;

    /// <summary>
    /// Wraps a project (from Microsoft.Build)
    /// </summary>
    public class ProjectDefinition : IReferences
    {
        /// <summary>
        /// Gets the build project.
        /// </summary>
        /// <value>
        /// The build project.
        /// </value>
        public Project Project { get; }

        private readonly string _projectDirectory;
        private readonly string _outputDirectory;

        private readonly IDictionary<string, string> _globalProperties;
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly ProjectCollection _projectCollection;

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
        /// Gets the name of the target.
        /// </summary>
        /// <value>
        /// The name of the target.
        /// </value>
        public string TargetName => Path.Combine(_projectDirectory, GetProperty("TargetName"));

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
        public string[] PropertiesKeys => Project?.Properties.Select(p => p.Name).ToArray();

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        /// <value>
        /// The project path.
        /// </value>
        public string ProjectPath { get; }

        /// <summary>
        /// Occurs when [load error].
        /// </summary>
        public static event EventHandler<ProjectDefinitionLoadErrorEventArgs> LoadError;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDefinition" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="globalProperties">The global properties.</param>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="projectCollection">The project collection.</param>
        public ProjectDefinition(string path, string outputDirectory = null, IDictionary<string, string> globalProperties = null, IAssemblyResolver assemblyResolver = null, ProjectCollection projectCollection = null)
        {
            ProjectPath = path;
            _assemblyResolver = assemblyResolver ?? new AssemblyResolver();
            _projectCollection = projectCollection;
            _projectDirectory = Path.GetDirectoryName(path);
            _globalProperties = globalProperties;
            try
            {
                Project = new Project(path, globalProperties, null, projectCollection);
            }
            catch (Exception e)
            {
                LoadError?.Invoke(this, new ProjectDefinitionLoadErrorEventArgs(e, this));
            }
            _outputDirectory = outputDirectory ?? Path.GetDirectoryName(TargetPath);
        }

        /// <summary>
        /// Loads the references.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<AssemblyReference> LoadReferences()
        {
            if (Project == null)
                yield break;

            var references = Project.Items.Where(i => i.ItemType == "Reference").ToArray();
            foreach (var reference in references)
            {
                var hintPath = reference.Metadata.SingleOrDefault(m => m.Name == "HintPath");
                if (hintPath != null)
                {
                    var fullPath = Path.Combine(_projectDirectory, hintPath.EvaluatedValue);
                    // when <HintPath> is provided, the assembly is private by default
                    yield return new AssemblyReference(_assemblyResolver, fullPath, IsPrivate(reference) ?? true, reference);
                }
                else
                {
                    var assemblyName = new AssemblyName(reference.EvaluatedInclude);
                    yield return new AssemblyReference(_assemblyResolver, assemblyName, IsPrivate(reference) ?? false, reference);
                }
            }
            var projectReferences = Project.Items.Where(i => i.ItemType == "ProjectReference").ToArray();
            foreach (var projectReference in projectReferences)
            {
                var isPrivate = IsPrivate(projectReference) ?? true;
                var projectPath = Path.Combine(_projectDirectory, projectReference.EvaluatedInclude);
                var referencedProject = new ProjectDefinition(projectPath, _outputDirectory, _globalProperties, _assemblyResolver, _projectCollection);
                if (referencedProject.Project == null)
                    continue;
                var targetPath = GetTargetPath(referencedProject, isPrivate);
                yield return new AssemblyReference(_assemblyResolver, targetPath, isPrivate, referencedProject);
            }
        }

        private string GetTargetPath(ProjectDefinition referencedProject, bool isPrivate)
        {
            if (!isPrivate)
                return referencedProject.TargetPath;

            var referenceFileName = Path.GetFileName(referencedProject.TargetPath);
            var targetPath = Path.Combine(_outputDirectory, referenceFileName);
            return targetPath;
        }

        private static bool? IsPrivate(ProjectItem item)
        {
            var isPrivateProperty = item.Metadata.SingleOrDefault(m => string.Equals(m.Name, "Private", StringComparison.InvariantCultureIgnoreCase));
            if (isPrivateProperty == null)
                return null;

            bool isPrivate;
            if (!bool.TryParse(isPrivateProperty.EvaluatedValue, out isPrivate))
                return null;

            return isPrivate;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetProperty(string key)
        {
            var property = Project?.Properties.SingleOrDefault(p => p.Name == key);
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
