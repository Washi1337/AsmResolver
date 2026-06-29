using System;
using System.IO;
using System.Security.Cryptography;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

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
        /// <summary>
        /// Reads a private key from an input file.
        /// </summary>
        /// <param name="path">The path to the strong-name key file.</param>
        /// <returns>The private key.</returns>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        public static StrongNamePublicKey FromFile(string path)
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
        public static StrongNamePublicKey FromReader(ref BinaryStreamReader reader)
        {
            // Read BLOBHEADER
            ReadBlobHeader(ref reader, StrongNameKeyStructureType.PublicKeyBlob, 2, SignatureAlgorithm.RsaSign);

            // Read RSAPUBKEY
            if ((RsaPublicKeyMagic)reader.ReadUInt32() != RsaPublicKeyMagic.Rsa1)
                throw new FormatException("Input stream does not contain a valid RSA public key header magic.");

            uint bitLength = reader.ReadUInt32();

            var result = new StrongNamePublicKey(new byte[bitLength / 8], reader.ReadUInt32());
            // CryptoAPI PUBLICKEYBLOB stores the modulus in little-endian; the
            // in-memory representation here is big-endian (matching the
            // RSAParameters constructor and ToRsaParameters), so reverse it.
            reader.ReadBytes(result.Modulus, 0, result.Modulus.Length);
            Array.Reverse(result.Modulus);
            return result;
        }

        /// <summary>
        /// Reverses the byte order of the provided byte array, returning a new
        /// array. Used to translate between the little-endian on-disk CryptoAPI
        /// key blob layout and the big-endian layout <see cref="RSAParameters"/>
        /// expects.
        /// </summary>
        protected static byte[] Reverse(byte[] data)
        {
            if (data is null)
                return null!;
            var result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[result.Length - i - 1] = data[i];
            return result;
        }

        /// <summary>
        /// Reads a big-endian byte array (as <see cref="RSAParameters.Exponent"/>
        /// is encoded) into a <see cref="uint"/>.
        /// </summary>
        protected static uint BigEndianBytesToUInt(byte[] bytes)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));

            uint value = 0;
            int start = 0;
            while (start < bytes.Length && bytes[start] == 0)
                start++;

            int significant = bytes.Length - start;
            if (significant > sizeof(uint))
                throw new ArgumentException("RSA exponent is too large to fit in a 32-bit CryptoAPI public exponent.");

            for (int i = start; i < bytes.Length; i++)
                value = (value << 8) | bytes[i];
            return value;
        }

        /// <summary>
        /// Converts a <see cref="uint"/> to the big-endian, no-leading-zero byte
        /// array <see cref="RSAParameters.Exponent"/> expects (e.g. 65537 ->
        /// <c>[01 00 01]</c>). A zero input yields a single zero byte.
        /// </summary>
        protected static byte[] UIntToBigEndianBytes(uint value)
        {
            if (value == 0)
                return new byte[] { 0 };

            int length = value switch
            {
                <= 0xFF => 1,
                <= 0xFFFF => 2,
                <= 0xFFFFFF => 3,
                _ => 4
            };
            var result = new byte[length];
            for (int i = length - 1; i >= 0; i--)
            {
                result[i] = (byte)value;
                value >>= 8;
            }
            return result;
        }

        /// <summary>
        /// Creates a new strong name public key.
        /// </summary>
        /// <param name="modulus">The modulus to use in the RSA crypto system.</param>
        /// <param name="publicExponent">The public exponent to use in the RSA crypto system.</param>
        public StrongNamePublicKey(byte[] modulus, uint publicExponent)
        {
            Modulus = modulus ?? throw new ArgumentNullException(nameof(modulus));
            PublicExponent = publicExponent;
        }

        /// <summary>
        /// Imports a public key from an instance of <see cref="RSAParameters"/>.
        /// </summary>
        /// <param name="parameters">The RSA parameters to import.</param>
        public StrongNamePublicKey(in RSAParameters parameters)
        {
            if (parameters.Modulus is null)
                throw new ArgumentException("RSA parameters does not define a modulus.");
            if (parameters.Exponent is null)
                throw new ArgumentException("RSA parameters does not define an exponent.");

            // RSAParameters stores integers big-endian; keep that as the
            // in-memory representation and only reverse at the blob boundary.
            Modulus = (byte[])parameters.Modulus.Clone();
            PublicExponent = BigEndianBytesToUInt(parameters.Exponent);
        }

        /// <inheritdoc />
        public override StrongNameKeyStructureType Type => StrongNameKeyStructureType.PublicKeyBlob;

        /// <inheritdoc />
        public override byte Version => 2;

        /// <inheritdoc />
        public override SignatureAlgorithm SignatureAlgorithm => SignatureAlgorithm.RsaSign;

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

        /// <summary>
        /// Prepares a blob signature containing the full public key of an assembly.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm that is used to hash the PE file.</param>
        /// <returns>The blob signature.</returns>
        public byte[] CreatePublicKeyBlob(AssemblyHashAlgorithm hashAlgorithm)
        {
            using var tempStream = new MemoryStream();
            var writer = new BinaryStreamWriter(tempStream);
            writer.WriteUInt32((uint)SignatureAlgorithm);
            writer.WriteUInt32((uint)hashAlgorithm);
            writer.WriteUInt32((uint)(0x14 + Modulus.Length));
            writer.WriteByte((byte)StrongNameKeyStructureType.PublicKeyBlob);
            writer.WriteByte(2);
            writer.WriteUInt16(0);
            writer.WriteUInt32((uint)SignatureAlgorithm);
            writer.WriteUInt32((uint)RsaPublicKeyMagic.Rsa1);
            writer.WriteUInt32((uint)BitLength);
            writer.WriteUInt32(PublicExponent);
            // Modulus is stored big-endian internally; the on-disk CryptoAPI
            // PUBLICKEYBLOB layout uses little-endian, so reverse on write.
            writer.WriteBytes(Reverse(Modulus));
            return tempStream.ToArray();
        }

        /// <summary>
        /// Translates the strong name parameters to an instance of <see cref="RSAParameters"/>.
        /// </summary>
        /// <returns>The converted RSA parameters.</returns>
        public virtual RSAParameters ToRsaParameters()
        {
            return new RSAParameters
            {
                Modulus = Modulus,
                // RSAParameters.Exponent is big-endian with no leading zeros.
                // BitConverter.GetBytes would emit little-endian 4 bytes (wrong).
                Exponent = UIntToBigEndianBytes(PublicExponent)
            };
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return base.GetPhysicalSize() // _PUBLICKEYSTRUC (BLOBHEADER)
                   + sizeof(RsaPublicKeyMagic) // magic
                   + sizeof(uint) // bitlen
                   + sizeof(uint) // pubexp
                   + (uint)Modulus.Length // modulus
                ;
        }

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32((uint)Magic);
            writer.WriteUInt32((uint)BitLength);
            writer.WriteUInt32(PublicExponent);
            // Modulus is stored big-endian internally; the on-disk CryptoAPI
            // layout uses little-endian, so reverse on write (mirrors FromReader,
            // which reverses the little-endian file bytes back into big-endian).
            writer.WriteBytes(Reverse(Modulus));
        }
    }
}
