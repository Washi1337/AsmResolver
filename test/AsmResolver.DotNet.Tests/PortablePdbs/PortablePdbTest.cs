using Xunit;

namespace AsmResolver.DotNet.Tests.PortablePdbs;

public class PortablePdbTest
{
    [Fact]
    public void Test()
    {
        var mod = ModuleDefinition.FromFile("/home/aaronr/.nuget/packages/microsoft.codeanalysis.csharp/5.3.0-1.25619.109/lib/net9.0/Microsoft.CodeAnalysis.CSharp.dll");
    }
}
