using System.IO;
using System.Security.Cryptography;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.StrongName;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.StrongName
{
    public class StrongNamePublicKeyTest
    {
        [Fact]
        public void PersistentStrongNamePublicKey()
        {
            using var rsa = RSA.Create();
            var rsaParameters = rsa.ExportParameters(true);
            var publicKey = new StrongNamePublicKey(rsaParameters);

            using var tempStream = new MemoryStream();
            publicKey.Write(new BinaryStreamWriter(tempStream));

            var reader = ByteArrayInputFile.CreateReader(tempStream.ToArray());
            var newPublicKey = StrongNamePublicKey.FromReader(ref reader);

            Assert.Equal(publicKey.Modulus, newPublicKey.Modulus);
            Assert.Equal(publicKey.PublicExponent, newPublicKey.PublicExponent);
        }
    }
}
