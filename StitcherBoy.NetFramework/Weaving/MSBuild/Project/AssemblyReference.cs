// Stitcher Boy - a small library to help building post-build tasks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Weaving.MSBuild.Project
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using Microsoft.Build.Evaluation;

    /// <summary>
    /// Reference to assembly
    /// </summary>
    [DebuggerDisplay("{Literal} / GAC={Gac}")]
    public class AssemblyReference : IReferences
    {
        private readonly IAssemblyResolver _assemblyResolver;

        /// <summary>
        /// Gets the project item.
        /// </summary>
        /// <value>
        /// The project item or null for indirect references.
        /// </value>
        public ProjectItem ProjectItem { get; }

        /// <summary>
        /// Gets the project definition.
        /// </summary>
        /// <value>
        /// The project definition.
        /// </value>
        public ProjectDefinition ProjectDefinition { get; }

        /// <summary>
        /// Gets the name.
        /// May be null
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public AssemblyName Name { get; }

        /// <summary>
        /// Gets the path.
        /// May be null
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is private (copy local).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrivate { get; }

        private AssemblyDef _assembly;

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        public AssemblyDef Assembly
        {
            get
            {
                if (_assembly == null)
                    _assembly = Resolve();
                return _assembly;
            }
        }

        /// <summary>
        /// Gets the load exception.
        /// </summary>
        /// <value>
        /// The load exception.
        /// </value>
        public Exception AssemblyLoadException { get; private set; }

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
                if (ProjectDefinition != null)
                    return ProjectDefinition.References;
                if (Assembly == null)
                    return null;
                if (_references == null)
                    _references = Assembly.ManifestModule.GetAssemblyRefs().Select(CreateReference);
                return _references;
            }
        }

        private AssemblyReference CreateReference(AssemblyRef assemblyRef)
        {
            return new AssemblyReference(_assemblyResolver, new AssemblyName(assemblyRef.RealFullName), false, null);
        }

        /// <summary>
        /// Indicates wheter the assembly is signed.
        /// </summary>
        /// <value>
        /// The is signed.
        /// </value>
        public bool? IsSigned
        {
            get
            {
                if (Assembly == null)
                    return null;
                return Assembly.PublicKey?.Data != null && Assembly.PublicKey.Data.Length > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AssemblyReference"/> is in GAC.
        /// Currently, we suppose this when no path and no version are specifed
        /// </summary>
        /// <value>
        ///   <c>true</c> if gac; otherwise, <c>false</c>.
        /// </value>
        public bool Gac
        {
            get
            {
                if (Name == null)
                    return false;
                if (Name.Version != null)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Gets the literal.
        /// </summary>
        /// <value>
        /// The literal.
        /// </value>
        private string Literal => Path ?? Name.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReference" /> class.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="path">The path.</param>
        /// <param name="isPrivate">if set to <c>true</c> [is private].</param>
        /// <param name="projectItem">The project item.</param>
        public AssemblyReference(IAssemblyResolver assemblyResolver, string path, bool isPrivate, ProjectItem projectItem)
        {
            _assemblyResolver = assemblyResolver;
            Path = System.IO.Path.GetFullPath(path);
            IsPrivate = isPrivate;
            ProjectItem = projectItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReference" /> class.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="path">The path.</param>
        /// <param name="isPrivate">if set to <c>true</c> [is private].</param>
        /// <param name="projectDefinition">The project definition.</param>
        public AssemblyReference(IAssemblyResolver assemblyResolver, string path, bool isPrivate, ProjectDefinition projectDefinition)
        {
            _assemblyResolver = assemblyResolver;
            Path = System.IO.Path.GetFullPath(path);
            IsPrivate = isPrivate;
            ProjectDefinition = projectDefinition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReference" /> class.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="name">The name.</param>
        /// <param name="isPrivate">if set to <c>true</c> [is private].</param>
        /// <param name="projectItem">The project item.</param>
        public AssemblyReference(IAssemblyResolver assemblyResolver, AssemblyName name, bool isPrivate, ProjectItem projectItem)
        {
            _assemblyResolver = assemblyResolver;
            Name = name;
            var assembly = assemblyResolver.Resolve(name, null);
            if (assembly != null)
                Path = assembly.ManifestModule.Location;
            IsPrivate = isPrivate;
            ProjectItem = projectItem;
        }

        /// <summary>
        /// Resolves the assembly (and loads it).
        /// </summary>
        /// <returns></returns>
        private AssemblyDef Resolve()
        {
            return ResolveName() ?? ResolvePath();
        }

        /// <summary>
        /// Resolves by path.
        /// </summary>
        /// <returns></returns>
        private AssemblyDef ResolvePath()
        {
            if (Path == null)
                return null;
            return SafeLoad(() => AssemblyDef.Load(File.ReadAllBytes(Path)));
        }

        /// <summary>
        /// Resolves by name.
        /// </summary>
        /// <returns></returns>
        private AssemblyDef ResolveName()
        {
            if (Name == null)
                return null;
            return SafeLoad(() => _assemblyResolver.Resolve(Name, null));
        }

        /// <summary>
        /// Safe loading of assembly (catches exceptions and returns null).
        /// </summary>
        /// <param name="loader">The loader.</param>
        /// <returns></returns>
        private AssemblyDef SafeLoad(Func<AssemblyDef> loader)
        {
            AssemblyLoadException = null;
            try
            {
                return loader();
            }
            catch (FileLoadException e)
            {
                AssemblyLoadException = e;
            }
            catch (FileNotFoundException e)
            {
                AssemblyLoadException = e;
            }
            catch (BadImageFormatException e)
            {
                AssemblyLoadException = e;
            }
            return null;
        }
    }
}
