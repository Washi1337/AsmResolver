using System;
using Xunit;

namespace AsmResolver.DotNet.Tests.Memory
{
    public class ExplicitStructLayoutTest : StructLayoutTestBase
    {
        public ExplicitStructLayoutTest(CurrentModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void EmptyStruct() => VerifySize<ExplicitTestStructs.EmptyStruct>();

        [Fact]
        public void SingleFieldStructDefaultPackImplicitSize() => VerifySize<ExplicitTestStructs.SingleFieldStructDefaultPackImplicitSize>();

    }
}