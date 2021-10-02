using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MethodSpecificationTest
    {
        private SignatureComparer _comparer = new SignatureComparer();

        [Fact]
        public void ReadMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(typeof(GenericsTestClass)
                .GetMethod(nameof(GenericsTestClass.MethodInstantiationFromGenericType)).MetadataToken);

            var call = method.CilMethodBody.Instructions.First(i => i.OpCode.Code == CilCode.Call);
            Assert.IsAssignableFrom<MethodSpecification>(call.Operand);

            var methodSpec = (MethodSpecification) call.Operand;
            Assert.Equal("GenericMethodInGenericType", methodSpec.Method.Name);
        }
        
        [Fact]
        public void ReadSignature()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(typeof(GenericsTestClass)
                .GetMethod(nameof(GenericsTestClass.MethodInstantiationFromGenericType)).MetadataToken);

            var call = method.CilMethodBody.Instructions.First(i => i.OpCode.Code == CilCode.Call);
            Assert.IsAssignableFrom<MethodSpecification>(call.Operand);

            var methodSpec = (MethodSpecification) call.Operand;
            Assert.Equal(
                new GenericInstanceMethodSignature(
                    module.CorLibTypeFactory.SByte,
                    module.CorLibTypeFactory.Int16,
                    module.CorLibTypeFactory.Int32),
                methodSpec.Signature,
                _comparer);
        }

    }
}
