using Xunit;

namespace AsmResolver.DotNet.Tests.Memory
{
    public class MixedStructLayoutTest : StructLayoutTestBase
    {
        public MixedStructLayoutTest(CurrentModuleFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void ExplicitStructWithSequentialStruct() => VerifySize<MixedTestStructs.ExplicitStructWithSequentialStruct>();

        [Fact]
        public void ExplicitStructWithTwoSequentialStructs() => VerifySize<MixedTestStructs.ExplicitStructWithTwoSequentialStructs>();

        [Fact]
        public void ExtremeStruct1() => VerifySize<MixedTestStructs.ExtremeStruct1>();
        
        [Fact]
        public void ExtremeStruct2() => VerifySize<MixedTestStructs.ExtremeStruct2>();
        
        [Fact]
        public void ExtremeStruct3() => VerifySize<MixedTestStructs.ExtremeStruct3>();
    }
}