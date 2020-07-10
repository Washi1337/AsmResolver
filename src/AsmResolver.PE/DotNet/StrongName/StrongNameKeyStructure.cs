using System;

namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference:
    // https://docs.microsoft.com/en-us/windows/win32/api/wincrypt/ns-wincrypt-publickeystruc
    
    /// <summary>
    /// Provides a base for strong name key structures.
    /// </summary>
    public abstract class StrongNameKeyStructure : SegmentBase
    {
        /// <summary>
        /// Reads and verifies the blob header at the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="expectedType">The expected structure type to read.</param>
        /// <param name="expectedVersion">The expected structure version to read.</param>
        /// <param name="expectedAlgorithm">The expected algorithm.</param>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        protected static void ReadBlobHeader(IBinaryStreamReader reader,
            StrongNameKeyStructureType expectedType, 
            byte expectedVersion, 
            SignatureAlgorithm expectedAlgorithm)
        {
            // Read RSAPUBKEY
            if ((StrongNameKeyStructureType) reader.ReadByte() != expectedType)
                throw new FormatException("Input stream does not contain the expected structure type.");
            if (reader.ReadByte() != expectedVersion)
                throw new NotSupportedException("Invalid or unsupported public/private key pair structure version number.");
            reader.ReadUInt16();
            if ((SignatureAlgorithm) reader.ReadUInt32() != expectedAlgorithm)
                throw new NotSupportedException("Invalid or unsupported public key algorithm.");
        }

        
        /// <summary>
        /// Gets the type of structure that is encoded.
        /// </summary>
        public abstract StrongNameKeyStructureType Type
        {
            get;
        }

        /// <summary>
        /// Gets the version number of the structure.
        /// </summary>
        public abstract byte Version
        {
            get;
        }

        /// <summary>
        /// Gets the algorithm used.
        /// </summary>
        public abstract SignatureAlgorithm SignatureAlgorithm
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return sizeof(StrongNameKeyStructureType) // bType
                   + sizeof(byte) // bVersion
                   + sizeof(ushort) // reserved
                   + sizeof(SignatureAlgorithm) // aiKeyAlg
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte) Type);
            writer.WriteByte(Version);
            writer.WriteUInt16(0);
            writer.WriteUInt32((uint) SignatureAlgorithm);
        }
    }
}