using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It contains language and code page formatting
    /// information for the strings specified by the Children member. A code page is an ordered character set.
    /// </summary>
    public class StringTable : VersionTableEntry, IDictionary<string, string>
    {
        /// <summary>
        /// The name of the string describing the comments assigned to the executable file.
        /// </summary>
        public const string CommentsKey = "Comments";

        /// <summary>
        /// The name of the string describing the name of the company that developed the executable file.
        /// </summary>
        public const string CompanyNameKey = "CompanyName";

        /// <summary>
        /// The name of the string describing the executable in such a way that it can be presented to users.
        /// </summary>
        public const string FileDescriptionKey = "FileDescription";

        /// <summary>
        /// The name of the string describing the version of the file.
        /// </summary>
        public const string FileVersionKey = "FileVersion";

        /// <summary>
        /// The name of the string describing the internal name of the file.
        /// </summary>
        public const string InternalNameKey = "InternalName";

        /// <summary>
        /// The name of the string describing the copyright notices that apply to the file.
        /// </summary>
        public const string LegalCopyrightKey = "LegalCopyright";

        /// <summary>
        /// The name of the string describing the trademark notices that apply to the file.
        /// </summary>
        public const string LegalTrademarksKey = "LegalTrademarks";

        /// <summary>
        /// The name of the string providing the original file name.
        /// </summary>
        public const string OriginalFilenameKey = "OriginalFilename";

        /// <summary>
        /// The name of the string describing by whom, where, and why this private version of the file was built.
        /// </summary>
        public const string PrivateBuildKey = "PrivateBuild";

        /// <summary>
        /// The name of the string describing the name of the product with which this file is distributed.
        /// </summary>
        public const string ProductNameKey = "ProductName";

        /// <summary>
        /// The name of the string describing the version of the product with which this file is distributed.
        /// </summary>
        public const string ProductVersionKey = "ProductVersion";

        /// <summary>
        /// The name of the string describing how this version of the file differs from the normal version
        /// </summary>
        public const string SpecialBuildKey = "SpecialBuild";

        private readonly IDictionary<string, string> _entries = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new string table.
        /// </summary>
        /// <param name="languageIdentifier">The language identifier.</param>
        /// <param name="codePage">The code page.</param>
        public StringTable(ushort languageIdentifier, ushort codePage)
        {
            LanguageIdentifier = languageIdentifier;
            CodePage = codePage;
        }

        /// <inheritdoc />
        public override string Key => $"{LanguageIdentifier:x4}{CodePage:x4}";

        /// <summary>
        /// Gets or sets the language identifier of this string table.
        /// </summary>
        public ushort LanguageIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the code page of this string table.
        /// </summary>
        public ushort CodePage
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override VersionTableValueType ValueType => VersionTableValueType.Binary;

        /// <inheritdoc />
        public string this[string key]
        {
            get => _entries[key];
            set => _entries[key] = value;
        }

        /// <inheritdoc />
        public int Count => _entries.Count;

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

        /// <inheritdoc />
        public ICollection<string> Keys => _entries.Keys;

        /// <inheritdoc />
        public ICollection<string> Values => _entries.Values;

        /// <summary>
        /// Reads a single StringTable structure from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read structure.</returns>
        public static StringTable FromReader(ref BinaryStreamReader reader)
        {
            ulong start = reader.Offset;

            // Read header.
            var header = VersionTableEntryHeader.FromReader(ref reader);
            if (header.Key.Length != 8 || !uint.TryParse(header.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rawKey))
                throw new FormatException("Invalid string table language identifier or code page.");

            var result = new StringTable((ushort) (rawKey >> 16), (ushort) (rawKey & 0xFFFF));

            // Read entries.
            while (reader.Offset - start < header.Length)
            {
                reader.Align(4);
                var entry = ReadEntry(ref reader);
                result.Add(entry.Key, entry.Value);
            }

            return result;
        }

        private static KeyValuePair<string, string> ReadEntry(ref BinaryStreamReader reader)
        {
            ulong start = reader.Offset;

            // Read header.
            var header = VersionTableEntryHeader.FromReader(ref reader);
            reader.Align(4);

            // Read value.
            byte[] data = new byte[header.ValueLength * sizeof(char)];
            int count = reader.ReadBytes(data, 0, data.Length);

            // Exclude zero terminator.
            count = Math.Max(count - 2, 0);
            string value = Encoding.Unicode.GetString(data, 0, count);

            // Skip any unprocessed bytes.
            reader.Offset = start + header.Length;

            return new KeyValuePair<string, string>(header.Key, value);
        }

        /// <inheritdoc />
        public void Add(string key, string value) => _entries.Add(key, value);

        /// <inheritdoc />
        public bool ContainsKey(string key) => _entries.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _entries.TryGetValue(key, out value);

        /// <inheritdoc />
        public void Add(KeyValuePair<string, string> item) => _entries.Add(item);

        /// <inheritdoc />
        public bool Remove(string key) => _entries.Remove(key);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, string> item) => _entries.Remove(item);

        /// <inheritdoc />
        public void Clear() => _entries.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, string> item) => _entries.Contains(item);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => _entries.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint size = VersionTableEntryHeader.GetHeaderSize(Key);

            foreach (var entry in _entries)
            {
                size = size.Align(4);
                size += CalculateEntrySize(entry);
            }

            return size;
        }

        private static uint CalculateEntrySize(KeyValuePair<string,string> entry)
        {
            uint size = VersionTableEntryHeader.GetHeaderSize(entry.Key);
            size = size.Align(4);
            size += CalculateEntryValueSize(entry.Value);
            return size;
        }

        private static uint CalculateEntryValueSize(string value)
        {
            return (uint) (Encoding.Unicode.GetByteCount(value) + sizeof(char));
        }

        /// <inheritdoc />
        protected override uint GetValueLength() => 0u;

        /// <inheritdoc />
        protected override void WriteValue(BinaryStreamWriter writer)
        {
            foreach (var entry in _entries)
            {
                writer.Align(4);
                WriteEntry(writer, entry);
            }
        }

        private static void WriteEntry(BinaryStreamWriter writer, KeyValuePair<string, string> entry)
        {
            var header = new VersionTableEntryHeader(entry.Key)
            {
                Length = (ushort) (VersionTableEntryHeader.GetHeaderSize(entry.Key).Align(4)
                    + CalculateEntryValueSize(entry.Value)),
                ValueLength = (ushort) (entry.Value.Length + 1),
                Type = VersionTableValueType.String
            };
            header.Write(writer);

            writer.Align(4);
            writer.WriteBytes(Encoding.Unicode.GetBytes(entry.Value));
            writer.WriteUInt16(0);
        }
    }
}
