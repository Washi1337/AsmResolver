using System.Linq;
using AsmResolver.DotNet.PortablePdbs.CustomRecords;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.PortablePdbs
{
    public class PortablePdbTest()
    {
        [Fact]
        public void Test()
        {
            var mod = ModuleDefinition.FromFile(typeof(PortablePdbTest).Assembly.Location);
            var doc = mod.Documents.Single(d => d.Name.Value.EndsWith("PortablePdbTest.cs")).CustomDebugInformations[0].Value;
        }
    }
}
