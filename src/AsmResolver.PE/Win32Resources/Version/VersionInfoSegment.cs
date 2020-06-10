using System;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents a native version resource file. 
    /// </summary>
    public class VersionInfoSegment : VersionTableEntry
    {
        /// <summary>
        /// The name of the root object of the native version resource file. 
        /// </summary>
        public const string VsVersionInfoKey = "VS_VERSION_INFO";
        
        /// <summary>
        /// Reads a version resource from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The parsed version resource.</returns>
        /// <exception cref="FormatException">
        /// Occurs when the input stream does not point to a valid version resource.
        /// </exception>
        public static VersionInfoSegment FromReader(IBinaryStreamReader reader)
        {
            var header = ResourceTableHeader.FromReader(reader);
            
            if (header.Key != VsVersionInfoKey)
                throw new FormatException($"Input stream does not point to a {VsVersionInfoKey} entry.");
            
            var result = new VersionInfoSegment();

            reader.Align(4);
            result.FixedVersionInfo = FixedVersionInfo.FromReader(reader);
            
            return result;
        }

        /// <inheritdoc />
        public override string Key => VsVersionInfoKey;

        /// <inheritdoc />
        protected override ResourceValueType ValueType => ResourceValueType.Binary;

        /// <inheritdoc />
        protected override ISegment Value => FixedVersionInfo;
        
        /// <summary>
        /// Gets the fixed version info stored in this version resource.
        /// </summary>
        public FixedVersionInfo FixedVersionInfo
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return ResourceTableHeader.GetResourceHeaderSize(VsVersionInfoKey)
                   + FixedVersionInfo.GetPhysicalSize().Align(4);
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}