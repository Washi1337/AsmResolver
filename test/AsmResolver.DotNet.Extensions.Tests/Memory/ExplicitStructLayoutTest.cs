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
        public void SingleFieldStructDefaultPackImplicitSize() => 
            VerifySize<ExplicitTestStructs.SingleFieldStructDefaultPackImplicitSize>();
        
        [Fact]
        public void MultipleFieldsStructDefaultPackImplicitSize() => 
            VerifySize<ExplicitTestStructs.MultipleFieldsStructDefaultPackImplicitSize>();
        
        [Fact]
        public void OverlappingFieldsStructDefaultPackImplicitSize() => 
            VerifySize<ExplicitTestStructs.OverlappingFieldsStructDefaultPackImplicitSize>();
        
        [Fact]
        public void OverlappingFieldsStructDefaultPackExplicitSize() => 
            VerifySize<ExplicitTestStructs.OverlappingFieldsStructDefaultPackExplicitSize>();
        
        [Fact]
        public void NestedStruct() => 
            VerifySize<ExplicitTestStructs.NestedStruct>();
        
        [Fact]
        public void NestedStructOverlapping() => 
            VerifySize<ExplicitTestStructs.NestedStructOverlapping>();
        
        [Fact]
        public void NestedStructInNestedStructOverlapping() => 
            VerifySize<ExplicitTestStructs.NestedStructInNestedStructOverlapping>();

    }
}