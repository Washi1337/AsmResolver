using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File.Headers;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class MethodSignatureTest
    {
        private readonly ModuleDefinition _module;

        public MethodSignatureTest()
        {
            _module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
        }

        [Fact]
        public void MakeInstanceShouldHaveHasThisFlagSet()
        {
            var signature = MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void);
            Assert.True(signature.HasThis);
            Assert.False(signature.IsGeneric);
        }

        [Fact]
        public void MakeStaticShouldNotHaveHasThisFlagSet()
        {
            var signature = MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void);
            Assert.False(signature.HasThis);
            Assert.False(signature.IsGeneric);
        }

        [Fact]
        public void MakeGenericInstanceShouldHaveHasThisAndGenericFlagSet()
        {
            var signature = MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void, 1);
            Assert.True(signature.HasThis);
            Assert.True(signature.IsGeneric);
        }

        [Fact]
        public void MakeGenericStaticShouldNotHaveHasThisAndGenericFlagSet()
        {
            var signature = MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void, 1);
            Assert.False(signature.HasThis);
            Assert.True(signature.IsGeneric);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SentinelParameterTypes(bool rebuild)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ArgListTest);
            if (rebuild)
            {
                using var stream = new MemoryStream();
                module.Write(stream);
                module = ModuleDefinition.FromBytes(stream.ToArray());
            }

            var reference = (MemberReference) module.ManagedEntryPointMethod!.CilMethodBody!
                .Instructions.First(i => i.OpCode.Code == CilCode.Call)
                .Operand!;

            var signature = Assert.IsAssignableFrom<MethodSignature>(reference.Signature);
            var type = Assert.Single(signature.SentinelParameterTypes);

            Assert.Equal(module.CorLibTypeFactory.String, type, SignatureComparer.Default);
        }
    }
}
