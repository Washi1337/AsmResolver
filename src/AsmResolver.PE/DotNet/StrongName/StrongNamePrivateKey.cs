namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference:
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/rsa-schannel-key-blobs
    //
    
    public class StrongNamePrivateKey : StrongNamePublicKey
    {
        /// <inheritdoc />
        public override StrongNameKeyStructureType Type => StrongNameKeyStructureType.PrivateKeyBlob;

        /// <inheritdoc />
        public override byte Version => 2;

        /// <inheritdoc />
        public override RsaPublicKeyMagic Magic => RsaPublicKeyMagic.Rsa2;

        public byte[] P
        {
            get;
            set;
        }

        public byte[] Q
        {
            get;
            set;
        }

        public byte[] DP
        {
            get;
            set;
        }

        public byte[] DQ
        {
            get;
            set;
        }

        public byte[] Coefficient
        {
            get;
            set;
        }
        
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
            writer.WriteBytes(Coefficient);
            writer.WriteBytes(PrivateExponent);
        }
    }
}