using Xunit;

namespace AsmResolver.DotNet.Tests.PortablePdbs
{
    public class PortablePdbTest
    {
        [Fact]
        public void Test()
        {
            var mod = ModuleDefinition.FromFile(typeof(PortablePdbTest).Assembly.Location);
            var method = mod.LookupMember<MethodDefinition>(typeof(PortablePdbTest).GetMethod("Test").MetadataToken);
            var mdi = method.MethodDebugInformation;
        }
    }
}
