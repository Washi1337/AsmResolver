using System;

namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference:
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/rsa-schannel-key-blobs
    
    /// <summary>
    /// Represents a public/private key pair in the RSA crypto system.
    /// </summary>
    public class StrongNamePrivateKey : StrongNamePublicKey
    {
        /// <summary>
        /// Reads a private key from an input file.
        /// </summary>
        /// <param name="path">The path to the strong-name key file.</param>
        /// <returns>The private key.</returns>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        public new static StrongNamePrivateKey FromFile(string path) => 
            FromReader(new ByteArrayReader(System.IO.File.ReadAllBytes(path)));

        /// <summary>
        /// Reads a private key from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The private key.</returns>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        public new static StrongNamePrivateKey FromReader(IBinaryStreamReader reader)
        {
            // Read BLOBHEADER
            ReadBlobHeader(reader, StrongNameKeyStructureType.PrivateKeyBlob, 2, AlgorithmIdentifier.RsaSign);

            // Read RSAPUBKEY
            if ((RsaPublicKeyMagic) reader.ReadUInt32() != RsaPublicKeyMagic.Rsa2)
                throw new FormatException("Input stream does not contain a valid RSA private key header magic.");
            
            uint bitLength = reader.ReadUInt32();
            uint length8 = bitLength / 8;
            uint length16 = bitLength / 16;

            var result = new StrongNamePrivateKey
            {
                PublicExponent = reader.ReadUInt32(),
                Modulus = new byte[length8],
                P = new byte[length16],
                Q = new byte[length16],
                DP = new byte[length16],
                DQ = new byte[length16],
                InverseQ = new byte[length16],
                PrivateExponent = new byte[length8]
            };
            
            reader.ReadBytes(result.Modulus, 0, result.Modulus.Length);

            // Read private data.
            reader.ReadBytes(result.P, 0, result.P.Length);
            reader.ReadBytes(result.Q, 0, result.Q.Length);
            reader.ReadBytes(result.DP, 0, result.DP.Length);
            reader.ReadBytes(result.DQ, 0, result.DQ.Length);
            reader.ReadBytes(result.InverseQ, 0, result.InverseQ.Length);
            reader.ReadBytes(result.PrivateExponent, 0, result.PrivateExponent.Length);

            return result;
        }
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