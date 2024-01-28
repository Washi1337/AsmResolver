using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Serialized;
using AsmResolver.IO;

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
            : this(UncachedFileService.Instance)
        {
        }

        /// <summary>
        /// Creates a new default assembly resolver.
        /// </summary>
        /// <param name="fileService">The service to use for reading files from the disk.</param>
        public DotNetFrameworkAssemblyResolver(IFileService fileService)
            : this(new ModuleReaderParameters
            {
                PEReaderParameters = {FileService = fileService}
            })
        {
        }

        /// <summary>
        /// Creates a new default assembly resolver.
        /// </summary>
        public DotNetFrameworkAssemblyResolver(ModuleReaderParameters readerParameters)
            : base(readerParameters)
        {
            DetectGacDirectories();
        }

        /// <summary>
        /// Gets a collection of 32-bit global assembly cache (GAC_32) directories that are probed upon resolving a
        /// reference to an assembly.
        /// </summary>
        public IList<GacDirectory> Gac32Directories
        {
            get;
        } = new List<GacDirectory>();

        /// <summary>
        /// Gets a collection of 64-bit global assembly cache (GAC_64) directories that are probed upon resolving a
        /// reference to an assembly.
        /// </summary>
        public IList<GacDirectory> Gac64Directories
        {
            get;
        } = new List<GacDirectory>();

        /// <summary>
        /// Gets a collection of MSIL global assembly cache (GAC_MSIL) directories that are probed upon resolving a
        /// reference to an assembly.
        /// </summary>
        public IList<GacDirectory> GacMsilDirectories
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
                GacMsilDirectories.Add(new GacDirectory("/usr/lib/mono/gac"));

            string? mostRecentMonoDirectory = Directory
                .EnumerateDirectories("/usr/lib/mono")
                .Where(d => d.EndsWith("-api"))
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (mostRecentMonoDirectory is not null)
                SearchDirectories.Add(mostRecentMonoDirectory);
        }

        private void AddGacDirectories(string windowsGac, string? prefix)
        {
            if (!Directory.Exists(windowsGac))
                return;

            foreach (string directory in Directory.EnumerateDirectories(windowsGac))
                GetGacDirectoryCollection(directory).Add(new GacDirectory(directory, prefix));

            IList<GacDirectory> GetGacDirectoryCollection(string directory) => Path.GetFileName(directory) switch
            {
                "GAC_32" => Gac32Directories,
                "GAC_64" => Gac64Directories,
                _ => GacMsilDirectories
            };
        }

        /// <inheritdoc />
        protected override string? ProbeRuntimeDirectories(AssemblyDescriptor assembly)
        {
            bool is32BitPreferred;
            bool is32BitRequired;

            // Try infer from declaring module which GAC directory would be preferred.
            if (assembly is IModuleProvider {Module: { } module})
            {
                is32BitPreferred = module.IsBit32Preferred;
                is32BitRequired = module.IsBit32Required;
            }
            else
            {
                // If declaring module could not be obtained, assume AnyCPU since it is the most common case.
                is32BitPreferred = false;
                is32BitRequired = false;
            }

            string? path;

            if (is32BitRequired)
            {
                // If this assembly only runs on 32-bit, then we should only try resolve from GAC_32 or GAC_MSIL.
                path = ProbeGacDirectories(Gac32Directories);
            }
            else if (is32BitPreferred)
            {
                // If this assembly can run on 64-bit but prefers 32-bit, then prefer GAC_32 over GAC_64.
                path = ProbeGacDirectories(Gac32Directories);
                path ??= ProbeGacDirectories(Gac64Directories);
            }
            else
            {
                // Otherwise assume a 64-bit environment first.
                path = ProbeGacDirectories(Gac64Directories);
                path ??= ProbeGacDirectories(Gac32Directories);
            }

            // Fallback: probe GAC_MSIL.
            return path ?? ProbeGacDirectories(GacMsilDirectories);

            string? ProbeGacDirectories(IList<GacDirectory> directories)
            {
                for (int i = 0; i < directories.Count; i++)
                {
                    if (directories[i].Probe(assembly) is { } p)
                        return p;
                }

                return null;
            }
        }
    }
}
