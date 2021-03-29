using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Cil
{
    public class CilMethodBodyTest
    {
        private CilMethodBody ReadMethodBody(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location);
            return GetMethodBodyInModule(module, name);
        }

        private static CilMethodBody GetMethodBodyInModule(ModuleDefinition module, string name)
        {
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MethodBodyTypes));
            return type.Methods.First(m => m.Name == name).CilMethodBody;
        }

        private CilMethodBody RebuildAndLookup(CilMethodBody methodBody)
        {
            var module = methodBody.Owner.Module;

            string tempFile = Path.GetTempFileName();
            module.Write(tempFile);

            var stream = new MemoryStream();
            module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            return GetMethodBodyInModule(newModule, methodBody.Owner.Name);
        }

        [Fact]
        public void ReadTinyMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.TinyMethod));
            Assert.False(body.IsFat);
        }

        [Fact]
        public void PersistentTinyMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.TinyMethod));
            var newBody = RebuildAndLookup(body);

            Assert.False(newBody.IsFat);
            Assert.Equal(body.Instructions.Count, newBody.Instructions.Count);
        }

        [Fact]
        public void ReadFatLongMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            Assert.True(body.IsFat);
        }

        [Fact]
        public void PersistentFatLongMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            var newBody = RebuildAndLookup(body);

            Assert.True(newBody.IsFat);
            Assert.Equal(body.Instructions.Count, newBody.Instructions.Count);
        }

        [Fact]
        public void ReadFatMethodWithLocals()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithLocals));
            Assert.True(body.IsFat);
            Assert.Contains(body.LocalVariables, x => x.VariableType.ElementType == ElementType.I4);
        }

        [Fact]
        public void ReadFatMethodWithManyLocals()
        {
            // https://github.com/Washi1337/AsmResolver/issues/55

            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithManyLocals));
            int expectedIndex = 0;
            foreach (var instruction in body.Instructions)
            {
                if (instruction.IsLdloc())
                {
                    var variable = instruction.GetLocalVariable(body.LocalVariables);
                    Assert.Equal(expectedIndex, variable.Index);
                    expectedIndex++;
                }
            }
        }

        [Fact]
        public void PersistentFatMethodWithLocals()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            var newBody = RebuildAndLookup(body);

            Assert.True(newBody.IsFat);
            Assert.Equal(
                body.LocalVariables.Select(v => v.VariableType.FullName),
                newBody.LocalVariables.Select(v => v.VariableType.FullName));
        }

        [Fact]
        public void ReadFatMethodWithExceptionHandler()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithExceptionHandler));
            Assert.True(body.IsFat);
            Assert.Single(body.ExceptionHandlers);
        }

        [Fact]
        public void ReadDynamicMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(TDynamicMethod).Assembly.Location);

            var type = module.TopLevelTypes.First(t => t.Name == nameof(TDynamicMethod));

            var method = type.Methods.FirstOrDefault(m => m.Name == nameof(TDynamicMethod.GenerateDynamicMethod));

            DynamicMethod generateDynamicMethod = TDynamicMethod.GenerateDynamicMethod();

            //Dynamic method => CilMethodBody
            var body = CilMethodBody.FromDynamicMethod(method, generateDynamicMethod);

            Assert.NotNull(body);

            Assert.NotEmpty(body.Instructions);

            Assert.Equal(body.Instructions.Select(q=>q.OpCode),new CilOpCode[]
            {
                CilOpCodes.Ldarg_0,
                CilOpCodes.Call,
                CilOpCodes.Ldarg_1,
                CilOpCodes.Ret
            });
        }

        private static CilMethodBody CreateDummyBody(bool isVoid)
        {
            var module = new ModuleDefinition("DummyModule");
            var method = new MethodDefinition("Main",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(isVoid ? module.CorLibTypeFactory.Void : module.CorLibTypeFactory.Int32));

            module.GetOrCreateModuleType().Methods.Add(method);
            return method.CilMethodBody = new CilMethodBody(method);
        }

        [Fact]
        public void MaxStackComputationOnNonVoidShouldFailIfNoValueOnStack()
        {
            var body = CreateDummyBody(false);
            body.Instructions.Add(CilOpCodes.Ret);

            Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
        }

        [Fact]
        public void MaxStackComputationOnNonVoid()
        {
            var body = CreateDummyBody(false);
            body.Instructions.Add(CilOpCodes.Ldnull);
            body.Instructions.Add(CilOpCodes.Ret);

            Assert.Equal(1, body.ComputeMaxStack());
        }

        [Fact]
        public void MaxStackComputationOnVoidShouldFailIfValueOnStack()
        {
            var body = CreateDummyBody(true);
            body.Instructions.Add(CilOpCodes.Ldnull);
            body.Instructions.Add(CilOpCodes.Ret);

            Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
        }

        [Fact]
        public void JoiningPathsWithSameStackSizeShouldSucceed()
        {
            var body = CreateDummyBody(false);
            var instructions = body.Instructions;

            var branchTarget1 = new CilInstructionLabel();
            var branchTarget2 = new CilInstructionLabel();

            instructions.Add(CilOpCodes.Ldarg_0);
            instructions.Add(CilOpCodes.Brtrue, branchTarget1);

            instructions.Add(CilOpCodes.Ldc_I4_1);
            instructions.Add(CilOpCodes.Br, branchTarget2);

            branchTarget1.Instruction = instructions.Add(CilOpCodes.Ldc_I4_0);
            branchTarget2.Instruction = instructions.Add(CilOpCodes.Nop);

            instructions.Add(CilOpCodes.Ret);

            Assert.Equal(1, body.ComputeMaxStack());
        }

        [Fact]
        public void JoiningPathsWithDifferentStackSizesShouldFail()
        {
            var body = CreateDummyBody(false);
            var instructions = body.Instructions;

            var branchTarget1 = new CilInstructionLabel();
            var branchTarget2 = new CilInstructionLabel();
            var end = new CilInstructionLabel();

            instructions.Add(CilOpCodes.Ldarg_0);
            instructions.Add(CilOpCodes.Brtrue, branchTarget1);

            instructions.Add(CilOpCodes.Ldc_I4_1);
            instructions.Add(CilOpCodes.Ldc_I4_2);
            instructions.Add(CilOpCodes.Br, branchTarget2);

            branchTarget1.Instruction = instructions.Add(CilOpCodes.Ldc_I4_0);
            branchTarget2.Instruction = instructions.Add(CilOpCodes.Nop);
            end.Instruction = instructions.Add(CilOpCodes.Ret);

            var exception = Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
            Assert.Equal(end.Offset, exception.Offset);
        }

        [Fact]
        public void ThrowsInstructionShouldTerminateTraversal()
        {
            var body = CreateDummyBody(false);
            var instructions = body.Instructions;

            instructions.Add(CilOpCodes.Ldnull);
            instructions.Add(CilOpCodes.Throw);

            // Junk opcodes..
            instructions.Add(CilOpCodes.Ldc_I4_0);

            Assert.Equal(1, body.ComputeMaxStack());
        }

        [Fact]
        public void ExceptionHandlerExpectsOneValueOnStack()
        {
            var body = CreateDummyBody(true);

            var end = new CilInstructionLabel();

            var tryStart = new CilInstructionLabel();
            var tryEnd = new CilInstructionLabel();
            var handlerStart = new CilInstructionLabel();
            var handlerEnd = new CilInstructionLabel();

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Exception,
                ExceptionType = body.Owner.Module.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
            });

            tryStart.Instruction = body.Instructions.Add(CilOpCodes.Nop);
            tryEnd.Instruction = body.Instructions.Add(CilOpCodes.Leave, end);
            handlerStart.Instruction = body.Instructions.Add(CilOpCodes.Pop);
            handlerEnd.Instruction = body.Instructions.Add(CilOpCodes.Leave, end);
            end.Instruction = body.Instructions.Add(CilOpCodes.Ret);

            Assert.Equal(1, body.ComputeMaxStack());
        }

        [Fact]
        public void FinallyHandlerExpectsNoValueOnStack()
        {
            var body = CreateDummyBody(true);

            var end = new CilInstructionLabel();

            var tryStart = new CilInstructionLabel();
            var tryEnd = new CilInstructionLabel();
            var handlerStart = new CilInstructionLabel();
            var handlerEnd = new CilInstructionLabel();

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Finally,
                ExceptionType = body.Owner.Module.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
            });

            tryStart.Instruction = body.Instructions.Add(CilOpCodes.Nop);
            tryEnd.Instruction = body.Instructions.Add(CilOpCodes.Leave, end);
            handlerStart.Instruction = body.Instructions.Add(CilOpCodes.Nop);
            handlerEnd.Instruction = body.Instructions.Add(CilOpCodes.Leave, end);
            end.Instruction = body.Instructions.Add(CilOpCodes.Ret);

            Assert.Equal(0, body.ComputeMaxStack());
        }

        [Fact]
        public void LeaveInstructionShouldClearStackAndNotFail()
        {
            var body = CreateDummyBody(true);

            var end = new CilInstructionLabel();

            var tryStart = new CilInstructionLabel();
            var tryEnd = new CilInstructionLabel();
            var handlerStart = new CilInstructionLabel();
            var handlerEnd = new CilInstructionLabel();

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Exception,
                ExceptionType = body.Owner.Module.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
            });

            tryStart.Instruction = body.Instructions.Add(CilOpCodes.Nop);
            tryEnd.Instruction = body.Instructions.Add(CilOpCodes.Leave, end);

            handlerStart.Instruction = body.Instructions.Add(CilOpCodes.Nop);
            // Push junk values on the stack.
            body.Instructions.Add(CilOpCodes.Ldc_I4_0);
            body.Instructions.Add(CilOpCodes.Ldc_I4_1);
            body.Instructions.Add(CilOpCodes.Ldc_I4_2);
            // Leave should clear.
            handlerEnd.Instruction = body.Instructions.Add(CilOpCodes.Leave, end);

            end.Instruction = body.Instructions.Add(CilOpCodes.Ret);

            Assert.Equal(4, body.ComputeMaxStack());
        }

        [Fact]
        public void LazyInitializationTest()
        {
            // https://github.com/Washi1337/AsmResolver/issues/97

            var module = ModuleDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(new MetadataToken(TableIndex.Method, 1));
            var body = method.CilMethodBody;
            method.DeclaringType.Methods.Remove(method);
            Assert.NotNull(body);

            var module2 = ModuleDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location);
            var method2 = (MethodDefinition) module2.LookupMember(new MetadataToken(TableIndex.Method, 1));
            method2.DeclaringType.Methods.Remove(method2);
            var body2 = method2.CilMethodBody;
            Assert.NotNull(body2);
        }

        [Fact]
        public void PathWithThrowDoesNotHaveToEndWithAnEmptyStack()
        {
            var body = CreateDummyBody(true);

            body.Instructions.Add(CilOpCodes.Ldc_I4_0);
            body.Instructions.Add(CilOpCodes.Ldnull);
            body.Instructions.Add(CilOpCodes.Throw);

            Assert.Equal(2, body.ComputeMaxStack());
        }

        [Fact]
        public void JmpShouldNotContinueAnalysisAfter()
        {
            var body = CreateDummyBody(true);

            body.Instructions.Add(CilOpCodes.Jmp, body.Owner);
            body.Instructions.Add(CilOpCodes.Ldnull);

            Assert.Equal(0, body.ComputeMaxStack());
        }

        [Fact]
        public void NonEmptyStackAtJmpShouldThrow()
        {
            var body = CreateDummyBody(true);

            body.Instructions.Add(CilOpCodes.Ldnull);
            body.Instructions.Add(CilOpCodes.Jmp, body.Owner);

            Assert.Throws<StackImbalanceException>(() => body.ComputeMaxStack());
        }

        [Fact]
        public void ReadInvalidMethodBodyErrorShouldAppearInDiagnostics()
        {
            var bag = new DiagnosticBag();

            // Read module with diagnostic bag as error listener.
            var module = ModuleDefinition.FromBytes(
                Properties.Resources.HelloWorld_InvalidMethodBody,
                new ModuleReaderParameters(bag));

            // Ensure invalid method body is loaded.
            foreach (var method in module.GetOrCreateModuleType().Methods)
                Assert.Null(method.CilMethodBody);

            // Verify error was reported.
            Assert.NotEmpty(bag.Exceptions);
        }

        [Fact]
        public void ExceptionHandlerWithHandlerEndOutsideOfMethodShouldResultInEndLabel()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HandlerEndAtEndOfMethodBody);
            var body = module.ManagedEntrypointMethod.CilMethodBody;
            Assert.Same(body.Instructions.EndLabel, body.ExceptionHandlers[0].HandlerEnd);
        }

        [Fact]
        public void NullBranchTargetShouldThrow()
        {
            var body = CreateDummyBody(true);
            body.Instructions.Add(new CilInstruction(CilOpCodes.Br, null));
            Assert.Throws<InvalidCilInstructionException>(() => body.VerifyLabels());
        }

        [Fact]
        public void NonExistingBranchTargetShouldThrow()
        {
            var body = CreateDummyBody(true);
            body.Instructions.Add(CilOpCodes.Br, new CilOffsetLabel(1337));
            Assert.Throws<InvalidCilInstructionException>(() => body.VerifyLabels());
        }

        [Fact]
        public void ExistingOffsetBranchTargetShouldNotThrow()
        {
            var body = CreateDummyBody(true);
            body.Instructions.Add(CilOpCodes.Br_S, new CilOffsetLabel(2));
            body.Instructions.Add(CilOpCodes.Ret);

            body.VerifyLabels();
        }

        [Fact]
        public void ExistingInstructionBranchTargetShouldNotThrow()
        {
            var body = CreateDummyBody(true);
            var label = new CilInstructionLabel();
            body.Instructions.Add(CilOpCodes.Br_S, label);
            label.Instruction = body.Instructions.Add(CilOpCodes.Ret);

            body.VerifyLabels();
        }

        [Fact]
        public void NullHandlerShouldThrow()
        {
            var body = CreateDummyBody(true);

            var handler = new CilExceptionHandler();
            body.ExceptionHandlers.Add(handler);
            Assert.Throws<AggregateException>(() => body.VerifyLabels());
        }

        [Fact]
        public void ValidHandlerShouldNotThrow()
        {
            var body = CreateDummyBody(true);

            var tryStart = new CilInstructionLabel();
            var tryEnd = new CilInstructionLabel();
            var handlerStart = new CilInstructionLabel();
            var handlerEnd = new CilInstructionLabel();

            var handler = new CilExceptionHandler
            {
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
                HandlerType = CilExceptionHandlerType.Exception
            };

            body.Instructions.Add(CilOpCodes.Nop);
            tryStart.Instruction = body.Instructions.Add(CilOpCodes.Leave, handlerEnd);
            handlerStart.Instruction = tryEnd.Instruction = body.Instructions.Add(CilOpCodes.Leave, handlerEnd);
            handlerEnd.Instruction = body.Instructions.Add(CilOpCodes.Ret);

            body.ExceptionHandlers.Add(handler);
            body.VerifyLabels();
        }

        [Fact]
        public void NullFilterOnFilterHandlerShouldThrow()
        {
            var body = CreateDummyBody(true);

            var tryStart = new CilInstructionLabel();
            var tryEnd = new CilInstructionLabel();
            var handlerStart = new CilInstructionLabel();
            var handlerEnd = new CilInstructionLabel();

            var handler = new CilExceptionHandler
            {
                TryStart = tryStart,
                TryEnd = tryEnd,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
                HandlerType = CilExceptionHandlerType.Filter
            };

            body.Instructions.Add(CilOpCodes.Nop);
            tryStart.Instruction = body.Instructions.Add(CilOpCodes.Leave, handlerEnd);
            tryEnd.Instruction = body.Instructions.Add(CilOpCodes.Endfilter);
            handlerStart.Instruction = body.Instructions.Add(CilOpCodes.Leave, handlerEnd);
            handlerEnd.Instruction = body.Instructions.Add(CilOpCodes.Ret);

            body.ExceptionHandlers.Add(handler);
            Assert.Throws<InvalidCilInstructionException>(() => body.VerifyLabels());
        }
    }
}
