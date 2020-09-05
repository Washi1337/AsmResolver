using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It contains language and code page formatting
    /// information for the strings specified by the Children member. A code page is an ordered character set.
    /// </summary>
    public class StringTable : VersionTableEntry, IEnumerable<KeyValuePair<string, string>>
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
        
        /// <summary>
        /// Reads a single StringTable structure from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read structure.</returns>
        public static StringTable FromReader(IBinaryStreamReader reader)
        {
            ulong start = reader.Offset;
            
            // Read header.
            var header = VersionTableEntryHeader.FromReader(reader);
            if (header.Key.Length != 8 || !uint.TryParse(header.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rawKey))
                throw new FormatException("Invalid string table language identifier or code page.");

            var result = new StringTable((ushort) (rawKey >> 16), (ushort) (rawKey & 0xFFFF));
            
            // Read entries.
            while (reader.Offset - start < header.Length)
            {
                reader.Align(4);
                var entry = ReadEntry(reader);
                result.Add(entry.Key, entry.Value);
            }

            return result;
        }

        private static KeyValuePair<string, string> ReadEntry(IBinaryStreamReader reader)
        {
            ulong start = reader.Offset;
            
            // Read header.
            var header = VersionTableEntryHeader.FromReader(reader);
            reader.Align(4);

            // Read value.
            var data = new byte[header.ValueLength * sizeof(char)];
            int count = reader.ReadBytes(data, 0, data.Length);
            
            // Exclude zero terminator.
            count = Math.Max(count - 2, 0);
            string value = Encoding.Unicode.GetString(data, 0, count);
            
            // Skip any unprocessed bytes.
            reader.Offset = start + header.Length;

            return new KeyValuePair<string, string>(header.Key, value);
        }

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
        public override string Key => $"{LanguageIdentifier:X4}{CodePage:X4}";

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

        /// <summary>
        /// Gets or sets the value of a single field in the string table.
        /// </summary>
        /// <param name="key">The name of the field in the string table.</param>
        public string this[string key]
        {
            get => _entries[key];
            set
            {
                if (value is null)
                    _entries.Remove(key);
                else 
                    _entries[key] = value;
            }
        }

        /// <summary>
        /// Adds (or overrides) a field to the string table.
        /// </summary>
        /// <param name="key">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public void Add(string key, string value) => 
            _entries[key] = value ?? throw new ArgumentNullException(nameof(value));

        /// <summary>
        /// Removes a single field from the string table by its name.
        /// </summary>
        /// <param name="key">The name of the field.</param>
        /// <returns><c>true</c> if the field existed and was removed successfully, <c>false</c> otherwise.</returns>
        public bool Remove(string key) => _entries.Remove(key);

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
        protected override void WriteValue(IBinaryStreamWriter writer)
        {
            foreach (var entry in _entries)
            {
                writer.Align(4);
                WriteEntry(writer, entry);
            }
        }

        private static void WriteEntry(IBinaryStreamWriter writer, KeyValuePair<string, string> entry)
        {
            var header = new VersionTableEntryHeader
            {
                Length = (ushort) (VersionTableEntryHeader.GetHeaderSize(entry.Key).Align(4)
                                   + CalculateEntryValueSize(entry.Value)),
                ValueLength = (ushort) (entry.Value.Length + 1),
                Key = entry.Key,
                Type = VersionTableValueType.String
            };
            header.Write(writer);

            writer.Align(4);
            writer.WriteBytes(Encoding.Unicode.GetBytes(entry.Value));
            writer.WriteUInt16(0);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}