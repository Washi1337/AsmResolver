using AsmResolver.DotNet.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.IO;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a base implementation of an assembly resolver, that includes a collection of search directories to look
    /// into for probing assemblies.
    /// </summary>
    public abstract class AssemblyResolverBase : IAssemblyResolver
    {
        private static readonly string[] BinaryFileExtensions = {".dll", ".exe"};
        private static readonly SignatureComparer Comparer = new()
        {
            AcceptNewerAssemblyVersionNumbers = true
        };

        private readonly Dictionary<AssemblyDescriptor, AssemblyDefinition> _cache = new(new SignatureComparer());

        /// <summary>
        /// Initializes the base of an assembly resolver.
        /// </summary>
        /// <param name="fileService">The service to use for reading files from the disk.</param>
        protected AssemblyResolverBase(IFileService fileService)
        {
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        /// <summary>
        /// Gets the file service that is used for reading files from the disk.
        /// </summary>
        public IFileService FileService
        {
            get;
        }

        /// <summary>
        /// Gets a collection of custom search directories that are probed upon resolving a reference
        /// to an assembly.
        /// </summary>
        public IList<string> SearchDirectories
        {
            get;
        } = new List<string>();

        /// <inheritdoc />
        public AssemblyDefinition Resolve(AssemblyDescriptor assembly)
        {
            if (_cache.TryGetValue(assembly, out var assemblyDef))
                return assemblyDef;

            assemblyDef = ResolveImpl(assembly);
            if (assemblyDef != null)
                _cache.Add(assembly, assemblyDef);

            return assemblyDef;
        }

        /// <inheritdoc />
        public void AddToCache(AssemblyDescriptor descriptor, AssemblyDefinition definition)
        {
            if (_cache.ContainsKey(descriptor))
                throw new ArgumentException($"The cache already contains an entry of assembly {descriptor.FullName}.", nameof(descriptor));

            if (!Comparer.Equals(descriptor, definition))
                throw new ArgumentException("Assembly descriptor and definition do not refer to the same assembly.");

            _cache.Add(descriptor, definition);
        }

        /// <inheritdoc />
        public bool RemoveFromCache(AssemblyDescriptor descriptor) => _cache.Remove(descriptor);

        /// <inheritdoc />
        public bool HasCached(AssemblyDescriptor descriptor) => _cache.ContainsKey(descriptor);

        /// <inheritdoc />
        public void ClearCache() => _cache.Clear();

        /// <summary>
        /// Resolves a new unseen reference to an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to resolve.</param>
        /// <returns>The resolved assembly, or <c>null</c> if the resolution failed.</returns>
        /// <remarks>
        /// This method should not implement caching of resolved assemblies. The caller of this method already implements
        /// this.
        /// </remarks>
        protected virtual AssemblyDefinition ResolveImpl(AssemblyDescriptor assembly)
        {
            // Prefer assemblies in the current directory, in case .NET libraries are shipped with the application.
            string path = ProbeSearchDirectories(assembly);

            if (string.IsNullOrEmpty(path))
            {
                // If failed, probe the runtime installation directories.
                if (assembly.GetPublicKeyToken() is not null)
                    path = ProbeRuntimeDirectories(assembly);

                // If still no suitable file was found, abort.
                if (string.IsNullOrEmpty(path))
                    return null;
            }

            // Attempt to load the file.
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

        /// <summary>
        /// Attempts to read an assembly from its file path.
        /// </summary>
        /// <param name="path">The path to the assembly.</param>
        /// <returns>The assembly.</returns>
        protected virtual AssemblyDefinition LoadAssemblyFromFile(string path)
        {
            return AssemblyDefinition.FromFile(FileService.OpenFile(path));
        }

        /// <summary>
        /// Probes all search directories in <see cref="SearchDirectories"/> for the provided assembly.
        /// </summary>
        /// <param name="assembly">The assembly descriptor to search.</param>
        /// <returns>The path to the assembly, or <c>null</c> if none was found.</returns>
        protected string ProbeSearchDirectories(AssemblyDescriptor assembly)
        {
            for (int i = 0; i < SearchDirectories.Count; i++)
            {
                string path = ProbeDirectory(assembly, SearchDirectories[i]);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }

        /// <summary>
        /// Probes all known runtime directories for the provided assembly.
        /// </summary>
        /// <param name="assembly">The assembly descriptor to search.</param>
        /// <returns>The path to the assembly, or <c>null</c> if none was found.</returns>
        protected abstract string ProbeRuntimeDirectories(AssemblyDescriptor assembly);

        /// <summary>
        /// Probes a directory for the provided assembly.
        /// </summary>
        /// <param name="assembly">The assembly descriptor to search.</param>
        /// <param name="directory">The path to the directory to probe.</param>
        /// <returns>The path to the assembly, or <c>null</c> if none was found.</returns>
        protected static string ProbeDirectory(AssemblyDescriptor assembly, string directory)
        {
            string path = string.IsNullOrEmpty(assembly.Culture)
                ? Path.Combine(directory, assembly.Name)
                : Path.Combine(directory, assembly.Culture, assembly.Name);

            path = ProbeFileFromFilePathWithoutExtension(path)
                   ?? ProbeFileFromFilePathWithoutExtension(Path.Combine(path, assembly.Name));
            return path;
        }

        internal static string ProbeFileFromFilePathWithoutExtension(string baseFilePath)
        {
            foreach (string extension in BinaryFileExtensions)
            {
                string path = baseFilePath + extension;
                if (File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}
