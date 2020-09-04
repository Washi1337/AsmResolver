using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.DotNet.TestCases.Properties;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class CustomAttributeTest
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();
        
        [Fact]
        public void ReadConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));

            Assert.All(type.CustomAttributes, a =>
                Assert.Equal(nameof(TestCaseAttribute), a.Constructor.DeclaringType.Name));
        }

        [Fact]
        public void PersistentConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            using var stream = new MemoryStream();
            module.Write(stream);
            
            module = ModuleDefinition.FromReader(new ByteArrayReader(stream.ToArray()));
            
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

        private static CustomAttribute GetCustomAttributeTestCase(string methodName, bool rebuild = false)
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            var method = type.Methods.First(m => m.Name == methodName);
            var attribute = method.CustomAttributes
                .First(c => c.Constructor.DeclaringType.Name == nameof(TestCaseAttribute));
            
            if (rebuild)
                attribute = RebuildAndLookup(attribute);
            return attribute;
        }

        private static CustomAttribute RebuildAndLookup(CustomAttribute attribute)
        {
            var stream = new MemoryStream();
            var method = (MethodDefinition) attribute.Parent;
            method.Module.Write(stream);
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());

            return newModule
                .TopLevelTypes.First(t => t.FullName == method.DeclaringType.FullName)
                .Methods.First(f => f.Name == method.Name)
                .CustomAttributes[0];
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedInt32Argument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32Argument), rebuild);
            
            Assert.Single(attribute.Signature.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal(1, argument.Element);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedStringArgument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedStringArgument), rebuild);
            Assert.Single(attribute.Signature.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal("String fixed arg", argument.Element);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedEnumArgument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedEnumArgument), rebuild);
            Assert.Single(attribute.Signature.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal((int) TestEnum.Value3, argument.Element);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedTypeArgument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedTypeArgument), rebuild);
            Assert.Single(attribute.Signature.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal(
                attribute.Constructor.Module.CorLibTypeFactory.String, 
                argument.Element as TypeSignature, _comparer);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedComplexTypeArgument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedComplexTypeArgument), rebuild);
            Assert.Single(attribute.Signature.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            var factory = attribute.Constructor.Module.CorLibTypeFactory;
            
            var listRef = new TypeReference(factory.CorLibScope, "System.Collections.Generic", "KeyValuePair`2");
            var instance = new GenericInstanceTypeSignature(listRef, false,
                new SzArrayTypeSignature(factory.String),
                new SzArrayTypeSignature(factory.Int32));

            Assert.Equal(instance, argument.Element as TypeSignature, _comparer);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void NamedInt32Argument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedInt32Argument), rebuild);
            Assert.Empty(attribute.Signature.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.NamedArguments[0];
            Assert.Equal(nameof(TestCaseAttribute.IntValue), argument.MemberName);
            Assert.Equal(2, argument.Argument.Element);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void NamedStringArgument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedStringArgument), rebuild);
            Assert.Empty(attribute.Signature.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.NamedArguments[0];
            Assert.Equal(nameof(TestCaseAttribute.StringValue), argument.MemberName);
            Assert.Equal("String named arg", argument.Argument.Element);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void NamedEnumArgument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedEnumArgument), rebuild);
            Assert.Empty(attribute.Signature.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.NamedArguments[0];
            Assert.Equal(nameof(TestCaseAttribute.EnumValue), argument.MemberName);
            Assert.Equal((int) TestEnum.Value2, argument.Argument.Element);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void NamedTypeArgument(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedTypeArgument), rebuild);
            Assert.Empty(attribute.Signature.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var expected = new TypeReference(
                attribute.Constructor.Module.CorLibTypeFactory.CorLibScope,
                "System", "Int32");
            
            var argument = attribute.Signature.NamedArguments[0];
            Assert.Equal(nameof(TestCaseAttribute.TypeValue), argument.MemberName);
            Assert.Equal(expected, (ITypeDescriptor) argument.Argument.Element, _comparer);
        }

        [Fact]
        public void IsCompilerGeneratedMember()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            var property = type.Properties.First();
            var setMethod = property.SetMethod;

            Assert.True(setMethod.IsCompilerGenerated());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GenericTypeArgument(bool rebuild)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92
            
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.GenericType), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            var module = attribute.Constructor.Module;
            var nestedClass = (TypeDefinition) module.LookupMember(typeof(TestGenericType<>).MetadataToken);
            var expected = new GenericInstanceTypeSignature(nestedClass, false, module.CorLibTypeFactory.Object);
            
            Assert.IsAssignableFrom<TypeSignature>(argument.Element);
            Assert.Equal(expected, (TypeSignature) argument.Element, _comparer);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ArrayGenericTypeArgument(bool rebuild)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92
            
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.GenericTypeArray), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            var module = attribute.Constructor.Module;
            var nestedClass = (TypeDefinition) module.LookupMember(typeof(TestGenericType<>).MetadataToken);
            var expected = new SzArrayTypeSignature(
                new GenericInstanceTypeSignature(nestedClass, false, module.CorLibTypeFactory.Object)
            );

            Assert.IsAssignableFrom<TypeSignature>(argument.Element);
            Assert.Equal(expected, (TypeSignature) argument.Element, _comparer);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IntPassedOnAsObject(bool rebuild)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92
            
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.Int32PassedAsObject), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Equal(123, ((BoxedArgument) argument.Element).Value);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TypePassedOnAsObject(bool rebuild)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92
            
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.TypePassedAsObject), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            var module = attribute.Constructor.Module;
            Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Equal(module.CorLibTypeFactory.Int32, (ITypeDescriptor) ((BoxedArgument) argument.Element).Value, _comparer);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedInt32NullArray(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayNullArgument), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            Assert.True(argument.IsNullArray);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedInt32EmptyArray(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayEmptyArgument), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            Assert.False(argument.IsNullArray);
            Assert.Empty(argument.Elements);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedInt32Array(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayArgument), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            Assert.Equal(new[]
            {
                1, 2, 3, 4
            }, argument.Elements.Cast<int>());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedInt32ArrayNullAsObject(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayAsObjectNullArgument), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            var boxedArgument = (BoxedArgument) argument.Element;
            Assert.Null(boxedArgument.Value);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedInt32EmptyArrayAsObject(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayAsObjectEmptyArgument), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            var boxedArgument = (BoxedArgument) argument.Element;
            Assert.Equal(new object[0], boxedArgument.Value);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FixedInt32ArrayAsObject(bool rebuild)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayAsObjectArgument), rebuild);
            var argument = attribute.Signature.FixedArguments[0];

            Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            var boxedArgument = (BoxedArgument) argument.Element;
            Assert.Equal(new[]
            {
                1, 2, 3, 4
            }, boxedArgument.Value);
        }
        
    }
}