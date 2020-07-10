namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/rsa-schannel-key-blobs
    // https://docs.microsoft.com/en-us/windows/win32/api/wincrypt/ns-wincrypt-rsapubkey
    
    public class StrongNamePublicKey : StrongNameKeyStructure
    {
        /// <inheritdoc />
        public override StrongNameKeyStructureType Type => StrongNameKeyStructureType.PublicKeyBlob;

        /// <inheritdoc />
        public override byte Version => 2;

        public virtual RsaPublicKeyMagic Magic => RsaPublicKeyMagic.Rsa1;

        public int BitLength => Modulus.Length * 8;
        
        public uint PublicExponent
        {
            get;
            set;
        }

        public byte[] Modulus
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return base.GetPhysicalSize() // _PUBLICKEYSTRUC (BLOBHEADER)
                   + sizeof(RsaPublicKeyMagic) // magic
                   + sizeof(uint) // bitlen
                   + sizeof(uint) // pubexp
                   + (uint) Modulus.Length / 8 // modulus
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32((uint) Magic);
            writer.WriteUInt32((uint) BitLength);
            writer.WriteUInt32(PublicExponent);
            writer.WriteBytes(Modulus);
        }
    }
}