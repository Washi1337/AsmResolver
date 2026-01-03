using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.PortablePdbs
{
    public class PortablePdbTest
    {
        [Fact]
        public void Test()
        {
            var mod = ModuleDefinition.FromFile(typeof(PortablePdbTest).Assembly.Location);
            var m1 = new MethodDefinition(null, MethodAttributes.Static, MethodSignature.CreateStatic(mod.CorLibTypeFactory.Void));
            var m2 = new MethodDefinition(null, MethodAttributes.Static, MethodSignature.CreateStatic(mod.CorLibTypeFactory.Void));

            m1.MoveNextMethod = m2;
        }
    }
}
