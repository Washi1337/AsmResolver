using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet.Serialized;
using AsmResolver.IO;
using AsmResolver.Shims;

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
            : this(new ModuleReaderParameters(UncachedFileService.Instance), MonoPathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new default assembly resolver.
        /// </summary>
        public DotNetFrameworkAssemblyResolver(ModuleReaderParameters readerParameters, MonoPathProvider? monoPathProvider)
            : base(readerParameters)
        {
            DetectGacDirectories(monoPathProvider);
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

        private void DetectGacDirectories(MonoPathProvider? monoPathProvider)
        {
            if (RuntimeInformationShim.IsRunningOnWindows)
            {
                DetectWindowsGacDirectories();
            }
            else if (monoPathProvider is not null)
            {
                DetectMonoGacDirectories(monoPathProvider);
            }
        }

        private void DetectWindowsGacDirectories()
        {
            string? systemRoot = Environment.GetEnvironmentVariable("windir");
            if (string.IsNullOrEmpty(systemRoot))
                return;

            string windowsGac = PathShim.Combine(systemRoot, "assembly");
            AddGacDirectories(windowsGac, null);

            string frameworkGac = PathShim.Combine(systemRoot, "Microsoft.NET", "assembly");
            AddGacDirectories(frameworkGac, "v4.0_");
        }

        private void DetectMonoGacDirectories(MonoPathProvider monoPathProvider)
        {
            if (!string.IsNullOrEmpty(monoPathProvider.GacDirectory))
                GacMsilDirectories.Add(new GacDirectory(monoPathProvider.GacDirectory!));

            if (!string.IsNullOrEmpty(monoPathProvider.ApiDirectory))
                SearchDirectories.Add(monoPathProvider.ApiDirectory!);

            if (!string.IsNullOrEmpty(monoPathProvider.FacadesDirectory))
                SearchDirectories.Add(monoPathProvider.FacadesDirectory!);
        }

        private void AddGacDirectories(string windowsGac, string? prefix)
        {
            if (!Directory.Exists(windowsGac))
                return;

            foreach (string directory in Directory.GetDirectories(windowsGac))
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
