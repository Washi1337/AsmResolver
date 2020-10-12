using System;
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

        [Theory]
        [InlineData(0, CilCode.Ldloc_0)]
        [InlineData(1, CilCode.Ldloc_1)]
        [InlineData(2, CilCode.Ldloc_2)]
        [InlineData(3, CilCode.Ldloc_3)]
        [InlineData(4, CilCode.Ldloc_S)]
        [InlineData(255, CilCode.Ldloc_S)]
        [InlineData(256, CilCode.Ldloc)]
        public void OptimizeLoadLocalInstructions(int index, CilCode expected)
        {
            var instructions = CreateDummyMethod(false, 0, index + 1);
            
            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Ldloc, instructions.Owner.LocalVariables[index]),
                new CilInstruction(CilOpCodes.Ret), 
            });

            instructions.OptimizeMacros();
            
            Assert.Equal(expected, instructions[0].OpCode.Code);
        }
        
        [Theory]
        [InlineData(0, CilCode.Stloc_0)]
        [InlineData(1, CilCode.Stloc_1)]
        [InlineData(2, CilCode.Stloc_2)]
        [InlineData(3, CilCode.Stloc_3)]
        [InlineData(4, CilCode.Stloc_S)]
        [InlineData(255, CilCode.Stloc_S)]
        [InlineData(256, CilCode.Stloc)]
        public void OptimizeStoreLocalInstructions(int index, CilCode expected)
        {
            var instructions = CreateDummyMethod(false, 0, index + 1);
            
            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Ldnull), 
                new CilInstruction(CilOpCodes.Stloc, instructions.Owner.LocalVariables[index]),
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(expected, instructions[1].OpCode.Code);
        }

        [Theory]
        [InlineData(0, CilCode.Ldarg_0)]
        [InlineData(1, CilCode.Ldarg_1)]
        [InlineData(2, CilCode.Ldarg_2)]
        [InlineData(3, CilCode.Ldarg_3)]
        [InlineData(4, CilCode.Ldarg_S)]
        [InlineData(255, CilCode.Ldarg_S)]
        [InlineData(256, CilCode.Ldarg)]
        public void OptimizeLoadArgInstructions(int index, CilCode expected)
        {
            var instructions = CreateDummyMethod(false, index + 1, 0);
            var method = instructions.Owner.Owner;
            
            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Ldarg, method.Parameters[index]),
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(expected, instructions[0].OpCode.Code);
        }

        [Theory]
        [InlineData(0, CilCode.Starg_S)]
        [InlineData(255, CilCode.Starg_S)]
        [InlineData(256, CilCode.Starg)]
        public void OptimizeStoreArgInstructions(int index, CilCode expected)
        {
            var instructions = CreateDummyMethod(false, index + 1, 0);
            var method = instructions.Owner.Owner;
            
            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Ldnull),
                new CilInstruction(CilOpCodes.Starg, method.Parameters[index]),
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(expected, instructions[1].OpCode.Code);
        }

        [Theory]
        [InlineData(0, CilCode.Ldc_I4_0)]
        [InlineData(1, CilCode.Ldc_I4_1)]
        [InlineData(2, CilCode.Ldc_I4_2)]
        [InlineData(3, CilCode.Ldc_I4_3)]
        [InlineData(4, CilCode.Ldc_I4_4)]
        [InlineData(5, CilCode.Ldc_I4_5)]
        [InlineData(6, CilCode.Ldc_I4_6)]
        [InlineData(7, CilCode.Ldc_I4_7)]
        [InlineData(8, CilCode.Ldc_I4_8)]
        [InlineData(-1, CilCode.Ldc_I4_M1)]
        [InlineData(sbyte.MaxValue, CilCode.Ldc_I4_S)]
        [InlineData(sbyte.MinValue, CilCode.Ldc_I4_S)]
        [InlineData(sbyte.MaxValue + 1, CilCode.Ldc_I4)]
        [InlineData(sbyte.MinValue - 1, CilCode.Ldc_I4)]
        public void OptimizeLdcI4(int operand, CilCode expected)
        {     
            var instructions = CreateDummyMethod(false, 0, 0);

            instructions.AddRange(new []
            {
                new CilInstruction(CilOpCodes.Ldc_I4, operand),
                new CilInstruction(CilOpCodes.Ret), 
            });
            
            instructions.OptimizeMacros();
            
            Assert.Equal(expected, instructions[0].OpCode.Code);
        }

        [Fact]
        public void RemoveIndices()
        {
            var instructions = CreateDummyMethod(false, 0, 0);
            instructions.AddRange(new[]
            {
                new CilInstruction(0, CilOpCodes.Nop),
                new CilInstruction(1, CilOpCodes.Nop),
                new CilInstruction(2, CilOpCodes.Nop),
                new CilInstruction(3, CilOpCodes.Nop),
                new CilInstruction(4, CilOpCodes.Nop),
                new CilInstruction(5, CilOpCodes.Nop),
                new CilInstruction(6, CilOpCodes.Ret),
            });

            var expected = new[]
            {
                instructions[0],
                instructions[2],
                instructions[4],
                instructions[5],
                instructions[6],
            };
            
            instructions.RemoveAt(2, -1, 1);
            Assert.Equal(expected, instructions);
        }

        [Fact]
        public void RemoveIndicesWithInvalidRelativeIndicesShouldThrowAndNotChangeAnything()
        {
            var instructions = CreateDummyMethod(false, 0, 0);
            instructions.AddRange(new[]
            {
                new CilInstruction(0, CilOpCodes.Nop),
                new CilInstruction(1, CilOpCodes.Nop),
                new CilInstruction(2, CilOpCodes.Nop),
                new CilInstruction(3, CilOpCodes.Nop),
                new CilInstruction(4, CilOpCodes.Nop),
                new CilInstruction(5, CilOpCodes.Nop),
                new CilInstruction(6, CilOpCodes.Ret),
            });

            var expected = instructions.ToArray();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                instructions.RemoveAt(2, -1, 100));
            Assert.Equal(expected, instructions);
        }
        
    }
}