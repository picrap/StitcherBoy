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
    using Logging;

    /// <summary>
    /// Allows to handle a <see cref="ModuleDef"/>
    /// (I personally dislike the "Manager" word which was way overrused, but this time I think the name is appropriate)
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ModuleManager : IDisposable
    {
        private readonly string _assemblyPath;
        private readonly bool _usePdb;
        private string _pdbPath;
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
        /// <param name="logger">The logger.</param>
        public ModuleManager(string assemblyPath, bool usePdb, bool? useTemp = true, ILogging logger = null)
        {
            _assemblyPath = assemblyPath;
            _usePdb = usePdb;

            Module = LoadDirect(assemblyPath, useTemp) ?? LoadWithTemp(assemblyPath, useTemp);
            LoadPdb(assemblyPath);
        }

        private void LoadPdb(string assemblyPath)
        {
            if (_usePdb)
            {
                var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
                if (File.Exists(pdbPath))
                {
                    _pdbPath = Path.GetFullPath(pdbPath);
                    var pdbBytes = File.ReadAllBytes(_pdbPath);

#if dnlibOldVersion
                    if (pdbBytes[0] == 0x42 && pdbBytes[1] == 0x53 && pdbBytes[2] == 0x4A && pdbBytes[3] == 0x42)
                    {
                        logger?.WriteError("Debug info is using portable format which is unsupported now. Please upvote the issue at https://github.com/0xd4d/dnlib/issues/128");
                        _usePdb = false;
                        return;
                    }
#endif
                    // no need to call it anymore, that dnlib mofo already loads it
                    //Module.LoadPdb(pdbBytes);
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
            var moduleWriterOptions = CreateModuleWriter();
            //moduleWriterOptions.PdbOptions = PdbWriterOptions.NoOldDiaSymReader | PdbWriterOptions.NoDiaSymReader;
            moduleWriterOptions.WritePdb = _usePdb;
            moduleWriterOptions.PdbFileName = _pdbPath;
            WriteModule(SetWriterOptions(assemblyOriginatorKeyFile, moduleWriterOptions));
        }

        /// <summary>
        /// Writes the module, using the given options.
        /// </summary>
        /// <param name="moduleWriterOptionsBase">The module writer options base.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void WriteModule(ModuleWriterOptionsBase moduleWriterOptionsBase)
        {
            if (moduleWriterOptionsBase is ModuleWriterOptions moduleWriterOptions)
                Module.Write(_assemblyPath, moduleWriterOptions);
            else if (moduleWriterOptionsBase is NativeModuleWriterOptions nativeModuleWriterOptions)
                Module.NativeWrite(_assemblyPath, nativeModuleWriterOptions);
            else
                throw new InvalidOperationException();
        }

        private ModuleWriterOptionsBase CreateModuleWriter()
        {
            if (Module.IsILOnly)
                return new ModuleWriterOptions(Module);
            return new NativeModuleWriterOptions(Module, true);
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
