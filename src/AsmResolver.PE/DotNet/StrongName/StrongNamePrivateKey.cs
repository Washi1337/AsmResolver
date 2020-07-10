namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference:
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/rsa-schannel-key-blobs
    //
    
    /// <summary>
    /// Represents a public/private key pair in the RSA crypto system.
    /// </summary>
    public class StrongNamePrivateKey : StrongNamePublicKey
    {
        /// <inheritdoc />
        public override StrongNameKeyStructureType Type => StrongNameKeyStructureType.PrivateKeyBlob;

        /// <inheritdoc />
        public override byte Version => 2;

        /// <inheritdoc />
        public override RsaPublicKeyMagic Magic => RsaPublicKeyMagic.Rsa2;

        /// <summary>
        /// Gets or sets the first prime number used in the RSA crypto system.
        /// </summary>
        public byte[] P
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the second prime number used in the RSA crypto system.
        /// </summary>
        public byte[] Q
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the first exponent (equal to d mod (p-1)) used in the RSA crypto system.
        /// </summary>
        public byte[] DP
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the second exponent (equal to d mod (q-1)) used in the RSA crypto system.
        /// </summary>
        public byte[] DQ
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the coefficient which is equal to the modular inverse of q mod p, used in the RSA crypto system.
        /// </summary>
        public byte[] InverseQ
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the private exponent used in the RSA crypto system.
        /// </summary>
        public byte[] PrivateExponent
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint length8 = (uint) (BitLength / 8);
            uint length16 = (uint) (BitLength / 16);
            return base.GetPhysicalSize()
                   + length16 // p
                   + length16 // q
                   + length16 // dp
                   + length16 // dq
                   + length16 // coefficient
                   + length8 // private exponent
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            base.Write(writer);
            writer.WriteBytes(P);
            writer.WriteBytes(Q);
            writer.WriteBytes(DP);
            writer.WriteBytes(DQ);
            writer.WriteBytes(InverseQ);
            writer.WriteBytes(PrivateExponent);
        }
    }
}