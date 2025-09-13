using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Cil
{
    public class CilMethodBodyTest
    {
        private CilMethodBody ReadMethodBody(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location, TestReaderParameters);
            return GetMethodBodyInModule(module, name);
        }

        private static CilMethodBody GetMethodBodyInModule(ModuleDefinition module, string name)
        {
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MethodBodyTypes));
            return type.Methods.First(m => m.Name == name).CilMethodBody;
        }

        private CilMethodBody RebuildAndLookup(CilMethodBody methodBody, bool accessBeforeBuild)
        {
            if (accessBeforeBuild)
                _ = methodBody.Instructions;

            var module = methodBody.Owner!.DeclaringModule!;

            var stream = new MemoryStream();
            module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);
            return GetMethodBodyInModule(newModule, methodBody.Owner.Name);
        }

        [Fact]
        public void ReadTinyMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.TinyMethod));
            Assert.False(body.IsFat);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PersistentTinyMethod(bool accessBeforeBuild)
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.TinyMethod));
            var newBody = RebuildAndLookup(body, accessBeforeBuild);

            Assert.False(newBody.IsFat);
            Assert.Equal(body.Instructions.Count, newBody.Instructions.Count);
        }

        [Fact]
        public void ReadFatLongMethod()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            Assert.True(body.IsFat);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PersistentFatLongMethod(bool accessBeforeBuild)
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            var newBody = RebuildAndLookup(body, accessBeforeBuild);

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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PersistentFatMethodWithLocals(bool accessBeforeBuild)
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatLongMethod));
            var newBody = RebuildAndLookup(body, accessBeforeBuild);

            Assert.True(newBody.IsFat);
            Assert.Equal(
                body.LocalVariables.Select(v => v.VariableType.FullName),
                newBody.LocalVariables.Select(v => v.VariableType.FullName));
        }

        [Fact]
        public void ReadFatMethodWithFinally()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithFinally));
            Assert.True(body.IsFat);
            Assert.Equal(CilExceptionHandlerType.Finally, Assert.Single(body.ExceptionHandlers).HandlerType);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PersistentFatMethodWithFinally(bool accessBeforeBuild)
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithFinally));
            var newBody = RebuildAndLookup(body, accessBeforeBuild);

            Assert.True(newBody.IsFat);
            Assert.Equal(CilExceptionHandlerType.Finally, Assert.Single(newBody.ExceptionHandlers).HandlerType);
        }

        [Fact]
        public void ReadFatMethodWithCatch()
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithCatch));
            Assert.True(body.IsFat);
            Assert.Equal(2, body.ExceptionHandlers.Count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PersistentFatMethodWithMultipleCatchBlocks(bool accessBeforeBuild)
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.FatMethodWithCatch));
            var newBody = RebuildAndLookup(body, accessBeforeBuild);

            Assert.True(newBody.IsFat);
            Assert.Equal(2, newBody.ExceptionHandlers.Count);
            Assert.Equal(newBody.ExceptionHandlers[0].ExceptionType!.FullName, newBody.ExceptionHandlers[0].ExceptionType!.FullName);
            Assert.Equal(newBody.ExceptionHandlers[1].ExceptionType!.FullName, newBody.ExceptionHandlers[1].ExceptionType!.FullName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddLocalToExistingTinyBodyShouldPromoteToFat(bool accessBeforeBuild)
        {
            var body = ReadMethodBody(nameof(MethodBodyTypes.TinyMethod));

            Assert.False(body.IsFat);
            body.LocalVariables.Add(new CilLocalVariable(body.Owner!.DeclaringModule!.CorLibTypeFactory.Int32));
            Assert.True(body.IsFat);

            var newBody = RebuildAndLookup(body, accessBeforeBuild);

            Assert.True(body.IsFat);
            Assert.Equal(newBody.Owner!.DeclaringModule!.CorLibTypeFactory.Int32, Assert.Single(newBody.LocalVariables).VariableType);
        }

        private static CilMethodBody CreateDummyBody(bool isVoid)
        {
            var module = new ModuleDefinition("DummyModule");
            var method = new MethodDefinition("Main",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(isVoid ? module.CorLibTypeFactory.Void : module.CorLibTypeFactory.Int32));

            module.GetOrCreateModuleType().Methods.Add(method);
            return method.CilMethodBody = new CilMethodBody();
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
                ExceptionType = body.Owner.DeclaringModule.CorLibTypeFactory.Object.ToTypeDefOrRef(),
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
                ExceptionType = body.Owner.DeclaringModule.CorLibTypeFactory.Object.ToTypeDefOrRef(),
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
                ExceptionType = body.Owner.DeclaringModule.CorLibTypeFactory.Object.ToTypeDefOrRef(),
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
        public void UnreachableBlockShouldNotBeScheduled()
        {
            var body = CreateDummyBody(true);
            var il = body.Instructions;

            // Exit early.
            il.Add(CilOpCodes.Ret);

            // Create unreachable deliberate stack underflow.
            il.Add(CilOpCodes.Pop);
            il.Add(CilOpCodes.Ret);

            // Max stack computation should succeed.
            Assert.Equal(0, body.ComputeMaxStack());
        }

        [Fact]
        public void UnreachableExceptionHandlerShouldNotBeScheduled()
        {
            var start = new CilInstructionLabel();
            var handler = new CilInstructionLabel();
            var end = new CilInstructionLabel();

            var body = CreateDummyBody(true);
            var il = body.Instructions;

            // Exit early.
            il.Add(CilOpCodes.Ret);

            // Create unreachable try-catch with deliberate stack underflow in try block.
            start.Instruction = il.Add(CilOpCodes.Pop);
            il.Add(CilOpCodes.Leave, end);
            handler.Instruction = il.Add(CilOpCodes.Leave, end);
            end.Instruction = il.Add(CilOpCodes.Ret);

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Exception,
                ExceptionType = body.Owner!.DeclaringModule!.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = start,
                TryEnd = handler,
                HandlerStart = handler,
                HandlerEnd = end
            });

            // Max stack computation should succeed.
            Assert.Equal(0, body.ComputeMaxStack());
        }

        [Fact]
        public void EnterTryBlockWithNonEmptyStackShouldThrow()
        {
            // https://github.com/Washi1337/AsmResolver/issues/652

            var start = new CilInstructionLabel();
            var handler = new CilInstructionLabel();
            var end = new CilInstructionLabel();

            var body = CreateDummyBody(true);
            var il = body.Instructions;

            // Push value.
            il.Add(CilOpCodes.Ldc_I4, 1337);

            // Create try block that consumes value
            start.Instruction = il.Add(CilOpCodes.Pop);
            il.Add(CilOpCodes.Leave, end);
            handler.Instruction = il.Add(CilOpCodes.Leave, end);
            end.Instruction = il.Add(CilOpCodes.Ret);

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Exception,
                ExceptionType = body.Owner!.DeclaringModule!.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = start,
                TryEnd = handler,
                HandlerStart = handler,
                HandlerEnd = end
            });

            // Max stack computation should succeed.
            Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
        }

        [Fact]
        public void LazyInitializationTest()
        {
            // https://github.com/Washi1337/AsmResolver/issues/97

            var module = ModuleDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location, TestReaderParameters);
            var method = (MethodDefinition) module.LookupMember(new MetadataToken(TableIndex.Method, 1));
            var body = method.CilMethodBody;
            method.DeclaringType.Methods.Remove(method);
            Assert.NotNull(body);

            var module2 = ModuleDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location, TestReaderParameters);
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
            var module = ModuleDefinition.FromBytes(Properties.Resources.HandlerEndAtEndOfMethodBody, TestReaderParameters);
            var body = module.ManagedEntryPointMethod.CilMethodBody;
            Assert.Same(body.Instructions.EndLabel, body.ExceptionHandlers[0].HandlerEnd);
            body.VerifyLabels();
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

        [Fact]
        public void SmallTryAndHandlerBlockShouldResultInTinyFormat()
        {
            var body = CreateDummyBody(true);
            for (int i = 0; i < 10; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Ret);
            body.Instructions.CalculateOffsets();

            var handler = new CilExceptionHandler
            {
                TryStart = body.Instructions[0].CreateLabel(),
                TryEnd = body.Instructions[1].CreateLabel(),
                HandlerStart = body.Instructions[1].CreateLabel(),
                HandlerEnd = body.Instructions[2].CreateLabel(),
                HandlerType = CilExceptionHandlerType.Finally
            };
            body.ExceptionHandlers.Add(handler);

            Assert.False(handler.IsFat);
        }

        [Fact]
        public void LargeTryBlockShouldResultInFatFormat()
        {
            var body = CreateDummyBody(true);
            for (int i = 0; i < 300; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Ret);
            body.Instructions.CalculateOffsets();

            var handler = new CilExceptionHandler
            {
                TryStart = body.Instructions[0].CreateLabel(),
                TryEnd = body.Instructions[256].CreateLabel(),
                HandlerStart = body.Instructions[256].CreateLabel(),
                HandlerEnd = body.Instructions[257].CreateLabel(),
                HandlerType = CilExceptionHandlerType.Finally
            };
            body.ExceptionHandlers.Add(handler);

            Assert.True(handler.IsFat);
        }

        [Fact]
        public void LargeHandlerBlockShouldResultInFatFormat()
        {
            var body = CreateDummyBody(true);
            for (int i = 0; i < 300; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Ret);
            body.Instructions.CalculateOffsets();

            var handler = new CilExceptionHandler
            {
                TryStart = body.Instructions[0].CreateLabel(),
                TryEnd = body.Instructions[1].CreateLabel(),
                HandlerStart = body.Instructions[1].CreateLabel(),
                HandlerEnd = body.Instructions[257].CreateLabel(),
                HandlerType = CilExceptionHandlerType.Finally
            };
            body.ExceptionHandlers.Add(handler);

            Assert.True(handler.IsFat);
        }

        [Fact]
        public void SmallTryBlockStartingOnLargeOffsetShouldResultInFatFormat()
        {
            var body = CreateDummyBody(true);
            for (int i = 0; i < 0x20000; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Ret);
            body.Instructions.CalculateOffsets();

            var handler = new CilExceptionHandler
            {
                TryStart = body.Instructions[0x10000].CreateLabel(),
                TryEnd = body.Instructions[0x10001].CreateLabel(),
                HandlerStart = body.Instructions[0x10001].CreateLabel(),
                HandlerEnd = body.Instructions[0x10002].CreateLabel(),
                HandlerType = CilExceptionHandlerType.Finally
            };
            body.ExceptionHandlers.Add(handler);

            Assert.True(handler.IsFat);
        }

        [Fact]
        public void ReadUserStringFromNormalMetadata()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_DoubleUserStringsStream, TestReaderParameters);
            var instruction = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions
                .First(i => i.OpCode.Code == CilCode.Ldstr);

            Assert.Equal("Hello Mars!!", instruction.Operand);
        }

        [Fact]
        public void ReadUserStringFromEnCMetadata()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_DoubleUserStringsStream_EnC, TestReaderParameters);
            var instruction = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions
                .First(i => i.OpCode.Code == CilCode.Ldstr);

            Assert.Equal("Hello World!", instruction.Operand);
        }

        private CilMethodBody CreateAndReadPatchedBody(IErrorListener listener, Action<CilRawFatMethodBody> patch)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            // Add new dummy method to type.
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));
            module.GetOrCreateModuleType().Methods.Add(method);

            // Give it a method body.
            var body = new CilMethodBody();
            method.MethodBody = body;

            // Add some random local variables.
            for (int i = 0; i < 10; i++)
                body.LocalVariables.Add(new CilLocalVariable(module.CorLibTypeFactory.Object));

            // Add some random instructions.
            for (int i = 0; i < 100; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Ret);

            // Construct PE image.
            var result = new ManagedPEImageBuilder().CreateImage(module);

            // Look up raw method body.
            var token = result.TokenMapping[method];
            var metadata = result.ConstructedImage!.DotNetDirectory!.Metadata!;
            var rawBody = (CilRawFatMethodBody) metadata
                .GetStream<TablesStream>()
                .GetTable<MethodDefinitionRow>()
                .GetByRid(token.Rid)
                .Body.GetSegment();

            Assert.NotNull(rawBody);

            // Patch it.
            patch(rawBody);

            // Read back module definition and look up interpreted method body.
            module = ModuleDefinition.FromImage(result.ConstructedImage, new ModuleReaderParameters(listener));
            return ((MethodDefinition) module.LookupMember(token)).CilMethodBody;
        }

        [Fact]
        public void ReadLocalsFromBodyWithInvalidCodeStream()
        {
            var body = CreateAndReadPatchedBody(EmptyErrorListener.Instance, raw =>
            {
                raw.Code = new DataSegment(new byte[]
                {
                    0xFE // 2-byte prefix opcode
                });
            });

            Assert.NotEmpty(body.LocalVariables);
        }

        [Fact]
        public void ReadCodeStreamFromBodyWithInvalidLocalVariablesSignature()
        {
            var body = CreateAndReadPatchedBody(EmptyErrorListener.Instance, raw =>
            {
                raw.LocalVarSigToken = new MetadataToken(TableIndex.StandAloneSig, 0x123456);
            });

            Assert.NotEmpty(body.Instructions);
        }

        [Fact]
        public void ReadInvalidBody()
        {
            var body = CreateAndReadPatchedBody(EmptyErrorListener.Instance, raw =>
            {
                raw.Code = new DataSegment(new byte[] { 0xFE });
                raw.LocalVarSigToken = new MetadataToken(TableIndex.StandAloneSig, 0x123456);
            });

            Assert.NotNull(body);
        }
    }
}
