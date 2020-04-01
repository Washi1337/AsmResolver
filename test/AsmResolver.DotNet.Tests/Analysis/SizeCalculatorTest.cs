using Xunit;

namespace AsmResolver.DotNet.Tests.Analysis
{
    public struct CustomStruct
    {
        public int Dummy1;

        public int Dummy2;
    }
    
    public class SizeCalculatorTest
    {
        [Fact]
        public void CustomStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomStruct).MetadataToken);
            
            Assert.Equal(8, custom.CalculateSize());
        }
    }
}