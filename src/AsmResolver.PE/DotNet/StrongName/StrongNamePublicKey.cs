namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/rsa-schannel-key-blobs
    // https://docs.microsoft.com/en-us/windows/win32/api/wincrypt/ns-wincrypt-rsapubkey
    
    /// <summary>
    /// Represents the public key in a RSA crypto system. 
    /// </summary>
    public class StrongNamePublicKey : StrongNameKeyStructure
    {
        /// <inheritdoc />
        public override StrongNameKeyStructureType Type => StrongNameKeyStructureType.PublicKeyBlob;

        /// <inheritdoc />
        public override byte Version => 2;

        /// <inheritdoc />
        public override AlgorithmIdentifier Algorithm => AlgorithmIdentifier.RsaSign;

        /// <summary>
        /// Gets the magic header number defining the type of RSA public key structure.
        /// </summary>
        public virtual RsaPublicKeyMagic Magic => RsaPublicKeyMagic.Rsa1;

        /// <summary>
        /// Gets the number of bits used by the modulus parameter.
        /// </summary>
        public int BitLength => Modulus.Length * 8;
        
        /// <summary>
        /// Gets or sets the public exponent used in the RSA crypto system.
        /// </summary>
        public uint PublicExponent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the modulus used in the RSA crypto system.
        /// </summary>
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