using System.IO;
using AsmResolver.PE.Certificates;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.PE.Tests.Certificates
{
    public class CertificateCollectionTest
    {
        [Fact]
        public void ReadHeader()
        {
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_Signed);
            var certificate = Assert.Single(image.Certificates);
            Assert.Equal(CertificateRevision.Revision_v2_0, certificate.Revision);
            Assert.Equal(CertificateType.PkcsSignedData, certificate.Type);
            Assert.Equal(0x580u, certificate.CreateContentReader().Length);
        }

        [Fact]
        public void RebuildExistingSignature()
        {
            var file = PEFile.FromBytes(Properties.Resources.HelloWorld_Signed);

            var image = PEImage.FromFile(file);
            var certificate = Assert.Single(image.Certificates);
            file.EofData = image.Certificates;

            using var stream = new MemoryStream();
            file.Write(stream);

            var newImage = PEImage.FromBytes(stream.ToArray());
            var newCertificate = Assert.Single(newImage.Certificates);
            Assert.Equal(certificate.Revision, newCertificate.Revision);
            Assert.Equal(certificate.Type, newCertificate.Type);
            Assert.Equal(certificate.CreateContentReader().ReadToEnd(), newCertificate.CreateContentReader().ReadToEnd());
            Assert.Equal(certificate.WriteIntoArray(), newCertificate.WriteIntoArray());
        }
    }
}
