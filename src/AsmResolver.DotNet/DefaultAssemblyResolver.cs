using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a default implementation of an assembly resolver, that includes the global assembly cache (GAC), as well
    /// as any custom search directories.
    /// </summary>
    public class DefaultAssemblyResolver : IAssemblyResolver
    {
        private static readonly string[] BinaryFileExtensions = {".dll", ".exe"};

        private readonly IDictionary<AssemblyDescriptor, AssemblyDefinition> _cache 
            = new Dictionary<AssemblyDescriptor, AssemblyDefinition>();
        
        /// <summary>
        /// Creates a new default assembly resolver.
        /// </summary>
        public DefaultAssemblyResolver()
        {
            DetectDotNetCoreSearchDirectories();
            DetectGacDirectories();
        }
        
        /// <summary>
        /// Gets a collection of custom search directories that are probed upon resolving a reference
        /// to an assembly.
        /// </summary>
        public IList<string> SearchDirectories
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Gets a collection of global assembly cache (GAC) directories that are probed upon resolving a reference
        /// to an assembly.
        /// </summary>
        public ICollection<GacDirectory> GacDirectories
        {
            get;
        } = new List<GacDirectory>();

        /// <inheritdoc />
        public AssemblyDefinition Resolve(AssemblyDescriptor assembly)
        {
            if (_cache.TryGetValue(assembly, out var assemblyDef))
                return assemblyDef;
            
            string path = null;
            
            if (assembly.GetPublicKeyToken()!= null)
                path = ProbeGlobalAssemblyCache(assembly);
            if (string.IsNullOrEmpty(path))
                path = ProbeSearchDirectories(assembly);

            try
            {
                assemblyDef = LoadAssemblyFromFile(path);
                _cache[assembly] = assemblyDef;
            }
            catch
            {
                // ignore any errors.
            }

            return assemblyDef;
        }

        private void DetectDotNetCoreSearchDirectories()
        {
            if (RuntimeInformation.FrameworkDescription.Contains("Core"))
            {
                string installationDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
                SearchDirectories.Add(installationDirectory);
            }
        }

        private void DetectGacDirectories()
        {
            if (Type.GetType("Mono.Runtime") == null)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    DetectWindowsGacDirectories();
            }
            else
            {
                // TODO: mono GAC support
            }
        }

        private void DetectWindowsGacDirectories()
        {
            string systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            
            string windowsGac = Path.Combine(systemRoot, "assembly");
            AddGacDirectories(windowsGac, null);
            
            string frameworkGac = Path.Combine(systemRoot, "Microsoft.NET", "assembly");
            AddGacDirectories(frameworkGac, "v4.0_");
        }

        private void AddGacDirectories(string windowsGac, string prefix)
        {
            if (!Directory.Exists(windowsGac))
                return;
            
            foreach (string directory in Directory.GetDirectories(windowsGac))
            {
                string name = Path.GetFileName(directory);
                if (name.StartsWith("GAC"))
                    GacDirectories.Add(new GacDirectory(directory, prefix));
            }
        }

        /// <summary>
        /// Attempts to read an assembly from its file path.
        /// </summary>
        /// <param name="path">The path to the assembly.</param>
        /// <returns>The assembly.</returns>
        protected virtual AssemblyDefinition LoadAssemblyFromFile(string path)
        {
            return AssemblyDefinition.FromFile(path);
        }

        private string ProbeGlobalAssemblyCache(AssemblyDescriptor assembly)
        {
            foreach (var directory in GacDirectories)
            {
                string path = directory.Probe(assembly);
                if (path != null)
                    return path;
            }

            return null;
        }

        private string ProbeSearchDirectories(AssemblyDescriptor assembly)
        {
            foreach (string directory in SearchDirectories)
            {
                string path = string.IsNullOrEmpty(assembly.Culture)
                    ? Path.Combine(directory, assembly.Name)
                    : Path.Combine(directory, assembly.Culture, assembly.Name);
                
                path = ProbeFileFromFilePathWithoutExtension(path)
                       ?? ProbeFileFromFilePathWithoutExtension(Path.Combine(path, assembly.Name));
         
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }
        
        internal static string ProbeFileFromFilePathWithoutExtension(string baseFilePath)
        {
            return BinaryFileExtensions
                .Select(extension => baseFilePath + extension)
                .FirstOrDefault(File.Exists);
        }
        
    }
}