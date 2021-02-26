using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes the global assembly cache (GAC), as well
    /// as any custom search directories.
    /// </summary>
    public class DotNetFrameworkAssemblyResolver : AssemblyResolverBase
    {
        /// <summary>
        /// Creates a new default assembly resolver.
        /// </summary>
        public DotNetFrameworkAssemblyResolver()
        {
            DetectGacDirectories();
        }

        /// <summary>
        /// Gets a collection of global assembly cache (GAC) directories that are probed upon resolving a reference
        /// to an assembly.
        /// </summary>
        public ICollection<GacDirectory> GacDirectories
        {
            get;
        } = new List<GacDirectory>();

        /// <inheritdoc />
        protected override AssemblyDefinition ResolveImpl(AssemblyDescriptor assembly)
        {
            string path = null;
            
            if (assembly.GetPublicKeyToken()!= null)
                path = ProbeGlobalAssemblyCache(assembly);
            if (string.IsNullOrEmpty(path))
                path = ProbeSearchDirectories(assembly);

            AssemblyDefinition assemblyDef = null;
            try
            {
                assemblyDef = LoadAssemblyFromFile(path);
            }
            catch
            {
                // ignore any errors.
            }

            return assemblyDef;
        }
        
        private void DetectGacDirectories()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                DetectWindowsGacDirectories();
            }
            else if (Directory.Exists("/usr/lib/mono"))
            {
                DetectMonoGacDirectories();
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

        private void DetectMonoGacDirectories()
        {
            if (Directory.Exists("/usr/lib/mono/gac"))
                GacDirectories.Add(new GacDirectory("/usr/lib/mono/gac"));

            string mostRecentMonoDirectory = Directory
                .EnumerateDirectories("/usr/lib/mono")
                .Where(d => d.EndsWith("-api"))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (mostRecentMonoDirectory != null)
                SearchDirectories.Add(mostRecentMonoDirectory);
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
    }
}