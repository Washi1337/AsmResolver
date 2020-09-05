using System;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It is the root structure that contains
    /// all other file-version information structures. 
    /// </summary>
    public class FixedVersionInfo : SegmentBase
    {
        /// <summary>
        /// The signature value a FixedVersionInfo structure starts with.
        /// </summary>
        public const uint Signature = 0xFEEF04BD;
        
        internal const uint DefaultStructVersion = 0x00010000;
        
        /// <summary>
        /// Reads a single fixed version info structure from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read structure.</returns>
        public static FixedVersionInfo FromReader(IBinaryStreamReader reader)
        {
            var result = new FixedVersionInfo();
            result.UpdateOffsets(reader.Offset, reader.Rva);

            uint signature = reader.ReadUInt32();
            if (signature != Signature)
                throw new FormatException($"Input stream does not point to a valid FixedVersionInfo structure.");

            uint structVersion = reader.ReadUInt32();
            
            result.FileVersion = ReadVersion(reader);
            result.ProductVersion = ReadVersion(reader);
            result.FileFlagsMask = (FileFlags) reader.ReadUInt32();
            result.FileFlags = (FileFlags) reader.ReadUInt32();
            result.FileOS = (FileOS) reader.ReadUInt32();
            result.FileType = (FileType) reader.ReadUInt32();
            result.FileSubType = (FileSubType) reader.ReadUInt32();
            result.FileDate = reader.ReadUInt64();
            
            return result;
        }

        private static System.Version ReadVersion(IBinaryStreamReader reader)
        {
            ushort minor = reader.ReadUInt16();
            ushort major = reader.ReadUInt16();
            ushort revision = reader.ReadUInt16();
            ushort build = reader.ReadUInt16();

            return new System.Version(major, minor, build, revision);
        }

        /// <summary>
        /// Gets or sets the file version number.
        /// </summary>
        public System.Version FileVersion
        {
            get;
            set;
        } = new System.Version();

        /// <summary>
        /// Gets or sets the product version number.
        /// </summary>
        public System.Version ProductVersion
        {
            get;
            set;
        } = new System.Version();

        /// <summary>
        /// Gets or sets the bitmask that specifies the valid bits in <see cref="FileFlags"/>.
        /// A bit is valid only if it was defined when the file was created.
        /// </summary>
        public FileFlags FileFlagsMask
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the bitmask that specifies the attributes of the file.
        /// </summary>
        public FileFlags FileFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or ses the operating system for which this file was designed. 
        /// </summary>
        public FileOS FileOS
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the general type of file.
        /// </summary>
        public FileType FileType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the function of the file.
        /// </summary>
        public FileSubType FileSubType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binary creation date and time stamp
        /// </summary>
        public ulong FileDate
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() =>
            sizeof(uint) // Signature 
            + sizeof(uint) // StructVersion
            + sizeof(ushort) * 4 // FileVersion
            + sizeof(ushort) * 4 // ProductVersion
            + sizeof(uint) // FileFlagsMask
            + sizeof(uint) // FileFlags
            + sizeof(uint) // FileOS
            + sizeof(uint) // FileType
            + sizeof(uint) // FileSubType
            + sizeof(ulong); // FileDate

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(Signature);
            writer.WriteUInt32(DefaultStructVersion);
            WriteVersion(writer, FileVersion);
            WriteVersion(writer, ProductVersion);
            writer.WriteUInt32((uint) FileFlagsMask);
            writer.WriteUInt32((uint) FileFlags);
            writer.WriteUInt32((uint) FileOS);
            writer.WriteUInt32((uint) FileType);
            writer.WriteUInt32((uint) FileSubType);
            writer.WriteUInt64(FileDate);
        }

        private static void WriteVersion(IBinaryStreamWriter writer, System.Version version)
        {
            writer.WriteUInt16((ushort) version.Minor);
            writer.WriteUInt16((ushort) version.Major);
            writer.WriteUInt16((ushort) version.Revision);
            writer.WriteUInt16((ushort) version.Build);
        }
    }
}