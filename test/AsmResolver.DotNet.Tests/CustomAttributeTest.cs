using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.DotNet.TestCases.Properties;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class CustomAttributeTest
    {
        private readonly SignatureComparer _comparer = new();

        [Fact]
        public void ReadConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));

            Assert.All(type.CustomAttributes, a =>
                Assert.Equal(nameof(TestCaseAttribute), a.Constructor!.DeclaringType!.Name));
        }

        [Fact]
        public void PersistentConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            using var stream = new MemoryStream();
            module.Write(stream);

            module = ModuleDefinition.FromReader(new BinaryStreamReader(stream.ToArray()));

            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            Assert.All(type.CustomAttributes, a =>
                Assert.Equal(nameof(TestCaseAttribute), a.Constructor!.DeclaringType!.Name));
        }

        [Fact]
        public void ReadParent()
        {
            int parentToken = typeof(CustomAttributesTestClass).MetadataToken;
            string filePath = typeof(CustomAttributesTestClass).Assembly.Location;

            var image = PEImage.FromFile(filePath);
            var tablesStream = image.DotNetDirectory!.Metadata!.GetStream<TablesStream>();
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

        private static CustomAttribute GetCustomAttributeTestCase(
            string methodName,
            bool rebuild = false,
            bool access = false,
            bool generic = false)
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            var method = type.Methods.First(m => m.Name == methodName);

            string attributeName = nameof(TestCaseAttribute);
            if (generic)
                attributeName += "`1";

            var attribute = method.CustomAttributes
                .First(c => c.Constructor!.DeclaringType!.Name!.Value.StartsWith(attributeName));

            if (access)
            {
                _ = attribute.Signature!.FixedArguments;
                _ = attribute.Signature.NamedArguments;
            }

            if (rebuild)
                attribute = RebuildAndLookup(attribute);
            return attribute;
        }

        private static CustomAttribute RebuildAndLookup(CustomAttribute attribute)
        {
            var stream = new MemoryStream();
            var method = (MethodDefinition) attribute.Parent!;
            method.Module!.Write(stream);
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());

            return newModule
                .TopLevelTypes.First(t => t.FullName == method.DeclaringType!.FullName)
                .Methods.First(f => f.Name == method.Name)
                .CustomAttributes[0];
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedInt32Argument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(
                nameof(CustomAttributesTestClass.FixedInt32Argument),
                rebuild,
                access);

            Assert.Single(attribute.Signature!.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal(1, argument.Element);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedStringArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedStringArgument),rebuild, access);
            Assert.Single(attribute.Signature!.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal("String fixed arg",  Assert.IsAssignableFrom<Utf8String>(argument.Element));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedEnumArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedEnumArgument),rebuild, access);
            Assert.Single(attribute.Signature!.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal((int) TestEnum.Value3, argument.Element);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedNullTypeArgument(bool rebuild, bool access)
        {
            var attribute =  GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedNullTypeArgument),rebuild, access);
            var fixedArg = Assert.Single(attribute.Signature!.FixedArguments);
            Assert.Null(fixedArg.Element);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedTypeArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedTypeArgument),rebuild, access);
            Assert.Single(attribute.Signature!.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            Assert.Equal(
                attribute.Constructor!.Module!.CorLibTypeFactory.String,
                argument.Element as TypeSignature, _comparer);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedComplexTypeArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedComplexTypeArgument),rebuild, access);
            Assert.Single(attribute.Signature!.FixedArguments);
            Assert.Empty(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.FixedArguments[0];
            var factory = attribute.Constructor!.Module!.CorLibTypeFactory;

            var instance = factory.CorLibScope
                .CreateTypeReference("System.Collections.Generic", "KeyValuePair`2")
                .MakeGenericInstanceType(
                    false,
                    factory.String.MakeSzArrayType(),
                    factory.Int32.MakeSzArrayType()
                );

            Assert.Equal(instance, argument.Element as TypeSignature, _comparer);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedInt32Argument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedInt32Argument),rebuild, access);
            Assert.Empty(attribute.Signature!.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.NamedArguments[0];
            Assert.Equal(nameof(TestCaseAttribute.IntValue), argument.MemberName);
            Assert.Equal(2, argument.Argument.Element);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedStringArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedStringArgument),rebuild, access);
            Assert.Empty(attribute.Signature!.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.NamedArguments[0];
            Assert.Equal(nameof(TestCaseAttribute.StringValue), argument.MemberName);
            Assert.Equal("String named arg", Assert.IsAssignableFrom<Utf8String>(argument.Argument.Element));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedEnumArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedEnumArgument),rebuild, access);
            Assert.Empty(attribute.Signature!.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var argument = attribute.Signature.NamedArguments[0];
            Assert.Equal(nameof(TestCaseAttribute.EnumValue), argument.MemberName);
            Assert.Equal((int) TestEnum.Value2, argument.Argument.Element);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedTypeArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedTypeArgument),rebuild, access);
            Assert.Empty(attribute.Signature!.FixedArguments);
            Assert.Single(attribute.Signature.NamedArguments);

            var expected = new TypeReference(
                attribute.Constructor!.Module!.CorLibTypeFactory.CorLibScope,
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
            var setMethod = property.SetMethod!;

            Assert.True(setMethod.IsCompilerGenerated());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void GenericTypeArgument(bool rebuild, bool access)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92

            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.GenericType),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            var module = attribute.Constructor!.Module!;
            var nestedClass = (TypeDefinition) module.LookupMember(typeof(TestGenericType<>).MetadataToken);
            var expected = nestedClass.MakeGenericInstanceType(false, module.CorLibTypeFactory.Object);

            var element = Assert.IsAssignableFrom<TypeSignature>(argument.Element);
            Assert.Equal(expected, element, _comparer);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void ArrayGenericTypeArgument(bool rebuild, bool access)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92

            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.GenericTypeArray),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            var module = attribute.Constructor!.Module!;
            var nestedClass = (TypeDefinition) module.LookupMember(typeof(TestGenericType<>).MetadataToken);
            var expected = nestedClass
                .MakeGenericInstanceType(false, module.CorLibTypeFactory.Object)
                .MakeSzArrayType();

            var element = Assert.IsAssignableFrom<TypeSignature>(argument.Element);
            Assert.Equal(expected, element, _comparer);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IntPassedOnAsObject(bool rebuild, bool access)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92

            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.Int32PassedAsObject),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            var element = Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Equal(123, element.Value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void TypePassedOnAsObject(bool rebuild, bool access)
        {
            // https://github.com/Washi1337/AsmResolver/issues/92

            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.TypePassedAsObject),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            var module = attribute.Constructor!.Module!;
            var element = Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Equal(module.CorLibTypeFactory.Int32, (ITypeDescriptor) element.Value, _comparer);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedInt32NullArray(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayNullArgument),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            Assert.True(argument.IsNullArray);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedInt32EmptyArray(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayEmptyArgument),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            Assert.False(argument.IsNullArray);
            Assert.Empty(argument.Elements);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedInt32Array(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayArgument),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            Assert.Equal(new[]
            {
                1, 2, 3, 4
            }, argument.Elements.Cast<int>());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedInt32ArrayNullAsObject(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayAsObjectNullArgument),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            var boxedArgument = Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Null(boxedArgument.Value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedInt32EmptyArrayAsObject(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayAsObjectEmptyArgument),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            var boxedArgument = Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Equal(Array.Empty<object>(), boxedArgument.Value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedInt32ArrayAsObject(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedInt32ArrayAsObjectArgument),rebuild, access);
            var argument = attribute.Signature!.FixedArguments[0];

            var boxedArgument = Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Equal(new[]
            {
                1, 2, 3, 4
            }, boxedArgument.Value);
        }

        [Fact]
        public void CreateNewWithFixedArgumentsViaConstructor()
        {
            var module = new ModuleDefinition("Module.exe");

            var attribute = new CustomAttribute(module.CorLibTypeFactory.CorLibScope
                    .CreateTypeReference("System", "ObsoleteAttribute")
                    .CreateMemberReference(".ctor", MethodSignature.CreateInstance(
                        module.CorLibTypeFactory.Void,
                        module.CorLibTypeFactory.String)),
                new CustomAttributeSignature(new CustomAttributeArgument(
                    module.CorLibTypeFactory.String,
                    "My Message")));

            Assert.NotNull(attribute.Signature);
            var argument = Assert.Single(attribute.Signature.FixedArguments);
            Assert.Equal("My Message", argument.Element);
        }

        [Fact]
        public void CreateNewWithFixedArgumentsViaProperty()
        {
            var module = new ModuleDefinition("Module.exe");

            var attribute = new CustomAttribute(module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System", "ObsoleteAttribute")
                .CreateMemberReference(".ctor", MethodSignature.CreateInstance(
                    module.CorLibTypeFactory.Void,
                    module.CorLibTypeFactory.String)));
            attribute.Signature!.FixedArguments.Add(new CustomAttributeArgument(
                module.CorLibTypeFactory.String,
                "My Message"));

            Assert.NotNull(attribute.Signature);
            var argument = Assert.Single(attribute.Signature.FixedArguments);
            Assert.Equal("My Message", argument.Element);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedGenericInt32Argument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedGenericInt32Argument),
                rebuild, access, true);
            var argument = attribute.Signature!.FixedArguments[0];

            int value = Assert.IsAssignableFrom<int>(argument.Element);
            Assert.Equal(1, value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedGenericStringArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedGenericStringArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.FixedArguments[0];

            string value = Assert.IsAssignableFrom<Utf8String>(argument.Element);
            Assert.Equal("Fixed string generic argument", value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedGenericInt32ArrayArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedGenericInt32ArrayArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.FixedArguments[0];

            Assert.Equal(new int[] {1, 2, 3, 4}, argument.Elements.Cast<int>());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedGenericInt32ArrayAsObjectArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedGenericInt32ArrayAsObjectArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.FixedArguments[0];

            var boxedArgument = Assert.IsAssignableFrom<BoxedArgument>(argument.Element);
            Assert.Equal(new[]
            {
                1, 2, 3, 4
            }, boxedArgument.Value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedGenericTypeArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedGenericTypeArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.FixedArguments[0];

            var expected = attribute.Constructor!.Module!.CorLibTypeFactory.Int32;
            var element = Assert.IsAssignableFrom<TypeSignature>(argument.Element);
            Assert.Equal(expected, element, _comparer);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void FixedGenericTypeNullArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.FixedGenericTypeNullArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.FixedArguments[0];

            Assert.Null(argument.Element);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedGenericInt32Argument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedGenericInt32Argument),
                rebuild, access, true);
            var argument = attribute.Signature!.NamedArguments[0];

            Assert.Equal("Value", argument.MemberName);
            int value = Assert.IsAssignableFrom<int>(argument.Argument.Element);
            Assert.Equal(1, value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedGenericStringArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedGenericStringArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.NamedArguments[0];

            Assert.Equal("Value", argument.MemberName);
            string value = Assert.IsAssignableFrom<Utf8String>(argument.Argument.Element);
            Assert.Equal("Named string generic argument", value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedGenericInt32ArrayArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedGenericInt32ArrayArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.NamedArguments[0];

            Assert.Equal("Value", argument.MemberName);
            Assert.Equal(new int[] {1,2,3,4}, argument.Argument.Elements.Cast<int>());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedGenericInt32ArrayAsObjectArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedGenericInt32ArrayAsObjectArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.NamedArguments[0];

            Assert.Equal("Value", argument.MemberName);
            var boxedArgument = Assert.IsAssignableFrom<BoxedArgument>(argument.Argument.Element);
            Assert.Equal(new[]
            {
                1, 2, 3, 4
            }, boxedArgument.Value);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedGenericTypeArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedGenericTypeArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.NamedArguments[0];

            var expected = attribute.Constructor!.Module!.CorLibTypeFactory.Int32;
            var element = Assert.IsAssignableFrom<TypeSignature>(argument.Argument.Element);
            Assert.Equal(expected, element, _comparer);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void NamedGenericTypeNullArgument(bool rebuild, bool access)
        {
            var attribute = GetCustomAttributeTestCase(nameof(CustomAttributesTestClass.NamedGenericTypeNullArgument),
                rebuild, access, true);
            var argument = attribute.Signature!.NamedArguments[0];

            Assert.Null(argument.Argument.Element);
        }

        [Fact]
        public void TestSignatureCompatibility()
        {
            var module = new ModuleDefinition("Dummy");
            var factory = module.CorLibTypeFactory;
            var ctor = factory.CorLibScope
                .CreateTypeReference("System", "CLSCompliantAttribute")
                .CreateMemberReference(".ctor", MethodSignature.CreateInstance(factory.Void, factory.Boolean))
                .ImportWith(module.DefaultImporter);

            var attribute = new CustomAttribute(ctor);

            // Empty signature is not compatible with a ctor that takes a boolean.
            Assert.False(attribute.Signature!.IsCompatibleWith(attribute.Constructor!));

            // If we add it, it should be compatible again.
            attribute.Signature.FixedArguments.Add(new CustomAttributeArgument(factory.Boolean, true));
            Assert.True(attribute.Signature!.IsCompatibleWith(attribute.Constructor!));
        }
    }
}
