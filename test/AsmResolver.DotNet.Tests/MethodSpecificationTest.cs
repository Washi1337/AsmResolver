using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MethodSpecificationTest
    {
        private readonly SignatureComparer _comparer = new();

        [Fact]
        public void ReadMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(typeof(GenericsTestClass)
                .GetMethod(nameof(GenericsTestClass.MethodInstantiationFromGenericType))!.MetadataToken);

            var call = method.CilMethodBody!.Instructions.First(i => i.OpCode.Code == CilCode.Call);
            Assert.IsAssignableFrom<MethodSpecification>(call.Operand);

            var specification = (MethodSpecification) call.Operand;
            Assert.Equal("GenericMethodInGenericType", specification.Method!.Name);
        }

        [Fact]
        public void ReadSignature()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(typeof(GenericsTestClass)
                .GetMethod(nameof(GenericsTestClass.MethodInstantiationFromGenericType))!.MetadataToken);

            var call = method.CilMethodBody!.Instructions.First(i => i.OpCode.Code == CilCode.Call);
            Assert.IsAssignableFrom<MethodSpecification>(call.Operand);

            var specification = (MethodSpecification) call.Operand;
            Assert.Equal(
                new GenericInstanceMethodSignature(
                    module.CorLibTypeFactory.SByte,
                    module.CorLibTypeFactory.Int16,
                    module.CorLibTypeFactory.Int32),
                specification.Signature,
                _comparer);
        }

        [Fact]
        public void InstantiateGenericMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(typeof(NonGenericType)
                .GetMethod(nameof(NonGenericType.GenericMethodInNonGenericType))!.MetadataToken);

            var specification = method.MakeGenericInstanceMethod(
                module.CorLibTypeFactory.SByte,
                module.CorLibTypeFactory.Int16,
                module.CorLibTypeFactory.Int32);

            Assert.Equal(
                new GenericInstanceMethodSignature(
                    module.CorLibTypeFactory.SByte,
                    module.CorLibTypeFactory.Int16,
                    module.CorLibTypeFactory.Int32),
                specification.Signature,
                _comparer);
        }

        [Fact]
        public void InstantiateGenericMethodWithWrongNumberOfArgumentsShouldThrow()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(typeof(NonGenericType)
                .GetMethod(nameof(NonGenericType.GenericMethodInNonGenericType))!.MetadataToken);

            Assert.Throws<ArgumentException>(() => method.MakeGenericInstanceMethod(
                module.CorLibTypeFactory.SByte,
                module.CorLibTypeFactory.Int16));
        }

        [Fact]
        public void InstantiateNonGenericMethodShouldThrow()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(typeof(NonGenericType)
                .GetMethod(nameof(NonGenericType.NonGenericMethodInNonGenericType))!.MetadataToken);

            Assert.Throws<ArgumentException>(() => method.MakeGenericInstanceMethod(
                module.CorLibTypeFactory.SByte,
                module.CorLibTypeFactory.Int16,
                module.CorLibTypeFactory.Int32));
        }
    }
}
