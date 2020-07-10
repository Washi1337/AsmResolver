namespace AsmResolver.PE.DotNet.StrongName
{
    /// <summary>
    /// Provides a base for strong name key structures.
    /// </summary>
    public abstract class StrongNameKeyStructure : SegmentBase
    {
        // Reference:
        // https://docs.microsoft.com/en-us/windows/win32/api/wincrypt/ns-wincrypt-publickeystruc
        
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
        public abstract AlgorithmIdentifier Algorithm
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return sizeof(StrongNameKeyStructureType) // bType
                   + sizeof(byte) // bVersion
                   + sizeof(ushort) // reserved
                   + sizeof(AlgorithmIdentifier) // aiKeyAlg
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte) Type);
            writer.WriteByte(Version);
            writer.WriteUInt16(0);
            writer.WriteUInt32((uint) Algorithm);
        }
    }
}