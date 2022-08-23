using AsmResolver.PE.DotNet.Metadata.Pdb;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata
{
    public class PdbStreamTest
    {
        [Fact]
        public void PdbStream()
        {
            var metadata = PE.DotNet.Metadata.Metadata.FromBytes(Properties.Resources.HelloWorldPortablePdb);
            var pdbStream = metadata.GetStream<PdbStream>();
            var tablesStream = metadata.GetStream<TablesStream>();
        }
    }
}
