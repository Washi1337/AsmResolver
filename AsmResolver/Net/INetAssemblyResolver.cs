using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public interface INetAssemblyResolver
    {
        AssemblyDefinition ResolveAssembly(IAssemblyDescriptor descriptor);
    }

    public class DefaultNetAssemblyResolver : INetAssemblyResolver
    {
        public static GacDirectory[] GacDirectories
        {
            get;
            private set;
        }

        static DefaultNetAssemblyResolver()
        {
            var gacDirectories = new List<GacDirectory>();

            gacDirectories.AddRange(
                GetGacDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "assembly"), false));
            gacDirectories.AddRange(
                GetGacDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "Microsoft.NET", "assembly"), true));

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
        }

        public DefaultNetAssemblyResolver(params string[] directories)
            : this()
        {
            foreach (var directory in directories)
                SearchDirectories.Add(directory);
        }

        public IList<string> SearchDirectories
        {
            get;
            private set;
        }

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
                if (assembly.NetDirectory != null && assembly.NetDirectory.MetadataHeader != null)
                {
                    var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
                    if (tableStream != null)
                    {

                        var assemblyTable = tableStream.GetTable<AssemblyDefinition>();
                        if (assemblyTable.Count > 0)
                        {

                            definition = assemblyTable[0];
                            _cachedAssemblies.Add(descriptor, definition);
                            return definition;
                        }
                    }
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

        public void ClearCache()
        {
            _cachedAssemblies.Clear();
        }

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

        protected virtual WindowsAssembly ReadAssembly(string filePath)
        {
            return WindowsAssembly.FromBytes(File.ReadAllBytes(filePath), new ReadingParameters());
        }

        protected virtual AssemblyDefinition OnAssemblyResolutionFailed(AssemblyResolutionEventArgs e)
        {
            if (AssemblyResolutionFailed != null)
                return AssemblyResolutionFailed(this, e);
            return null;
        }
    }

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
