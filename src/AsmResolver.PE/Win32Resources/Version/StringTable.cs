using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It contains language and code page formatting
    /// information for the strings specified by the Children member. A code page is an ordered character set.
    /// </summary>
    public class StringTable : VersionTableEntry
    {
        /// <summary>
        /// Reads a single StringTable structure from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read structure.</returns>
        public static StringTable FromReader(IBinaryStreamReader reader)
        {
            uint start = reader.FileOffset;
            
            var header = ResourceTableHeader.FromReader(reader);
            if (header.Key.Length != 8 || !uint.TryParse(header.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rawKey))
                throw new FormatException("Invalid string table language identifier or code page.");

            var result = new StringTable((ushort) (rawKey >> 16), (ushort) (rawKey & 0xFFFF));
            
            while (reader.FileOffset - start < header.Length)
            {
                reader.Align(4);
                result.Entries.Add(StringTableEntry.FromReader(reader));
            }

            return result;
        }

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
        protected override ResourceValueType ValueType => ResourceValueType.Binary;

        /// <summary>
        /// Gets a collection of entries stored in this string table.
        /// </summary>
        public IList<StringTableEntry> Entries
        {
            get;
        } = new List<StringTableEntry>();

        /// <inheritdoc />
        protected override uint GetValueLength() => (uint) Entries.Sum(e => e.GetPhysicalSize());

        /// <inheritdoc />
        protected override void WriteValue(IBinaryStreamWriter writer)
        {
            foreach (var entry in Entries)
                entry.Write(writer);
        }
    }
}