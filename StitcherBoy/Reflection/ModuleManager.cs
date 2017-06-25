#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoy.Reflection
{
    using System;
    using System.IO;
    using dnlib.DotNet;
    using dnlib.DotNet.Pdb;
    using dnlib.DotNet.Writer;

    /// <summary>
    /// Allows to handle a <see cref="ModuleDef"/>
    /// (I personally dislike the "Manager" word which was way overrused, but this time I think the name is appropriate)
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ModuleManager : IDisposable
    {
        private readonly string _assemblyPath;
        private readonly bool _usePdb;
        private readonly string _pdbPath;
        private string _tempAssemblyPath;

        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public ModuleDefMD Module { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleManager" /> class.
        /// </summary>
        /// <param name="assemblyPath">The path.</param>
        /// <param name="usePdb">if set to <c>true</c> [load PDB].</param>
        /// <param name="useTemp">if set to <c>true</c> [use temporary].</param>
        public ModuleManager(string assemblyPath, bool usePdb, bool? useTemp = true)
        {
            _assemblyPath = assemblyPath;
            _usePdb = usePdb;

            Module = LoadDirect(assemblyPath, useTemp) ?? LoadWithTemp(assemblyPath, useTemp);

            if (usePdb)
            {
                var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
                if (File.Exists(pdbPath))
                {
                    _pdbPath = Path.GetFullPath(pdbPath);
                    Module.LoadPdb(PdbImplType.Managed, File.ReadAllBytes(_pdbPath));
                }
            }
        }

        /// <summary>
        /// Loads the module directly from source.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="useTemp">The use temporary.</param>
        /// <returns></returns>
        private ModuleDefMD LoadDirect(string assemblyPath, bool? useTemp)
        {
            try
            {
                if (!(useTemp ?? false))
                    return ModuleDefMD.Load(assemblyPath);
            }
            catch (IOException)
            {
                // this occurs when module is opened somewhere else
            }
            return null;
        }

        /// <summary>
        /// Loads the module using a temporary file.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="useTemp">The use temporary.</param>
        /// <returns></returns>
        private ModuleDefMD LoadWithTemp(string assemblyPath, bool? useTemp)
        {
            if (useTemp ?? true)
            {
                _tempAssemblyPath = assemblyPath + ".2";
                File.Copy(assemblyPath, _tempAssemblyPath, true);
                return ModuleDefMD.Load(_tempAssemblyPath);
            }
            return null;
        }

        /// <summary>
        /// Writes the current module using optional SNK.
        /// </summary>
        /// <param name="assemblyOriginatorKeyFile">The assembly originator key file.</param>
        public void Write(string assemblyOriginatorKeyFile)
        {
            if (Module.IsILOnly)
            {
                var moduleWriterOptions = new ModuleWriterOptions(Module);
                moduleWriterOptions.WritePdb = _usePdb;
                moduleWriterOptions.PdbFileName = _pdbPath;
                Module.Write(_assemblyPath, SetWriterOptions(assemblyOriginatorKeyFile, moduleWriterOptions));
            }
            else
            {
                var nativeModuleWriterOptions = new NativeModuleWriterOptions(Module);
                nativeModuleWriterOptions.WritePdb = _usePdb;
                nativeModuleWriterOptions.PdbFileName = _pdbPath;
                Module.NativeWrite(_assemblyPath, SetWriterOptions(assemblyOriginatorKeyFile, nativeModuleWriterOptions));
            }
        }

        private TOptions SetWriterOptions<TOptions>(string snkPath, TOptions options)
            where TOptions : ModuleWriterOptionsBase
        {
            options.WritePdb = _usePdb;
            if (!string.IsNullOrEmpty(snkPath) && File.Exists(snkPath))
                options.StrongNameKey = new StrongNameKey(snkPath);
            return options;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Module.Dispose();
            if (_tempAssemblyPath != null)
                File.Delete(_tempAssemblyPath);
        }
    }
}
