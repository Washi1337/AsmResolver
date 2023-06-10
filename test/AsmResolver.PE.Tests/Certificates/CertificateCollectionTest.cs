using AsmResolver.PE.Certificates;
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
        }
    }
}
