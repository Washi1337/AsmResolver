using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public IList<GacDirectory> GacDirectories
        {
            get;
        } = new List<GacDirectory>();

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

            foreach (string directory in Directory.EnumerateDirectories(windowsGac))
            {
                string name = Path.GetFileName(directory);
                if (name.StartsWith("GAC"))
                    GacDirectories.Add(new GacDirectory(directory, prefix));
            }
        }

        /// <inheritdoc />
        public override string ProbeRuntimeDirectories(AssemblyDescriptor assembly)
        {
            for (int i = 0; i < GacDirectories.Count; i++)
            {
                string path = GacDirectories[i].Probe(assembly);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }
    }
}
