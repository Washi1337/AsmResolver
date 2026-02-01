using System.Linq;
using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.PortablePdbs;

public class PortablePdbTest
{
    [Fact]
    public void Test()
    {
        var mod = ModuleDefinition.FromFile(typeof(PortablePdbTest).Assembly.Location);
        var pdb = ((PortablePdbSymbolReader)((SerializedModuleDefinition)mod).SymbolReader).Pdb.EnumerateTableMembers(TableIndex.CustomDebugInformation).Cast<CustomDebugInformation>().ToArray();
    }
}
