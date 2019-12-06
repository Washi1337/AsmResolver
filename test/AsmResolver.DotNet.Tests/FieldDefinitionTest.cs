using System.Linq;
using AsmResolver.DotNet.TestCases.Fields;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class FieldDefinitionTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            Assert.Equal(nameof(SingleField.IntField), type.Fields[0].Name);
        }
    }
}