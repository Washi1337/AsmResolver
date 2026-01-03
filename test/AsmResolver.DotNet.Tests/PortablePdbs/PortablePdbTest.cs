using System.Collections.Generic;
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
            var method = mod.LookupMember<MethodDefinition>(typeof(PortablePdbTest).GetMethod("Test").MetadataToken);
            var scope = method.LocalScopes[0].LocalConstants[0].Signature;
            const decimal someT = 12.5m;
        }
    }
}
