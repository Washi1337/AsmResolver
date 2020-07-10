namespace AsmResolver.PE.DotNet.StrongName
{
    public abstract class StrongNameKeyStructure : SegmentBase
    {
        // Reference:
        // https://docs.microsoft.com/en-us/windows/win32/api/wincrypt/ns-wincrypt-publickeystruc
        
        public abstract StrongNameKeyStructureType Type
        {
            get;
        }

        public abstract byte Version
        {
            get;
        }

        public AlgorithmIdentifier Algorithm
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