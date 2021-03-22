using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class ReferenceRelationTest : IClassFixture<TestCasesFixture>
    {
        private readonly TestCasesFixture _fixture;

        public ReferenceRelationTest(TestCasesFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CustomAttributePropertyTest()
        {
            var module = _fixture.CustomAttributesAssembly.ManifestModule;
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            var method = type.Methods.First(m => m.Name == nameof(CustomAttributesTestClass.NamedInt32Argument));
            var customAttribute = method.CustomAttributes.First();
            var customAttributeNamedArgument = customAttribute.Signature.NamedArguments.First();


            var customAttributeType = module.TopLevelTypes.First(t => t.Name == nameof(TestCaseAttribute));
            var customAttributeProperty = customAttributeType.Properties.First(m => m.Name == nameof(TestCaseAttribute.IntValue));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.CustomAttributesAssembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(customAttributeProperty);
            Assert.Contains(customAttributeNamedArgument,
                node.GetRelatedObjects(DotNetRelations.ReferenceArgument));
        }

        [Fact]
        public void CustomAttributeFieldTest()
        {
            var module = _fixture.CustomAttributesAssembly.ManifestModule;
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            var method = type.Methods.First(m => m.Name == nameof(CustomAttributesTestClass.NamedInt32FieldArgument));
            var customAttribute = method.CustomAttributes.First();
            var customAttributeNamedArgument = customAttribute.Signature.NamedArguments.First();


            var customAttributeType = module.TopLevelTypes.First(t => t.Name == nameof(TestCaseAttribute));
            var customAttributeField = customAttributeType.Fields.First(m => m.Name == nameof(TestCaseAttribute.IntFieldValue));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.CustomAttributesAssembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(customAttributeField);
            Assert.Contains(customAttributeNamedArgument,
                node.GetRelatedObjects(DotNetRelations.ReferenceArgument));
        }
    }
}
