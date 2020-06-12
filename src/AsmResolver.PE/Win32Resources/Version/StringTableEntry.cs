using System;
using System.Text;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It contains a string that describes a
    /// specific aspect of a file, for example, a file's version, its copyright notices, or its trademarks.
    /// </summary>
    public class StringTableEntry : VersionTableEntry
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
        /// Reads a single String structure from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read structure.</returns>
        public static StringTableEntry FromReader(IBinaryStreamReader reader)
        {
            uint start = reader.FileOffset;
            
            // Read header.
            var header = ResourceTableHeader.FromReader(reader);
            reader.Align(4);

            // Read value.
            var data = new byte[header.ValueLength * sizeof(char)];
            int count = reader.ReadBytes(data, 0, data.Length);
            
            // Exclude zero terminator.
            count = Math.Max(count - 2, 0);
            string value = Encoding.Unicode.GetString(data, 0, count);
            
            // Skip any unprocessed bytes.
            reader.FileOffset = start + header.Length;

            return new StringTableEntry(header.Key, value);
        }

        /// <summary>
        /// Creates a new string table entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value"></param>
        public StringTableEntry(string key, string value)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public override string Key
        {
            get;
        }

        /// <inheritdoc />
        protected override ResourceValueType ValueType => ResourceValueType.String;

        /// <summary>
        /// Gets or sets the string value describing a specific aspect of a file.
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override uint GetValueLength() => (uint) (Encoding.Unicode.GetByteCount(Value) + sizeof(char));

        /// <inheritdoc />
        protected override void WriteValue(IBinaryStreamWriter writer) =>
            writer.WriteBytes(Encoding.Unicode.GetBytes(Value));

        /// <inheritdoc />
        public override string ToString() => $"{Key}: {Value}";
    }
}