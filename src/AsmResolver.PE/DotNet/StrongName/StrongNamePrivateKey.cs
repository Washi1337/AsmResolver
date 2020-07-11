using System;
using System.Collections.Generic;
using System.Security.Cryptography;

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
            ReadBlobHeader(reader, StrongNameKeyStructureType.PrivateKeyBlob, 2, SignatureAlgorithm.RsaSign);

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

            Array.Reverse(result.Modulus);
            Array.Reverse(result.P);
            Array.Reverse(result.Q);
            Array.Reverse(result.DP);
            Array.Reverse(result.DQ);
            Array.Reverse(result.InverseQ);
            Array.Reverse(result.PrivateExponent);
            
            return result;
        }

        /// <summary>
        /// Creates a new empty public/private key pair.
        /// </summary>
        public StrongNamePrivateKey()
        {
        }
        
        /// <summary>
        /// Imports a public/private key pair from an instance of <see cref="RSAParameters"/>.
        /// </summary>
        /// <param name="parameters">The RSA parameters to import.</param>
        public StrongNamePrivateKey(in RSAParameters parameters)
        {
            Modulus = parameters.Modulus;
            P = parameters.P;
            Q = parameters.Q;
            DP = parameters.DP;
            DQ = parameters.DQ;

            uint exponent = 0;
            for (int i = 0; i < Math.Min(sizeof(uint), parameters.Exponent.Length); i++)
                exponent |= (uint) (parameters.Exponent[i] << (8 * i));

            PublicExponent = exponent;
            InverseQ = parameters.InverseQ;
            PrivateExponent = parameters.D;
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
        public override RSAParameters ToRsaParameters()
        {
            var exponentBytes = new List<byte>(sizeof(uint))
            {
                (byte) (PublicExponent & 0xFF),
                (byte) ((PublicExponent >> 8) & 0xFF),
                (byte) ((PublicExponent >> 16) & 0xFF),
                (byte) ((PublicExponent >> 24) & 0xFF),
            };
            
            for (int i = exponentBytes.Count - 1; i >= 0 && exponentBytes[i] == 0; i--)
                exponentBytes.RemoveAt(i);

            return new RSAParameters
            {
                Modulus = Modulus,
                Exponent = exponentBytes.ToArray(),
                P = P,
                Q = Q,
                DP = DP,
                DQ = DQ,
                D = PrivateExponent,
                InverseQ = InverseQ,
            };
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