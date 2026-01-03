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
            foreach (var c in method.LocalScopes[0].LocalConstants)
            {
                _ = c.Signature;
            }
            const string s = "\u00ff";
            const string nullS = null;
        }
    }
}
