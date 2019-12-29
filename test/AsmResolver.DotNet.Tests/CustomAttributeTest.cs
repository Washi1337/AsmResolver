using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class CustomAttributeTest
    {
        [Fact]
        public void ReadConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));

            Assert.All(type.CustomAttributes, a =>
                Assert.Equal(nameof(TestCaseAttribute), a.Constructor.DeclaringType.Name));
        }

        [Fact]
        public void ReadParent()
        {
            int parentToken = typeof(CustomAttributesTestClass).MetadataToken;
            string filePath = typeof(CustomAttributesTestClass).Assembly.Location;
            
            var image = PEImage.FromFile(filePath);
            var tablesStream = image.DotNetDirectory.Metadata.GetStream<TablesStream>();
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);
            var attributeTable = tablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);

            // Find token of custom attribute
            var attributeToken = MetadataToken.Zero;
            for (int i = 0; i < attributeTable.Count && attributeToken == 0; i++)
            {
                var row = attributeTable[i];
                var token = encoder.DecodeIndex(row.Parent);
                if (token == parentToken) 
                    attributeToken = new MetadataToken(TableIndex.CustomAttribute, (uint) (i + 1));
            }
            
            // Resolve by token and verify parent (forcing parent to execute the lazy initialization ).
            var module = ModuleDefinition.FromFile(filePath);
            var attribute = (CustomAttribute) module.LookupMember(attributeToken);
            Assert.NotNull(attribute.Parent);
            Assert.IsAssignableFrom<TypeDefinition>(attribute.Parent);
            Assert.Equal(parentToken, attribute.Parent.MetadataToken);
        }

    }
}