// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
namespace StitcherBoy.Project
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Build.Evaluation;

    /// <summary>
    /// Reference to assembly
    /// </summary>
    [DebuggerDisplay("{Literal} / GAC={Gac}")]
    public class AssemblyReference : IReferences
    {
        /// <summary>
        /// Gets the project item.
        /// </summary>
        /// <value>
        /// The project item or null for indirect references.
        /// </value>
        public ProjectItem ProjectItem { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public AssemblyName Name { get; }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is private (copy local).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrivate { get; }

        private Assembly _assembly;

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        public Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                    _assembly = Resolve();
                return _assembly;
            }
        }

        private AssemblyName _assemblyName;

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public AssemblyName AssemblyName
        {
            get
            {
                if (Assembly == null)
                    return null;
                if (_assemblyName == null)
                {
                    _assemblyName = Assembly.GetName();
                    if (Path == null)
                        Path = Assembly.Location;
                }
                return _assemblyName;
            }
        }

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
                if (Assembly == null)
                    return null;
                if (_references == null)
                    _references = Assembly.GetReferencedAssemblies().Select(a => new AssemblyReference(a, false, null));
                return _references;
            }
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
                return Assembly.GetName().GetPublicKey().Length > 0;
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
                if (Path != null)
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
        /// <param name="path">The path.</param>
        /// <param name="isPrivate">if set to <c>true</c> [is private].</param>
        /// <param name="projectItem">The project item.</param>
        public AssemblyReference(string path, bool isPrivate, ProjectItem projectItem)
        {
            Path = path;
            IsPrivate = isPrivate;
            ProjectItem = projectItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReference" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isPrivate">if set to <c>true</c> [is private].</param>
        /// <param name="projectItem">The project item.</param>
        public AssemblyReference(AssemblyName name, bool isPrivate, ProjectItem projectItem)
        {
            Name = name;
            IsPrivate = isPrivate;
            ProjectItem = projectItem;
        }

        /// <summary>
        /// Resolves the assembly (and loads it).
        /// </summary>
        /// <returns></returns>
        private Assembly Resolve()
        {
            return ResolveName() ?? ResolvePath();
        }

        /// <summary>
        /// Resolves by path.
        /// </summary>
        /// <returns></returns>
        private Assembly ResolvePath()
        {
            if (Path == null)
                return null;
            var absolutePath = System.IO.Path.GetFullPath(Path);
            Path = absolutePath;
            return SafeLoad(() => Assembly.ReflectionOnlyLoad(File.ReadAllBytes(absolutePath)));
        }

        /// <summary>
        /// Resolves by name.
        /// </summary>
        /// <returns></returns>
        private Assembly ResolveName()
        {
            if (Name == null)
                return null;
            return SafeLoad(() => Assembly.ReflectionOnlyLoad(Name.FullName))
#pragma warning disable 618
 ?? SafeLoad(() => Assembly.LoadWithPartialName(Name.ToString()));
#pragma warning restore 618
        }

        /// <summary>
        /// Safe loading of assembly (catches exceptions and returns null).
        /// </summary>
        /// <param name="loader">The loader.</param>
        /// <returns></returns>
        private static Assembly SafeLoad(Func<Assembly> loader)
        {
            try
            {
                return loader();
            }
            catch (FileLoadException)
            { }
            catch (FileNotFoundException)
            { }
            catch (BadImageFormatException)
            { }
            return null;
        }
    }
}
