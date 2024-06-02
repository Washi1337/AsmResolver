using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class TypeSignatureTest
    {
        private readonly TypeDefinition _dummyType = new("Namespace", "Type", TypeAttributes.Class);

        [Fact]
        public void GetTypeDefOrRefFullName()
        {
            Assert.Equal("Namespace.Type", _dummyType.ToTypeSignature().FullName);
        }

        [Fact]
        public void GetArrayTypeFullName()
        {
            Assert.Equal("Namespace.Type[0...9, 0...19]", _dummyType
                .ToTypeSignature()
                .MakeArrayType(
                    new ArrayDimension(10),
                    new ArrayDimension(20))
                .FullName);
        }

        [Fact]
        public void GetByReferenceTypeFullName()
        {
            Assert.Equal("Namespace.Type&", _dummyType
                .ToTypeSignature()
                .MakeByReferenceType()
                .FullName);
        }

        [Fact]
        public void GetCorLibTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            Assert.Equal("System.String", module.CorLibTypeFactory.String.FullName);
        }

        [Fact]
        public void GetFunctionPointerTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            Assert.Equal("method System.String *(System.Object, System.Int32)",
                MethodSignature.CreateStatic(
                        module.CorLibTypeFactory.String,
                        module.CorLibTypeFactory.Object,
                        module.CorLibTypeFactory.Int32)
                    .MakeFunctionPointerType().FullName);
        }

        [Fact]
        public void GetInstanceFunctionPointerTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            Assert.Equal("method instance System.String *(System.Object, System.Int32)",
                MethodSignature.CreateInstance(
                        module.CorLibTypeFactory.String,
                        module.CorLibTypeFactory.Object,
                        module.CorLibTypeFactory.Int32)
                    .MakeFunctionPointerType().FullName);
        }

        [Fact]
        public void GetGenericInstanceTypeFullName()
        {
            var module = new ModuleDefinition("Dummy");
            var genericInstance = _dummyType.MakeGenericInstanceType(
                module.CorLibTypeFactory.Int32,
                _dummyType.MakeGenericInstanceType(module.CorLibTypeFactory.Object));

            Assert.Equal("Type<System.Int32, Namespace.Type<System.Object>>", genericInstance.Name);
            Assert.Equal("Namespace.Type<System.Int32, Namespace.Type<System.Object>>", genericInstance.FullName);
        }

        [Fact]
        public void GetGenericParameterFullName()
        {
            Assert.Equal("!2", new GenericParameterSignature(GenericParameterType.Type, 2).FullName);
            Assert.Equal("!!2", new GenericParameterSignature(GenericParameterType.Method, 2).FullName);
        }

        [Fact]
        public void GetPointerTypeFullName()
        {
            Assert.Equal("Namespace.Type*", _dummyType
                .ToTypeSignature()
                .MakePointerType()
                .FullName);
        }

        [Fact]
        public void GetSzArrayTypeFullName()
        {
            Assert.Equal("Namespace.Type[]", _dummyType
                .ToTypeSignature()
                .MakeSzArrayType()
                .FullName);
        }

        [Fact]
        public void GetRequiredModifierTypeFullName()
        {
            Assert.Equal("Namespace.Type modreq(Namespace.Type)",
                _dummyType
                    .ToTypeSignature()
                    .MakeModifierType(_dummyType, true)
                    .FullName);
        }

        [Fact]
        public void GetOptionalModifierTypeFullName()
        {
            Assert.Equal("Namespace.Type modopt(Namespace.Type)",
                _dummyType
                    .ToTypeSignature()
                    .MakeModifierType(_dummyType, false)
                    .FullName);
        }

        [Fact]
        public void StripModifiersPinnedType()
        {
            var type = _dummyType.ToTypeSignature();
            Assert.Equal(type, type.MakePinnedType().StripModifiers(), SignatureComparer.Default);
        }

        [Fact]
        public void StripModifiersCustomModifierType()
        {
            var type = _dummyType.ToTypeSignature();
            Assert.Equal(type, type.MakeModifierType(_dummyType, false).StripModifiers(), SignatureComparer.Default);
        }

        [Fact]
        public void StripMultipleModifiers()
        {
            var type = _dummyType.ToTypeSignature();
            Assert.Equal(type,
                type
                    .MakeModifierType(_dummyType, false)
                    .MakeModifierType(_dummyType, true)
                    .MakePinnedType()
                    .StripModifiers(),
                SignatureComparer.Default);
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I1)]
        [InlineData(ElementType.I2, ElementType.I2)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I1)]
        [InlineData(ElementType.U2, ElementType.I2)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.Boolean)]
        [InlineData(ElementType.Char, ElementType.Char)]
        public void GetReducedTypeOfPrimitive(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(expected, module.CorLibTypeFactory.FromElementType(type)!.GetReducedType().ElementType);
        }

        [Theory]
        [InlineData(typeof(Int32Enum), ElementType.I4)]
        [InlineData(typeof(Int64Enum), ElementType.I8)]
        public void GetReducedTypeOfEnum(Type type, ElementType expected)
        {
            var module = ModuleDefinition.FromFile(type.Assembly.Location);
            var signature = module.LookupMember<TypeDefinition>(type.MetadataToken).ToTypeSignature();
            Assert.Equal(expected, signature.GetReducedType().ElementType);
        }

        [Fact]
        public void GetReducedTypeOfNonPrimitive()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature(false);
            Assert.Equal(type, type.GetReducedType());
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I1)]
        [InlineData(ElementType.I2, ElementType.I2)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I1)]
        [InlineData(ElementType.U2, ElementType.I2)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.I1)]
        [InlineData(ElementType.Char, ElementType.I2)]
        public void GetVerificationTypeOfPrimitive(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(expected, module.CorLibTypeFactory.FromElementType(type)!.GetVerificationType().ElementType);
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I1)]
        [InlineData(ElementType.I2, ElementType.I2)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I1)]
        [InlineData(ElementType.U2, ElementType.I2)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.I1)]
        [InlineData(ElementType.Char, ElementType.I2)]
        public void GetVerificationTypeOfManagedPrimitivePointer(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var pointerType = module.CorLibTypeFactory.FromElementType(type)!.MakeByReferenceType();
            var actualType = Assert.IsAssignableFrom<ByReferenceTypeSignature>(pointerType.GetVerificationType());
            Assert.Equal(expected, actualType.BaseType.ElementType);
        }

        [Fact]
        public void GetVerificationTypeOfNonPrimitive()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature(false);
            Assert.Equal(type, type.GetVerificationType());
        }

        [Theory]
        [InlineData(ElementType.I, ElementType.I)]
        [InlineData(ElementType.I1, ElementType.I4)]
        [InlineData(ElementType.I2, ElementType.I4)]
        [InlineData(ElementType.I4, ElementType.I4)]
        [InlineData(ElementType.I8, ElementType.I8)]
        [InlineData(ElementType.U, ElementType.I)]
        [InlineData(ElementType.U1, ElementType.I4)]
        [InlineData(ElementType.U2, ElementType.I4)]
        [InlineData(ElementType.U4, ElementType.I4)]
        [InlineData(ElementType.U8, ElementType.I8)]
        [InlineData(ElementType.String, ElementType.String)]
        [InlineData(ElementType.Boolean, ElementType.I4)]
        [InlineData(ElementType.Char, ElementType.I4)]
        [InlineData(ElementType.R4, ElementType.R8)] // Technically incorrect, as it should be F.
        [InlineData(ElementType.R8, ElementType.R8)]
        public void GetIntermediateTypeOfPrimitive(ElementType type, ElementType expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(expected, module.CorLibTypeFactory.FromElementType(type)!.GetIntermediateType().ElementType);
        }

        [Fact]
        public void GetIntermediateTypeOfNonPrimitive()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature(false);
            Assert.Equal(type, type.GetIntermediateType());
        }

        [Fact]
        public void GetDirectBaseClassOfArrayType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var signature = module.CorLibTypeFactory.Object.MakeArrayType(3);
            Assert.Equal("System.Array", signature.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfSzArrayType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var signature = module.CorLibTypeFactory.Object.MakeSzArrayType();
            Assert.Equal("System.Array", signature.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfInterfaceType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var interfaceDefinition = new TypeDefinition("Namespace", "IName", TypeAttributes.Interface);
            module.TopLevelTypes.Add(interfaceDefinition);

            var interfaceSignature = interfaceDefinition.ToTypeSignature(false);

            Assert.Equal("System.Object", interfaceSignature.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfNormalType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeSignature();
            Assert.Equal("System.Object", type.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfNormalTypeWithBaseType()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedClass).Assembly.Location);
            var type = module.LookupMember<TypeDefinition>(typeof(DerivedClass).MetadataToken);
            Assert.Equal(type.BaseType!.FullName, type.ToTypeSignature().GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfGenericTypeInstance()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location);
            var genericInstanceType = module.LookupMember<TypeDefinition>(typeof(GenericType<,,>).MetadataToken)
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.String,
                    module.CorLibTypeFactory.Object);

            Assert.Equal("System.Object", genericInstanceType.GetDirectBaseClass()!.FullName);
        }

        [Fact]
        public void GetDirectBaseClassOfGenericTypeInstanceWithGenericBaseClass()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericDerivedType<,>).Assembly.Location);
            var genericInstanceType = module.LookupMember<TypeDefinition>(typeof(GenericDerivedType<,>).MetadataToken)
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.Object);

            var baseClass = Assert.IsAssignableFrom<GenericInstanceTypeSignature>(
                genericInstanceType.GetDirectBaseClass());

            Assert.Equal(typeof(GenericType<,,>).Namespace, baseClass.GenericType.Namespace);
            Assert.Equal(typeof(GenericType<,,>).Name, baseClass.GenericType.Name);
            Assert.Equal(new[]
            {
                "System.Int32",
                "System.Object",
                "System.String"
            }, baseClass.TypeArguments.Select(t => t.FullName));
        }

        [Theory]
        [InlineData(ElementType.I)]
        [InlineData(ElementType.I1)]
        [InlineData(ElementType.I2)]
        [InlineData(ElementType.I4)]
        [InlineData(ElementType.I8)]
        [InlineData(ElementType.U)]
        [InlineData(ElementType.U1)]
        [InlineData(ElementType.U2)]
        [InlineData(ElementType.U4)]
        [InlineData(ElementType.U8)]
        [InlineData(ElementType.R4)]
        [InlineData(ElementType.R8)]
        [InlineData(ElementType.String)]
        [InlineData(ElementType.Boolean)]
        [InlineData(ElementType.Char)]
        public void IsCompatibleWithIdenticalPrimitiveTypes(ElementType elementType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.CorLibTypeFactory.FromElementType(elementType)!;
            Assert.True(type.IsCompatibleWith(type));
        }

        [Theory]
        [InlineData(typeof(AbstractClass))]
        [InlineData(typeof(DerivedClass))]
        public void IsCompatibleWithIdenticalUserTypes(Type type)
        {
            var module = ModuleDefinition.FromFile(type.Assembly.Location);
            var signature = module.LookupMember<TypeDefinition>(type.MetadataToken).ToTypeSignature();
            Assert.True(signature.IsCompatibleWith(signature));
        }

        [Theory]
        [InlineData(typeof(DerivedClass), typeof(AbstractClass), true)]
        [InlineData(typeof(DerivedDerivedClass), typeof(DerivedClass), true)]
        [InlineData(typeof(DerivedDerivedClass), typeof(AbstractClass), true)]
        [InlineData(typeof(AbstractClass), typeof(DerivedClass), false)]
        [InlineData(typeof(AbstractClass), typeof(DerivedDerivedClass), false)]
        public void IsCompatibleWithBaseClass(Type derivedType, Type baseType, bool expected)
        {
            var module = ModuleDefinition.FromFile(derivedType.Assembly.Location);
            var derivedSignature = module.LookupMember<TypeDefinition>(derivedType.MetadataToken).ToTypeSignature();
            var abstractSignature = module.LookupMember<TypeDefinition>(baseType.MetadataToken).ToTypeSignature();
            Assert.Equal(expected, derivedSignature.IsCompatibleWith(abstractSignature));
        }

        [Theory]
        [InlineData(typeof(InterfaceImplementations), typeof(IInterface1), true)]
        [InlineData(typeof(InterfaceImplementations), typeof(IInterface2), true)]
        [InlineData(typeof(InterfaceImplementations), typeof(IInterface3), false)]
        [InlineData(typeof(InterfaceImplementations), typeof(IInterface4), false)]
        [InlineData(typeof(DerivedInterfaceImplementations), typeof(IInterface1), true)]
        [InlineData(typeof(DerivedInterfaceImplementations), typeof(IInterface2), true)]
        [InlineData(typeof(DerivedInterfaceImplementations), typeof(IInterface3), true)]
        [InlineData(typeof(DerivedInterfaceImplementations), typeof(IInterface4), false)]
        [InlineData(typeof(IInterface1), typeof(InterfaceImplementations), false)]
        [InlineData(typeof(IInterface2), typeof(InterfaceImplementations), false)]
        [InlineData(typeof(IInterface3), typeof(DerivedInterfaceImplementations), false)]
        [InlineData(typeof(IInterface4), typeof(DerivedInterfaceImplementations), false)]
        public void IsCompatibleWithInterface(Type derivedType, Type interfaceType, bool expected)
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedClass).Assembly.Location);
            var derivedSignature = module.LookupMember<TypeDefinition>(derivedType.MetadataToken).ToTypeSignature();
            var interfaceSignature = module.LookupMember<TypeDefinition>(interfaceType.MetadataToken).ToTypeSignature();
            Assert.Equal(expected, derivedSignature.IsCompatibleWith(interfaceSignature));
        }

        [Theory]
        [InlineData(new[] { ElementType.I4, ElementType.I4 }, new[] { ElementType.I4, ElementType.I4, ElementType.String }, true)]
        [InlineData(new[] { ElementType.I4, ElementType.I8 }, new[] { ElementType.I4, ElementType.I4, ElementType.String }, false)]
        [InlineData(new[] { ElementType.I4, ElementType.I4 }, new[] { ElementType.I4, ElementType.I8, ElementType.String }, false)]
        [InlineData(new[] { ElementType.I4, ElementType.I4 }, new[] { ElementType.I4, ElementType.I4, ElementType.Boolean }, false)]
        public void IsCompatibleWithGenericInterface(ElementType[] typeArguments1, ElementType[] typeArguments2, bool expected)
        {
            var module = ModuleDefinition.FromFile(typeof(GenericInterfaceImplementation<,>).Assembly.Location);

            var type1 = module.LookupMember<TypeDefinition>(typeof(GenericInterfaceImplementation<,>).MetadataToken)
                .ToTypeSignature(false)
                .MakeGenericInstanceType(
                    typeArguments1.Select(x => (TypeSignature) module.CorLibTypeFactory.FromElementType(x)!).ToArray()
                );

            var type2 = module.LookupMember<TypeDefinition>(typeof(IGenericInterface<,,>).MetadataToken)
                .ToTypeSignature(false)
                .MakeGenericInstanceType(
                    typeArguments2.Select(x => (TypeSignature) module.CorLibTypeFactory.FromElementType(x)!).ToArray()
                );

            Assert.Equal(expected, type1.IsCompatibleWith(type2));
        }

        [Theory]
        [InlineData(ElementType.I1, ElementType.I1, true)]
        [InlineData(ElementType.U1, ElementType.I1, true)]
        [InlineData(ElementType.I1, ElementType.U1, true)]
        [InlineData(ElementType.U1, ElementType.U1, true)]
        [InlineData(ElementType.I1, ElementType.U2, false)]
        [InlineData(ElementType.U2, ElementType.U1, false)]
        [InlineData(ElementType.I4, ElementType.I4, true)]
        [InlineData(ElementType.I4, ElementType.U4, true)]
        [InlineData(ElementType.U4, ElementType.I4, true)]
        public void IsCompatibleWithArray(ElementType elementType1, ElementType elementType2, bool expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type1 = module.CorLibTypeFactory.FromElementType(elementType1)!.MakeSzArrayType();
            var type2 = module.CorLibTypeFactory.FromElementType(elementType2)!.MakeSzArrayType();
            Assert.Equal(expected, type1.IsCompatibleWith(type2));
        }

        [Theory]
        [InlineData(ElementType.I1, ElementType.I1, true)]
        [InlineData(ElementType.U1, ElementType.I1, true)]
        [InlineData(ElementType.I1, ElementType.U1, true)]
        [InlineData(ElementType.U1, ElementType.U1, true)]
        [InlineData(ElementType.I1, ElementType.U2, false)]
        [InlineData(ElementType.U2, ElementType.U1, false)]
        [InlineData(ElementType.I4, ElementType.I4, true)]
        [InlineData(ElementType.I4, ElementType.U4, true)]
        [InlineData(ElementType.U4, ElementType.I4, true)]
        public void IsCompatibleWithArrayAndIList(ElementType elementType1, ElementType elementType2, bool expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var type1 = module.CorLibTypeFactory.FromElementType(elementType1)!.MakeSzArrayType();
            var type2 = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System.Collections.Generic", "IList`1")
                .ToTypeSignature(false)
                .MakeGenericInstanceType(module.CorLibTypeFactory.FromElementType(elementType2)!);

            Assert.Equal(expected, type1.IsCompatibleWith(type2));
        }

        [Fact]
        public void IsCompatibleWithGenericInstanceAndObject()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location);

            var type1 = module
                .LookupMember<TypeDefinition>(typeof(GenericType<,,>).MetadataToken)
                .ToTypeSignature()
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.Object,
                    module.CorLibTypeFactory.String);

            Assert.True(type1.IsCompatibleWith(type1));
            Assert.True(type1.IsCompatibleWith(module.CorLibTypeFactory.Object));
        }

        [Fact]
        public void IsCompatibleWithGenericInstance()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericDerivedType<,>).Assembly.Location);

            var type1 = module
                .LookupMember<TypeDefinition>(typeof(GenericDerivedType<,>).MetadataToken)
                .ToTypeSignature()
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.Object);

            var type2 = module
                .LookupMember<TypeDefinition>(typeof(GenericType<,,>).MetadataToken)
                .ToTypeSignature()
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.Object,
                    module.CorLibTypeFactory.String);

            var type3 = module
                .LookupMember<TypeDefinition>(typeof(GenericType<,,>).MetadataToken)
                .ToTypeSignature()
                .MakeGenericInstanceType(
                    module.CorLibTypeFactory.Object,
                    module.CorLibTypeFactory.Int32,
                    module.CorLibTypeFactory.String);

            Assert.True(type1.IsCompatibleWith(type2));
            Assert.False(type1.IsCompatibleWith(type3));
        }

        [Theory]
        [InlineData(ElementType.I1, ElementType.I1, true)]
        [InlineData(ElementType.U1, ElementType.I1, true)]
        [InlineData(ElementType.I1, ElementType.U1, true)]
        [InlineData(ElementType.I1, ElementType.Boolean, true)]
        [InlineData(ElementType.I2, ElementType.Char, true)]
        [InlineData(ElementType.I4, ElementType.Boolean, false)]
        [InlineData(ElementType.I1, ElementType.U2, false)]
        public void IsCompatibleWithPointers(ElementType elementType1, ElementType elementType2, bool expected)
        {
            var module = ModuleDefinition.FromFile(typeof(GenericDerivedType<,>).Assembly.Location);

            var type1 = module.CorLibTypeFactory.FromElementType(elementType1)!.MakePointerType();
            var type2 = module.CorLibTypeFactory.FromElementType(elementType2)!.MakePointerType();

            Assert.Equal(expected, type1.IsCompatibleWith(type2));
        }

        [Theory]
        [InlineData(ElementType.I1, ElementType.I4, true)]
        [InlineData(ElementType.I2, ElementType.I4, true)]
        [InlineData(ElementType.I4, ElementType.I4, true)]
        [InlineData(ElementType.I8, ElementType.I4, false)]
        [InlineData(ElementType.I4, ElementType.I1, true)]
        [InlineData(ElementType.I4, ElementType.I2, true)]
        [InlineData(ElementType.I4, ElementType.I8, false)]
        [InlineData(ElementType.I, ElementType.I4, true)]
        [InlineData(ElementType.I4, ElementType.I, true)]
        public void IsAssignablePrimitives(ElementType elementType1, ElementType elementType2, bool expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var type1 = module.CorLibTypeFactory.FromElementType(elementType1)!;
            var type2 = module.CorLibTypeFactory.FromElementType(elementType2)!;

            Assert.Equal(expected, type1.IsAssignableTo(type2));
        }

        [Fact]
        public void IgnoreCustomModifiers()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var type1 = module.CorLibTypeFactory.Int32;
            var type2 = module.CorLibTypeFactory.Int32.MakeModifierType(module.CorLibTypeFactory.CorLibScope
                    .CreateTypeReference("System.Runtime.CompilerServices", "IsVolatile")
                    .ImportWith(module.DefaultImporter),
                true);

            Assert.True(type1.IsCompatibleWith(type2));
            Assert.True(type2.IsCompatibleWith(type1));
        }

        [Fact]
        public void IgnoreNestedCustomModifiers()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var type1 = module.CorLibTypeFactory.Int32;
            var type2 = module.CorLibTypeFactory.Int32.MakeModifierType(module.CorLibTypeFactory.CorLibScope
                    .CreateTypeReference("System.Runtime.CompilerServices", "IsVolatile")
                    .ImportWith(module.DefaultImporter),
                true);

            var genericType = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System.Collections.Generic", "List`1")
                .ImportWith(module.DefaultImporter);

            var genericType1 = genericType.MakeGenericInstanceType(type1);
            var genericType2 = genericType.MakeGenericInstanceType(type2);

            Assert.True(genericType1.IsCompatibleWith(genericType2));
            Assert.True(genericType2.IsCompatibleWith(genericType1));
        }

        [Fact]
        public void IgnorePinnedModifiers()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var type1 = module.CorLibTypeFactory.Int32;
            var type2 = module.CorLibTypeFactory.Int32.MakePinnedType();

            Assert.True(type1.IsCompatibleWith(type2));
            Assert.True(type2.IsCompatibleWith(type1));
        }

        [Fact]
        public void GetModuleOfTypeDefOrRef()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var signature = module.GetOrCreateModuleType().ToTypeSignature();
            Assert.Same(module, signature.Module);
        }

        [Fact]
        public void GetModuleOfTypeDefOrRefWithNullScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefNullScope_CurrentModule);
            var signature = module
                .LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 2))
                .ToTypeSignature();

            Assert.Null(signature.Scope);
            Assert.Same(module, signature.Module);
        }

        [Fact]
        public void GetModuleOfSpecificationTypeWithNullScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefNullScope_CurrentModule);
            var signature = module
                .LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 2))
                .ToTypeSignature()
                .MakeSzArrayType();

            Assert.Null(signature.Scope);
            Assert.Same(module, signature.Module);
        }
    }
}
