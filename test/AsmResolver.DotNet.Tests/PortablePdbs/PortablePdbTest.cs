using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.PortablePdbs.CustomRecords;
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
        var stateMachineMethod = mod.LookupMember<MethodDefinition>(typeof(PortablePdbTest).GetMethod("TestStateMachine").MetadataToken);
        var asyncMethod = mod.LookupMember<MethodDefinition>(typeof(PortablePdbTest).GetMethod("TestAsyncMethod").MetadataToken);
        var records = pdb.EnumerateTableMembers(TableIndex.CustomDebugInformation).Cast<CustomDebugInformation>().ToArray();
    }

    public static IEnumerable<int> TestStateMachine()
    {
        var a = 1;
        yield return a;
    }

    public static async Task<int> TestAsyncMethod()
    {
        var a = 1;
        try
        {
            await Task.Yield();
        }
        catch (ArgumentException e)
        {
            await Task.Yield();
            Console.WriteLine(e);
        }
        return a;
    }
}
