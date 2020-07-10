using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents a native version resource file. 
    /// </summary>
    public class VersionInfoResource : VersionTableEntry, IWin32Resource
    {
        /// <summary>
        /// The name of the root object of the native version resource file. 
        /// </summary>
        public const string VsVersionInfoKey = "VS_VERSION_INFO";

        /// <summary>
        /// Obtains the version info resource from the provided root win32 resources directory.
        /// </summary>
        /// <param name="rootDirectory">The root resources directory to extract the version info from.</param>
        /// <returns>The version info resource, or <c>null</c> if none was found.</returns>
        public static VersionInfoResource FromDirectory(IResourceDirectory rootDirectory)
        {
            var versionDirectory = (IResourceDirectory) rootDirectory.Entries[ResourceDirectoryHelper.IndexOfResourceDirectoryType(rootDirectory, ResourceType.Version)];

            var categoryDirectory = versionDirectory
                ?.Entries
                .OfType<IResourceDirectory>()
                .FirstOrDefault();
            
            var dataEntry = categoryDirectory
                ?.Entries
                .OfType<IResourceData>()
                .FirstOrDefault();

            if (dataEntry is null)
                return null;
            
            if (dataEntry.CanRead)
                return FromReader(dataEntry.CreateReader());
            if (dataEntry.Contents is VersionInfoResource resource)
                return resource;
            throw new ArgumentException("Version resource data is not readable.");
        }
        
        /// <summary>
        /// Reads a version resource from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The parsed version resource.</returns>
        /// <exception cref="FormatException">
        /// Occurs when the input stream does not point to a valid version resource.
        /// </exception>
        public static VersionInfoResource FromReader(IBinaryStreamReader reader)
        {
            uint start = reader.FileOffset;
            
            // Read header.
            var header = VersionTableEntryHeader.FromReader(reader);
            if (header.Key != VsVersionInfoKey)
                throw new FormatException($"Input stream does not point to a {VsVersionInfoKey} entry.");
            
            var result = new VersionInfoResource();

            // Read fixed version info.
            reader.Align(4);
            result.FixedVersionInfo = FixedVersionInfo.FromReader(reader);

            // Read children.
            while (reader.FileOffset - start < header.Length)
            {
                reader.Align(4);
                result.AddEntry(ReadNextEntry(reader));
            }

            return result;
        }

        private static VersionTableEntry ReadNextEntry(IBinaryStreamReader reader)
        {
            uint start = reader.FileOffset;
            
            var header = VersionTableEntryHeader.FromReader(reader);
            reader.Align(4);

            return header.Key switch
            {
                VarFileInfo.VarFileInfoKey => VarFileInfo.FromReader(start, header, reader),
                StringFileInfo.StringFileInfoKey => StringFileInfo.FromReader(start, header, reader),
                _ => throw new FormatException($"Invalid or unsupported entry {header.Key}.")
            };
        }

        private FixedVersionInfo _fixedVersionInfo = new FixedVersionInfo();
        private readonly IDictionary<string, VersionTableEntry> _entries = new Dictionary<string, VersionTableEntry>();

        /// <inheritdoc />
        public override string Key => VsVersionInfoKey;

        /// <inheritdoc />
        protected override VersionTableValueType ValueType => VersionTableValueType.Binary;

        /// <summary>
        /// Gets the fixed version info stored in this version resource.
        /// </summary>
        public FixedVersionInfo FixedVersionInfo
        {
            get => _fixedVersionInfo;
            set => _fixedVersionInfo = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets a version table entry by its name.
        /// </summary>
        /// <param name="name">The name of the child.</param>
        public VersionTableEntry this[string name]
        {
            get => _entries[name];
            set
            {
                if (value is null)
                    _entries.Remove(name);
                else
                    _entries[name] = value;
            }
        }

        /// <summary>
        /// Gets a collection of entries stored in the version resource. 
        /// </summary>
        public IEnumerable<VersionTableEntry> GetChildren() => _entries.Values;

        /// <summary>
        /// Gets a version table entry by its name.
        /// </summary>
        /// <param name="name">The name of the child.</param>
        /// <typeparam name="TEntry">The type of the version table entry to lookup.</typeparam>
        /// <returns>The entry.</returns>
        public TEntry GetChild<TEntry>(string name) 
            where TEntry : VersionTableEntry
        {
            return this[name] as TEntry;
        }

        /// <summary>
        /// Adds (or overrides the existing entry with the same name) to the version resource. 
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry(VersionTableEntry entry) => _entries[entry.Key] = entry;

        /// <summary>
        /// Remove an entry by its name.
        /// </summary>
        /// <param name="name">The name of the child to remove..</param>
        /// <returns>
        /// <c>true</c> if the name existed in the table and was removed successfully, <c>false</c> otherwise.
        /// </returns>
        public bool RemoveEntry(string name) => _entries.Remove(name);

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint size = VersionTableEntryHeader.GetHeaderSize(Key);
            size = size.Align(4);
            size += _fixedVersionInfo.GetPhysicalSize();
            size = size.Align(4);
            
            foreach (var entry in _entries)
            {
                size = size.Align(4);
                size += entry.Value.GetPhysicalSize();
            }

            return size;
        }

        /// <inheritdoc />
        protected override uint GetValueLength() => FixedVersionInfo.GetPhysicalSize();

        /// <inheritdoc />
        protected override void WriteValue(IBinaryStreamWriter writer)
        {
            FixedVersionInfo.Write(writer);
            writer.Align(4);
            foreach (var entry in _entries.Values)
                entry.Write(writer);
        }

        /// <inheritdoc />
        public void WriteToDirectory(IResourceDirectory rootDirectory)
        {
            // Find and remove old version directory.
            int index = ResourceDirectoryHelper.IndexOfResourceDirectoryType(rootDirectory, ResourceType.Version);
            
            if (index == -1)
                index = rootDirectory.Entries.Count;
            else
                rootDirectory.Entries.RemoveAt(index);

            // Construct new directory.
            var newVersionDirectory = new ResourceDirectory(ResourceType.Version)
            {
                Entries =
                {
                    new ResourceDirectory(1)
                    {
                        Entries = {new ResourceData(0u, this)}
                    }
                }
            };
            
            // Insert.
            rootDirectory.Entries.Insert(index, newVersionDirectory);
        }
    }
}