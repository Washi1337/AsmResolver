using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeSpecificationTest
    {

        [Fact]
        public void ReadGenericTypeInstantiation()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location, TestReaderParameters);
            var fieldType = module
                .TopLevelTypes.First(t => t.Name == nameof(GenericsTestClass))
                .Fields.First(f => f.Name == nameof(GenericsTestClass.GenericField))
                .Signature.FieldType;

            Assert.IsAssignableFrom<GenericInstanceTypeSignature>(fieldType);
            var genericType = (GenericInstanceTypeSignature) fieldType;
            Assert.Equal("GenericType`3", genericType.GenericType.Name);
            Assert.Equal(new[]
            {
                "System.String", "System.Int32", "System.Object"
            }, genericType.TypeArguments.Select(a => a.FullName));
        }

        [Fact]
        public void PersistentGenericTypeInstantiation()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location, TestReaderParameters);

            using var tempStream = new MemoryStream();
            module.Write(tempStream);

            module = ModuleDefinition.FromBytes(tempStream.ToArray(), TestReaderParameters);
            var fieldType = module
                .TopLevelTypes.First(t => t.Name == nameof(GenericsTestClass))!
                .Fields.First(f => f.Name == nameof(GenericsTestClass.GenericField))!
                .Signature!.FieldType;

            Assert.IsAssignableFrom<GenericInstanceTypeSignature>(fieldType);
            var genericType = (GenericInstanceTypeSignature) fieldType;
            Assert.Equal("GenericType`3", genericType.GenericType.Name);
            Assert.Equal(new[]
            {
                "System.String", "System.Int32", "System.Object"
            }, genericType.TypeArguments.Select(a => a.FullName));
        }

        [Fact]
        public void IllegalTypeSpecInTypeDefOrRef()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_IllegalTypeSpecInTypeDefOrRefSig, TestReaderParameters);
            var typeSpec =  (TypeSpecification) module.LookupMember(new MetadataToken(TableIndex.TypeSpec, 1));
            Assert.NotNull(typeSpec);
        }

        [Fact]
        public void MaliciousTypeSpecLoop()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_MaliciousTypeSpecLoop,
                new ModuleReaderParameters(EmptyErrorListener.Instance));
            var typeSpec =  (TypeSpecification) module.LookupMember(new MetadataToken(TableIndex.TypeSpec, 1));
            Assert.NotNull(typeSpec.Signature);
        }

    }
}
