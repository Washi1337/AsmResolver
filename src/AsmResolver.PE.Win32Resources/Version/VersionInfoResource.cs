using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;

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

        private FixedVersionInfo _fixedVersionInfo = new();
        private readonly Dictionary<string, VersionTableEntry> _entries = new();

        /// <summary>
        /// Creates a new empty version info resource targeting the English (United States) language identifier.
        /// </summary>
        public VersionInfoResource()
            : this(0)
        {
        }

        /// <summary>
        /// Creates a new empty version info resource.
        /// </summary>
        /// <param name="lcid">The language identifier the resource version info is targeting.</param>
        public VersionInfoResource(int lcid)
        {
            Lcid = lcid;
        }

        /// <summary>
        /// Gets the language identifier the resource version info is targeting.
        /// </summary>
        public int Lcid
        {
            get;
        }

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
            set => _entries[name] = value;
        }

        /// <summary>
        /// Obtains all version info resources from the provided root win32 resources directory.
        /// </summary>
        /// <param name="rootDirectory">The root resources directory to extract the version info from.</param>
        /// <returns>The version info resource, or <c>null</c> if none was found.</returns>
        public static IEnumerable<VersionInfoResource?> FindAllFromDirectory(ResourceDirectory rootDirectory)
        {
            if (!rootDirectory.TryGetDirectory(ResourceType.Version, out var versionDirectory))
                return Enumerable.Empty<VersionInfoResource?>();

            var categoryDirectory = versionDirectory
                .Entries
                .OfType<ResourceDirectory>()
                .FirstOrDefault();

            if (categoryDirectory is null)
                return Enumerable.Empty<VersionInfoResource?>();

            return categoryDirectory.Entries
                .OfType<ResourceData>()
                .Select(FromResourceData)!;
        }

        /// <summary>
        /// Obtains the first version info resource from the provided root win32 resources directory.
        /// </summary>
        /// <param name="rootDirectory">The root resources directory to extract the version info from.</param>
        /// <returns>The version info resource, or <c>null</c> if none was found.</returns>
        public static VersionInfoResource? FromDirectory(ResourceDirectory rootDirectory)
        {
            return FindAllFromDirectory(rootDirectory).FirstOrDefault();
        }

        /// <summary>
        /// Obtains the version info resource from the provided root win32 resources directory.
        /// </summary>
        /// <param name="rootDirectory">The root resources directory to extract the version info from.</param>
        /// <param name="lcid">The language identifier to get the version info from.</param>
        /// <returns>The version info resource, or <c>null</c> if none was found.</returns>
        public static VersionInfoResource? FromDirectory(ResourceDirectory rootDirectory, int lcid)
        {
            if (!rootDirectory.TryGetDirectory(ResourceType.Version, out var versionDirectory))
                return null;

            var categoryDirectory = versionDirectory
                .Entries
                .OfType<ResourceDirectory>()
                .FirstOrDefault();

            var dataEntry = categoryDirectory
                ?.Entries
                .OfType<ResourceData>()
                .FirstOrDefault(x => x.Id == lcid);

            if (dataEntry is null)
                return null;

            return FromResourceData(dataEntry);
        }

        /// <summary>
        /// Obtains the version info resource from the provided resource data entry.
        /// </summary>
        /// <param name="dataEntry">The data entry to extract the version info from.</param>
        /// <returns>The extracted version info resource.</returns>
        public static VersionInfoResource FromResourceData(ResourceData dataEntry)
        {
            if (dataEntry.CanRead)
            {
                var dataReader = dataEntry.CreateReader();
                return FromReader((int) dataEntry.Id, ref dataReader);
            }

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
        public static VersionInfoResource FromReader(ref BinaryStreamReader reader) => FromReader(0, ref reader);

        /// <summary>
        /// Reads a version resource from an input stream.
        /// </summary>
        /// <param name="lcid">The language identifier to get the version info from.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The parsed version resource.</returns>
        /// <exception cref="FormatException">
        /// Occurs when the input stream does not point to a valid version resource.
        /// </exception>
        public static VersionInfoResource FromReader(int lcid, ref BinaryStreamReader reader)
        {
            ulong start = reader.Offset;

            // Read header.
            var header = VersionTableEntryHeader.FromReader(ref reader);
            if (header.Key != VsVersionInfoKey)
                throw new FormatException($"Input stream does not point to a {VsVersionInfoKey} entry.");

            var result = new VersionInfoResource(lcid);

            // Read fixed version info.
            reader.Align(4);
            result.FixedVersionInfo = FixedVersionInfo.FromReader(ref reader);

            // Read children.
            while (reader.Offset - start < header.Length)
            {
                reader.Align(4);
                result.AddEntry(ReadNextEntry(ref reader));
            }

            return result;
        }

        private static VersionTableEntry ReadNextEntry(ref BinaryStreamReader reader)
        {
            ulong start = reader.Offset;

            var header = VersionTableEntryHeader.FromReader(ref reader);
            reader.Align(4);

            return header.Key switch
            {
                VarFileInfo.VarFileInfoKey => VarFileInfo.FromReader(start, header, ref reader),
                StringFileInfo.StringFileInfoKey => StringFileInfo.FromReader(start, header, ref reader),
                _ => throw new FormatException($"Invalid or unsupported entry {header.Key}.")
            };
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
            return (TEntry) this[name];
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
        protected override void WriteValue(BinaryStreamWriter writer)
        {
            FixedVersionInfo.Write(writer);
            foreach (var entry in _entries.Values)
            {
                writer.Align(4);
                entry.Write(writer);
            }
        }

        /// <inheritdoc />
        public void InsertIntoDirectory(ResourceDirectory rootDirectory)
        {
            // Add version directory if it doesn't exist yet.
            if (!rootDirectory.TryGetDirectory(ResourceType.Version, out var versionDirectory))
            {
                versionDirectory = new ResourceDirectory(ResourceType.Version);
                rootDirectory.InsertOrReplaceEntry(versionDirectory);
            }

            // Add category directory if it doesn't exist yet.
            if (!versionDirectory.TryGetDirectory(1, out var categoryDirectory))
            {
                categoryDirectory = new ResourceDirectory(1);
                versionDirectory.InsertOrReplaceEntry(categoryDirectory);
            }

            // Insert / replace data entry.
            categoryDirectory.InsertOrReplaceEntry(new ResourceData((uint) Lcid, this));
        }
    }
}
