using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Cil
{
    public class CilInstructionCollectionTest
    {
        private readonly ModuleDefinition _module;

        public CilInstructionCollectionTest()
        {
            _module = new ModuleDefinition("DummyModule");
        }

        private CilInstructionCollection CreateDummyMethod(bool hasThis, int paramCount, int localCount)
        {
            var flags = hasThis
                ? MethodAttributes.Public
                : MethodAttributes.Public | MethodAttributes.Static;
            
            var parameterTypes = Enumerable.Repeat<TypeSignature>(_module.CorLibTypeFactory.Object, paramCount);
            
            var signature = hasThis
                ? MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void, parameterTypes)
                : MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void, parameterTypes);

            var method = new MethodDefinition("Dummy", flags, signature);

            var body = new CilMethodBody(method);
            for (int i = 0; i < localCount; i++)
                body.LocalVariables.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));

            method.MethodBody = body;
            
            return body.Instructions;
        }

        [Theory]
        [InlineData(0, CilCode.Ldarg_0)]
        [InlineData(1, CilCode.Ldarg_1)]
        [InlineData(2, CilCode.Ldarg_2)]
        [InlineData(3, CilCode.Ldarg_3)]
        public void OptimizeFirst4ArgumentsToMacros(int index, CilCode expectedMacro)
        {
            var instructions = CreateDummyMethod(false, 4, 0);
            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Ldarg, instructions.Owner.Owner.Parameters[index]),
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(expectedMacro.ToOpCode(), instructions[0].OpCode);
            Assert.Null(instructions[0].Operand);
        }

        [Fact]
        public void OptimizeHiddenThisToLdarg0()
        {
            var instructions = CreateDummyMethod(true, 0, 0);
            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Ldarg, instructions.Owner.Owner.Parameters.ThisParameter), 
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(CilOpCodes.Ldarg_0, instructions[0].OpCode);
            Assert.Null(instructions[0].Operand);
        }

        [Fact]
        public void BranchWithSmallPositiveDeltaShouldOptimizeToShortBranch()
        {
            var instructions = CreateDummyMethod(false, 0, 0);
            var target = new CilInstruction(CilOpCodes.Nop);
            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Br, new CilInstructionLabel(target)), 
                new CilInstruction(CilOpCodes.Nop), 
                new CilInstruction(CilOpCodes.Nop), 
                target,
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(CilOpCodes.Br_S, instructions[0].OpCode);
        }

        [Fact]
        public void BranchWithSmallNegativeDeltaShouldOptimizeToShortBranch()
        {
            var instructions = CreateDummyMethod(false, 0, 0);
            var target = new CilInstruction(CilOpCodes.Nop);
            instructions.AddRange(new []
            {
                target,
                new CilInstruction(CilOpCodes.Nop), 
                new CilInstruction(CilOpCodes.Nop), 
                new CilInstruction(CilOpCodes.Br, new CilInstructionLabel(target)), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(CilOpCodes.Br_S, instructions[3].OpCode);
        }

        [Fact]
        public void BranchWithLargePositiveDeltaShouldNotOptimize()
        {
            var instructions = CreateDummyMethod(false, 0, 0);
            var target = new CilInstruction(CilOpCodes.Nop);
            
            instructions.Add(new CilInstruction(CilOpCodes.Br, new CilInstructionLabel(target)));

            for (int i = 0; i < 255; i++)
                instructions.Add(new CilInstruction(CilOpCodes.Nop));

            instructions.AddRange(new []
            { 
                target,
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(CilOpCodes.Br, instructions[0].OpCode);
        }

        [Fact]
        public void BranchWithLargeNegativeDeltaShouldNotOptimize()
        {
            var instructions = CreateDummyMethod(false, 0, 0);
            var target = new CilInstruction(CilOpCodes.Nop);
            
            instructions.Add(target);

            for (int i = 0; i < 255; i++)
                instructions.Add(new CilInstruction(CilOpCodes.Nop));
            
            instructions.Add(new CilInstruction(CilOpCodes.Br, new CilInstructionLabel(target)));
            
            instructions.OptimizeMacros();

            Assert.Equal(CilOpCodes.Br, instructions[instructions.Count - 1].OpCode);
        }

        [Fact]
        public void BranchWithInitialLargeDeltaButSmallDeltaAfterFirstPassShouldOptimize()
        {
            var instructions = CreateDummyMethod(false, 0, 0);
            var target = new CilInstruction(CilOpCodes.Nop);
            
            instructions.Add(new CilInstruction(CilOpCodes.Br, new CilInstructionLabel(target)));

            for (int i = 0; i < 60; i++)
            {
                instructions.Add(new CilInstruction(CilOpCodes.Ldc_I4, 1));
                instructions.Add(new CilInstruction(CilOpCodes.Pop));
            }

            instructions.AddRange(new []
            { 
                target,
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(CilOpCodes.Br_S, instructions[0].OpCode);
        }
    }
}