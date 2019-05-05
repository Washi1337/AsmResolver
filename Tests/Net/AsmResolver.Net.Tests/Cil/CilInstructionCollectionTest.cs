using System;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using Xunit;

namespace AsmResolver.Tests.Net.Cil
{
    public class CilInstructionCollectionTest
    {
        private const string DummyAssemblyName = "SomeAssemblyName";
        private readonly SignatureComparer _comparer = new SignatureComparer();

        private static CilInstructionCollection CreateDummyCollection(bool @static)
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();

            var type = new TypeDefinition("SomeType", "SomeMethod");
            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var method = new MethodDefinition("SomeMethod",
                MethodAttributes.Public,
                new MethodSignature(image.TypeSystem.Void) {HasThis = !@static});
            method.IsStatic = @static;
            type.Methods.Add(method);

            var methodBody = new CilMethodBody(method);
            method.MethodBody = methodBody;
            return methodBody.Instructions;
        }

        [Fact]
        public void OptimizeMacrosArguments()
        {
            var instructions = CreateDummyCollection(true);
            var method = instructions.Owner.Method;

            for (int i = 0; i <= 256; i++)
                method.Signature.Parameters.Add(new ParameterSignature(method.Image.TypeSystem.Object));

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldarg, method.Signature.Parameters[0]),
                CilInstruction.Create(CilOpCodes.Ldarg, method.Signature.Parameters[1]),
                CilInstruction.Create(CilOpCodes.Ldarg, method.Signature.Parameters[255]),
                CilInstruction.Create(CilOpCodes.Ldarg, method.Signature.Parameters[256]),
            });

            instructions.OptimizeMacros();

            Assert.Equal(CilCode.Ldarg_0, instructions[0].OpCode.Code);
            Assert.Null(instructions[0].Operand);
            
            Assert.Equal(CilCode.Ldarg_1, instructions[1].OpCode.Code);
            Assert.Null(instructions[1].Operand);
            
            Assert.Equal(CilCode.Ldarg_S, instructions[2].OpCode.Code);
            Assert.Equal(method.Signature.Parameters[255], instructions[2].Operand);
            
            Assert.Equal(CilCode.Ldarg, instructions[3].OpCode.Code);
            Assert.Equal(method.Signature.Parameters[256], instructions[3].Operand);
        }

        [Fact]
        public void OptimizeMacrosThisParameter()
        {
            var instructions = CreateDummyCollection(false);
            var method = instructions.Owner.Method;

            for (int i = 0; i < 2; i++)
                method.Signature.Parameters.Add(new ParameterSignature(method.Image.TypeSystem.Object));

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldarg, method.CilMethodBody.ThisParameter),
                CilInstruction.Create(CilOpCodes.Ldarg, method.Signature.Parameters[0]),
                CilInstruction.Create(CilOpCodes.Ldarg, method.Signature.Parameters[1]),
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(CilCode.Ldarg_0, instructions[0].OpCode.Code);
            Assert.Null(instructions[0].Operand);
            Assert.Equal(CilCode.Ldarg_1, instructions[1].OpCode.Code);
            Assert.Null(instructions[1].Operand);
            Assert.Equal(CilCode.Ldarg_2, instructions[2].OpCode.Code);
            Assert.Null(instructions[2].Operand);
        }

    }
    
    
}