using System;
using System.Security.Cryptography;
using AsmResolver.IO;

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
        public new static StrongNamePrivateKey FromFile(string path)
        {
            var reader = new BinaryStreamReader(System.IO.File.ReadAllBytes(path));
            return FromReader(ref reader);
        }

        /// <summary>
        /// Reads a private key from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The private key.</returns>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        public new static StrongNamePrivateKey FromReader(ref BinaryStreamReader reader)
        {
            // Read BLOBHEADER
            ReadBlobHeader(ref reader, StrongNameKeyStructureType.PrivateKeyBlob, 2, SignatureAlgorithm.RsaSign);

            // Read RSAPUBKEY
            if ((RsaPublicKeyMagic)reader.ReadUInt32() != RsaPublicKeyMagic.Rsa2)
                throw new FormatException("Input stream does not contain a valid RSA private key header magic.");

            uint bitLength = reader.ReadUInt32();

            var result = new StrongNamePrivateKey(bitLength)
            {
                PublicExponent = reader.ReadUInt32(),
            };

            // CryptoAPI PRIVATEKEYBLOB stores every RSA integer (Modulus, P, Q,
            // DP, DQ, InverseQ, D) in little-endian. The in-memory representation
            // here is big-endian (matching StrongNamePublicKey's storage and the
            // big-endian RSAParameters that ToRsaParameters returns), so each
            // integer is read as raw little-endian bytes and then reversed.
            reader.ReadBytes(result.Modulus, 0, result.Modulus.Length);
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
        /// Creates a new empty private key.
        /// </summary>
        public StrongNamePrivateKey(uint bitLength)
            : base(new byte[bitLength / 8], 65537)
        {
            uint length8 = bitLength / 8;
            uint length16 = bitLength / 16;

            P = new byte[length16];
            Q = new byte[length16];
            DP = new byte[length16];
            DQ = new byte[length16];
            InverseQ = new byte[length16];
            PrivateExponent = new byte[length8];
        }

        /// <summary>
        /// Imports a public/private key pair from an instance of <see cref="RSAParameters"/>.
        /// </summary>
        /// <param name="parameters">The RSA parameters to import. All integers
        /// are expected in the big-endian byte order <see cref="RSA"/> uses.</param>
        public StrongNamePrivateKey(in RSAParameters parameters)
            : base(parameters.Modulus ?? throw new ArgumentException("The provided RSA parameters do not define a modulus."),
                ByteSwap(parameters))
        {
            // base() set Modulus to a clone of the big-endian input - keep it
            // big-endian (matches the on-disk reverse done in Write/FromReader).
            P = parameters.P ?? throw new ArgumentException("The provided RSA parameters do not define prime P.");
            Q = parameters.Q ?? throw new ArgumentException("The provided RSA parameters do not define prime Q.");
            DP = parameters.DP ?? throw new ArgumentException("The provided RSA parameters do not define DP.");
            DQ = parameters.DQ ?? throw new ArgumentException("The provided RSA parameters do not define DQ.");

            InverseQ = parameters.InverseQ
                       ?? throw new ArgumentException("The provided RSA parameters do not define InverseQ.");
            PrivateExponent =
                parameters.D ?? throw new ArgumentException("The provided RSA parameters do not define D.");
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
            return new RSAParameters
            {
                Modulus = Modulus,
                // RSAParameters.Exponent is big-endian with no leading zeros.
                Exponent = UIntToBigEndianBytes(PublicExponent),
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
            uint length8 = (uint)(BitLength / 8);
            uint length16 = (uint)(BitLength / 16);
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
        public override void Write(BinaryStreamWriter writer)
        {
            // base.Write emits BLOBHEADER + RSAPUBKEY + Modulus. The base
            // StrongNamePublicKey.Write reverses Modulus (big-endian storage ->
            // little-endian on-disk). The private RSA integers (P, Q, DP, DQ,
            // InverseQ, D) are likewise stored big-endian internally and must be
            // reversed here to produce a valid CryptoAPI PRIVATEKEYBLOB.
            base.Write(writer);
            writer.WriteBytes(Reverse(P));
            writer.WriteBytes(Reverse(Q));
            writer.WriteBytes(Reverse(DP));
            writer.WriteBytes(Reverse(DQ));
            writer.WriteBytes(Reverse(InverseQ));
            writer.WriteBytes(Reverse(PrivateExponent));
        }

        private static uint ByteSwap(RSAParameters parameters)
        {
            if (parameters.Exponent is null)
                throw new ArgumentException("The provided RSA parameters do not define an exponent.");

            return BigEndianBytesToUInt(parameters.Exponent);
        }
    }
}
