using System.Collections.Generic;
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
        const List<(int a, string b)> some = null;
        var mod = ModuleDefinition.FromFile(typeof(PortablePdbTest).Assembly.Location);
        var pdb = ((PortablePdbSymbolReader)((SerializedModuleDefinition)mod).SymbolReader).Pdb;
        var thisMethod = mod.LookupMember<MethodDefinition>(typeof(PortablePdbTest).GetMethod("Test").MetadataToken);
        var m = pdb.EnumerateTableMembers(TableIndex.CustomDebugInformation).Cast<CustomDebugInformation>().Where(x => x.Owner is MethodDefinition { Name.Value: "Test" }).ToArray();
    }
}
