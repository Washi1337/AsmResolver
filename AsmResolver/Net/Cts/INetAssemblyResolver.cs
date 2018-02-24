using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides methods for resolving .NET assemblies.
    /// </summary>
    public interface INetAssemblyResolver
    {
        /// <summary>
        /// Resolves a reference to a .NET assembly.
        /// </summary>
        /// <param name="descriptor">The assembly to resolve.</param>
        /// <returns>The resolved assembly.</returns>
        AssemblyDefinition ResolveAssembly(IAssemblyDescriptor descriptor);
    }

    /// <summary>
    /// Provides a default assembly resolution mechanism for .NET assemblies.
    /// </summary>
    public class DefaultNetAssemblyResolver : INetAssemblyResolver
    {
        /// <summary>
        /// Gets the standard GAC directories used to search for assemblies registered in windows.
        /// </summary>
        public static GacDirectory[] GacDirectories
        {
            get;
            private set;
        }

        static DefaultNetAssemblyResolver()
        {
            var gacDirectories = new List<GacDirectory>();

            if (Utilities.IsRunningOnMono())
            {
                gacDirectories.AddRange(GetGacDirectories("/usr/lib/mono/gac", true));
            }
            else
            {
                gacDirectories.AddRange(
                    GetGacDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        "assembly"), false));
                gacDirectories.AddRange(
                    GetGacDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        "Microsoft.NET", "assembly"), true));
            }

            GacDirectories = gacDirectories.ToArray();
        }

        private static IEnumerable<GacDirectory> GetGacDirectories(string assemblyDirectory, bool is40)
        {
            return
                Directory.EnumerateDirectories(assemblyDirectory)
                    .Select(directory => new GacDirectory(directory, is40 ? "v4.0_" : string.Empty));
        }

        public event AssemblyResolutionEventHandler AssemblyResolutionFailed;
        private readonly Dictionary<IAssemblyDescriptor, AssemblyDefinition> _cachedAssemblies = new Dictionary<IAssemblyDescriptor, AssemblyDefinition>();
        private readonly SignatureComparer _signatureComparer = new SignatureComparer();

        public DefaultNetAssemblyResolver()
        {
            ThrowOnNotFound = true;
            SearchDirectories = new List<string>();
            if (Utilities.IsRunningOnMono())
            {
                SearchDirectories.AddRange(Directory.GetDirectories("/usr/lib/mono"));
            }
        }

        public DefaultNetAssemblyResolver(params string[] directories)
            : this()
        {
            foreach (var directory in directories)
                SearchDirectories.Add(directory);
        }

        /// <summary>
        /// Gets a list of directories to search for assemblies.
        /// </summary>
        public List<string> SearchDirectories
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resolver should throw an exception when an assembly could not be resolved.
        /// </summary>
        public bool ThrowOnNotFound
        {
            get;
            set;
        }

        public AssemblyDefinition ResolveAssembly(IAssemblyDescriptor descriptor)
        {
            AssemblyDefinition definition;
            if (_cachedAssemblies.TryGetValue(descriptor, out definition))
                return definition;

            var path = GetFilePath(descriptor);
            if (!string.IsNullOrEmpty(path))
            {
                var assembly = ReadAssembly(path);
                if (assembly.NetDirectory != null 
                    && assembly.NetDirectory.MetadataHeader != null 
                    && assembly.NetDirectory.MetadataHeader.Image != null)
                {
                    definition = assembly.NetDirectory.MetadataHeader.Image.Assembly;
                    _cachedAssemblies.Add(descriptor, definition);
                    return definition;
                }
            }

            definition = OnAssemblyResolutionFailed(new AssemblyResolutionEventArgs(descriptor));
            if (definition == null)
            {
                if (ThrowOnNotFound)
                    throw new AssemblyResolutionException(descriptor);
            }
            else
            {
                _cachedAssemblies.Add(descriptor, definition);
            }

            return definition;
        }

        /// <summary>
        /// Clears the assembly resolution cache.
        /// </summary>
        public void ClearCache()
        {
            _cachedAssemblies.Clear();
        }

        /// <summary>
        /// Gets the file path to the assembly file that was described by the given assembly descriptor.
        /// </summary>
        /// <param name="descriptor">The assembly to resolve.</param>
        /// <returns>The path to the assembly file.</returns>
        protected virtual string GetFilePath(IAssemblyDescriptor descriptor)
        {
            if (descriptor.PublicKeyToken != null)
            {
                foreach (var gacDirectory in GacDirectories)
                {
                    var filePath = gacDirectory.GetFilePath(descriptor);
                    if (File.Exists(filePath))
                        return filePath;
                }
            }

            foreach (var directory in SearchDirectories)
            {
                var path = Path.Combine(directory, descriptor.Name);
                if (File.Exists(path + ".dll"))
                    return path + ".dll";
                if (File.Exists(path + ".dll"))
                    return path + ".dll";
            }

            return null;
        }

        /// <summary>
        /// Reads a windows assembly image from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to the assembly to read.</param>
        /// <returns>The assembly that was read.</returns>
        protected virtual WindowsAssembly ReadAssembly(string filePath)
        {
            var windowsAssembly = WindowsAssembly.FromBytes(File.ReadAllBytes(filePath), new ReadingParameters());
            windowsAssembly.NetDirectory.MetadataHeader.LockMetadata();
            return windowsAssembly;
        }

        /// <summary>
        /// Fires when an assembly could not be resolved, providing the availability to extend the assembly resolver.
        /// </summary>
        /// <param name="e">The event arguments associated with this event.</param>
        /// <returns>The assembly that was resolved, or null if none could be found.</returns>
        protected virtual AssemblyDefinition OnAssemblyResolutionFailed(AssemblyResolutionEventArgs e)
        {
            if (AssemblyResolutionFailed != null)
                return AssemblyResolutionFailed(this, e);
            return null;
        }
    }

    /// <summary>
    /// Represents a directory in the GAC.
    /// </summary>
    public class GacDirectory
    {
        public GacDirectory(string path, string folderPrefix)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            DirectoryPath = path;
            FolderPrefix = folderPrefix ?? string.Empty;
        }

        public string DirectoryPath
        {
            get;
            private set;
        }

        public string FolderPrefix
        {
            get;
            private set;
        }

        public string GetFilePath(IAssemblyDescriptor descriptor)
        {
            return Path.Combine(DirectoryPath,
                descriptor.Name,
                FolderPrefix + descriptor.Version + "__" + descriptor.PublicKeyToken.ToHexString(),
                descriptor.Name + ".dll");
        }
    }
}
