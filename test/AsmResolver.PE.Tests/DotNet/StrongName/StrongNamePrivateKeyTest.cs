using System;
using System.IO;
using System.Security.Cryptography;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.StrongName;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.StrongName
{
    public class StrongNamePrivateKeyTest
    {
        /// <summary>
        /// A private key constructed from <see cref="RSAParameters"/> must round-trip
        /// through <see cref="StrongNamePrivateKey.Write"/> and
        /// <see cref="StrongNamePrivateKey.FromReader"/> and still produce parameters
        /// that <see cref="RSA.ImportParameters"/> accepts. This guards the CryptoAPI
        /// PRIVATEKEYBLOB byte order (all RSA integers are little-endian in the blob).
        /// </summary>
        [Fact]
        public void PersistentStrongNamePrivateKey()
        {
            using var rsa = RSA.Create(1024);
            var rsaParameters = rsa.ExportParameters(true);
            var privateKey = new StrongNamePrivateKey(rsaParameters);

            using var tempStream = new MemoryStream();
            privateKey.Write(new BinaryStreamWriter(tempStream));

            tempStream.Position = 0;
            var reader = new BinaryStreamReader(tempStream);
            var newPrivateKey = StrongNamePrivateKey.FromReader(ref reader);

            // Round-trip preserves the stored integers (both are in the
            // little-endian on-disk CryptoAPI layout).
            Assert.Equal(privateKey.Modulus, newPrivateKey.Modulus);
            Assert.Equal(privateKey.P, newPrivateKey.P);
            Assert.Equal(privateKey.Q, newPrivateKey.Q);
            Assert.Equal(privateKey.DP, newPrivateKey.DP);
            Assert.Equal(privateKey.DQ, newPrivateKey.DQ);
            Assert.Equal(privateKey.InverseQ, newPrivateKey.InverseQ);
            Assert.Equal(privateKey.PrivateExponent, newPrivateKey.PrivateExponent);

            // The ORIGINAL key (constructed directly from RSAParameters) must
            // produce RSA parameters usable for signing.
            using var rsaOriginal = RSA.Create();
            rsaOriginal.ImportParameters(privateKey.ToRsaParameters());

            // The ROUND-TRIPPED key must produce RSA parameters usable for signing.
            using var rsa2 = RSA.Create();
            rsa2.ImportParameters(newPrivateKey.ToRsaParameters());

            // Both must produce the same strong-name signature over a sample hash.
            byte[] hash = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
            Assert.Equal(SignWithRsa(rsaOriginal, hash), SignWithRsa(rsa2, hash));
        }

        /// <summary>
        /// A private key written to a file must be readable back to a key that
        /// produces the same SHA-1 strong-name signature as the original.
        /// </summary>
        [Fact]
        public void WriteAndReadPrivateKey_ProducesEquivalentSigningKey()
        {
            using var rsa = RSA.Create(1024);
            var rsaParameters = rsa.ExportParameters(true);
            var original = new StrongNamePrivateKey(rsaParameters);

            using var tempStream = new MemoryStream();
            original.Write(new BinaryStreamWriter(tempStream));

            tempStream.Position = 0;
            var reader = new BinaryStreamReader(tempStream);
            var roundTripped = StrongNamePrivateKey.FromReader(ref reader);

            byte[] hash = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

            using var rsaOriginal = RSA.Create();
            rsaOriginal.ImportParameters(original.ToRsaParameters());
            using var rsaRound = RSA.Create();
            rsaRound.ImportParameters(roundTripped.ToRsaParameters());

            Assert.Equal(SignWithRsa(rsaOriginal, hash), SignWithRsa(rsaRound, hash));
        }

        /// <summary>
        /// Verifies that the PRIVATEKEYBLOB written by <see cref="StrongNamePrivateKey.Write"/>
        /// lays every RSA integer out in little-endian, matching the CryptoAPI
        /// on-disk format (and what <c>sn -k</c> produces). The default exponent
        /// 65537 = [01 00 01] is a palindrome, so this is the only way to catch a
        /// wrong byte order in the modulus/private integers or the pubexp field.
        /// </summary>
        [Fact]
        public void Write_ProducesCryptoApiCompliantLittleEndianBlob()
        {
            using var rsa = RSA.Create(1024);
            var rsaParameters = rsa.ExportParameters(true);
            var privateKey = new StrongNamePrivateKey(rsaParameters);

            using var tempStream = new MemoryStream();
            privateKey.Write(new BinaryStreamWriter(tempStream));
            byte[] blob = tempStream.ToArray();

            Assert.Equal(privateKey.GetPhysicalSize(), (uint)blob.Length);

            // BLOBHEADER (8) + RSAPUBKEY magic (4) + bitlen (4) + pubexp (4) = 20.
            Assert.Equal(0x07, blob[0]); // bType = PRIVATEKEYBLOB
            Assert.Equal(0x02, blob[1]); // bVersion
            Assert.Equal(0x00002400u, BitConverter.ToUInt32(blob, 4)); // CALG_RSA_SIGN
            Assert.Equal(0x32415352u, BitConverter.ToUInt32(blob, 8)); // "RSA2" magic, LE
            Assert.Equal(1024u, BitConverter.ToUInt32(blob, 12)); // bitlen LE

            // pubexp is a little-endian uint32 at offset 16.
            // 65537 = 0x00010001 -> blob bytes [01 00 01 00].
            Assert.Equal(65537u, BitConverter.ToUInt32(blob, 16));
            Assert.Equal(new byte[] { 0x01, 0x00, 0x01, 0x00 }, blob[16..20]);

            // Modulus (128 bytes, little-endian in the blob). RSAParameters.Modulus
            // is big-endian, so the blob bytes must be its reverse.
            byte[] blobModulus = blob[20..(20 + 128)];
            Assert.Equal(ReverseBytes(rsaParameters.Modulus), blobModulus);

            int len16 = 1024 / 16;
            int offset = 20 + 128;
            Assert.Equal(ReverseBytes(rsaParameters.P), blob[offset..(offset + len16)]);
            offset += len16;
            Assert.Equal(ReverseBytes(rsaParameters.Q), blob[offset..(offset + len16)]);
            offset += len16;
            Assert.Equal(ReverseBytes(rsaParameters.DP), blob[offset..(offset + len16)]);
            offset += len16;
            Assert.Equal(ReverseBytes(rsaParameters.DQ), blob[offset..(offset + len16)]);
            offset += len16;
            Assert.Equal(ReverseBytes(rsaParameters.InverseQ), blob[offset..(offset + len16)]);
            offset += len16;
            Assert.Equal(ReverseBytes(rsaParameters.D), blob[offset..(offset + 128)]);
        }

        /// <summary>
        /// The exponent byte order must be handled big-endian, not little-endian.
        /// 65537 = [01 00 01] is a palindrome and hides a wrong byte order; a
        /// non-palindrome exponent such as 0x010002 = [01 00 02] must be read as
        /// 65538 (not 131073) and round-trip back to the same big-endian bytes.
        /// This guards the exponent (the modulus/private integers are covered by
        /// <see cref="Write_ProducesCryptoApiCompliantLittleEndianBlob"/>).
        /// </summary>
        [Fact]
        public void Constructor_ReadsExponent_BigEndian()
        {
            // Non-palindrome: big-endian [01 00 02] = 0x010002 = 65538.
            // A buggy little-endian reader would produce 0x020001 = 131073.
            var rsaParams = new RSAParameters
            {
                Modulus = new byte[128],
                Exponent = new byte[] { 0x01, 0x00, 0x02 }
            };

            var publicKey = new StrongNamePublicKey(rsaParams);
            Assert.Equal(65538u, publicKey.PublicExponent);

            // ToRsaParameters must emit the exponent back big-endian with no
            // leading zeros (RSAParameters convention). The previous code
            // returned BitConverter.GetBytes(...) = little-endian 4 bytes.
            Assert.Equal(rsaParams.Exponent, publicKey.ToRsaParameters().Exponent);
        }

        /// <summary>
        /// A private key constructed with a non-palindrome exponent must round-trip
        /// through Write/FromReader preserving the exponent as well. (The
        /// <c>sn -k</c>-default 65537 masks any exponent byte-order bug because it
        /// is a palindrome; this test uses a synthetic non-palindrome exponent and
        /// does not require a mathematically valid RSA key, only that the exponent
        /// survives the blob round-trip and ToRsaParameters.)
        /// </summary>
        [Fact]
        public void WriteAndReadPrivateKey_PreservesNonPalindromicExponent()
        {
            // Build a dummy private key with a non-palindrome exponent. The P/Q/D
            // values are not mathematically valid, but the blob round-trip and the
            // exponent conversion are what we assert here (no RSA signing).
            var rsaParams = new RSAParameters
            {
                Modulus = new byte[128],
                Exponent = new byte[] { 0x01, 0x00, 0x02 }, // 65538, non-palindrome
                P = new byte[64],
                Q = new byte[64],
                DP = new byte[64],
                DQ = new byte[64],
                InverseQ = new byte[64],
                D = new byte[128]
            };

            var original = new StrongNamePrivateKey(rsaParams);
            Assert.Equal(65538u, original.PublicExponent);

            using var tempStream = new MemoryStream();
            original.Write(new BinaryStreamWriter(tempStream));

            // The pubexp field in the blob is a little-endian uint32 = 65538
            // = [02 00 01 00]. A buggy big-endian writer would emit [01 00 02 00].
            byte[] blob = tempStream.ToArray();
            Assert.Equal(65538u, BitConverter.ToUInt32(blob, 16));

            tempStream.Position = 0;
            var reader = new BinaryStreamReader(tempStream);
            var roundTripped = StrongNamePrivateKey.FromReader(ref reader);

            Assert.Equal(65538u, roundTripped.PublicExponent);
            Assert.Equal(rsaParams.Exponent, roundTripped.ToRsaParameters().Exponent);
        }

        private static byte[] SignWithRsa(RSA rsa, byte[] hash)
        {
            var formatter = new RSAPKCS1SignatureFormatter(rsa);
            formatter.SetHashAlgorithm("SHA1");
            var signature = formatter.CreateSignature(hash);
            Array.Reverse(signature);
            return signature;
        }

        private static byte[] ReverseBytes(byte[]? data)
        {
            Assert.NotNull(data);
            var result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = data[data.Length - 1 - i];
            return result;
        }
    }
}
