using System;
using System.IO;
using System.Security.Cryptography;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

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
        public static StrongNamePublicKey FromFile(string path) => 
            FromReader(new ByteArrayReader(System.IO.File.ReadAllBytes(path)));

        /// <summary>
        /// Reads a private key from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The private key.</returns>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        public static StrongNamePublicKey FromReader(IBinaryStreamReader reader)
        {
            // Read BLOBHEADER
            ReadBlobHeader(reader, StrongNameKeyStructureType.PublicKeyBlob, 2, SignatureAlgorithm.RsaSign);
            
            // Read RSAPUBKEY
            if ((RsaPublicKeyMagic) reader.ReadUInt32() != RsaPublicKeyMagic.Rsa1)
                throw new FormatException("Input stream does not contain a valid RSA public key header magic.");
            
            uint bitLength = reader.ReadUInt32();

            var result = new StrongNamePublicKey
            {
                PublicExponent = reader.ReadUInt32(),
                Modulus = new byte[bitLength / 8]
            };
            
            reader.ReadBytes(result.Modulus, 0, result.Modulus.Length);

            return result;
        }

        internal byte[] CopyReversed(byte[] data)
        {
            var result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[result.Length - i - 1] = data[i];
            return result;
        }

        /// <summary>
        /// Creates a new empty public key.
        /// </summary>
        public StrongNamePublicKey()
        {
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
            Modulus = CopyReversed(parameters.Modulus);
            uint exponent = 0;
            for (int i = 0; i < Math.Min(sizeof(uint), parameters.Exponent.Length); i++)
                exponent |= (uint) (parameters.Exponent[i] << (8 * i));
            PublicExponent = exponent;
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
            writer.WriteUInt32((uint) SignatureAlgorithm);
            writer.WriteUInt32((uint) hashAlgorithm);
            writer.WriteUInt32((uint) (0x14 + Modulus.Length));
            writer.WriteByte((byte) StrongNameKeyStructureType.PublicKeyBlob);
            writer.WriteByte(2);
            writer.WriteUInt16(0);
            writer.WriteUInt32((uint) SignatureAlgorithm);
            writer.WriteUInt32((uint) RsaPublicKeyMagic.Rsa1);
            writer.WriteUInt32((uint) BitLength);
            writer.WriteUInt32(PublicExponent);
            writer.WriteBytes(CopyReversed(Modulus));
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
                Exponent = BitConverter.GetBytes(PublicExponent)
            };
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