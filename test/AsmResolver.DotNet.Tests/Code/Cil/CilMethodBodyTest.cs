using System.IO;
using System.Linq;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Cil;
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
            body.Instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Ret), 
            });

            Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
        }
        
        [Fact]
        public void MaxStackComputationOnNonVoid()
        {
            var body = CreateDummyBody(false);
            body.Instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Ldnull),
                new CilInstruction(CilOpCodes.Ret), 
            });

            Assert.Equal(1, body.ComputeMaxStack());
        }
        
        [Fact]
        public void MaxStackComputationOnVoidShouldFailIfValueOnStack()
        {
            var body = CreateDummyBody(true);
            body.Instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Ldnull),
                new CilInstruction(CilOpCodes.Ret), 
            });

            Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
        }
        
        [Fact]
        public void JoiningPathsWithSameStackSizeShouldSucceed()
        {
            var body = CreateDummyBody(false);

            var branchTarget1 = new CilInstruction(CilOpCodes.Ldc_I4_0);
            var branchTarget2 = new CilInstruction(CilOpCodes.Nop);
            
            body.Instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Ldarg_0),
                new CilInstruction(CilOpCodes.Brtrue, new CilInstructionLabel(branchTarget1)),

                new CilInstruction(CilOpCodes.Ldc_I4_1),
                new CilInstruction(CilOpCodes.Br, new CilInstructionLabel(branchTarget2)),
                
                branchTarget1,
                branchTarget2,
                
                new CilInstruction(CilOpCodes.Ret), 
            });

            Assert.Equal(1, body.ComputeMaxStack());
        }
        
        [Fact]
        public void JoiningPathsWithDifferentStackSizesShouldFail()
        {
            var body = CreateDummyBody(false);

            var branchTarget1 = new CilInstruction(CilOpCodes.Ldc_I4_0);
            var branchTarget2 = new CilInstruction(CilOpCodes.Nop);
            var end = new CilInstruction(CilOpCodes.Ret);
            
            body.Instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Ldarg_0),
                new CilInstruction(CilOpCodes.Brtrue, new CilInstructionLabel(branchTarget1)),

                new CilInstruction(CilOpCodes.Ldc_I4_1),
                new CilInstruction(CilOpCodes.Ldc_I4_2),
                new CilInstruction(CilOpCodes.Br, new CilInstructionLabel(branchTarget2)),

                branchTarget1,
                
                branchTarget2,

                end, 
            });

            var exception = Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
            Assert.Equal(end.Offset, exception.Offset);   
        }
        
        [Fact]
        public void ThrowsInstructionShouldTerminateTraversal()
        {
            var body = CreateDummyBody(false);

            body.Instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Ldnull),
                new CilInstruction(CilOpCodes.Throw),

                // Junk opcodes..
                new CilInstruction(CilOpCodes.Ldc_I4_0),
            });

            Assert.Equal(1, body.ComputeMaxStack());
        }

        [Fact]
        public void ThrowsInstructionShouldFailIfTooManyValuesOnStack()
        {
            var body = CreateDummyBody(false);

            var throwInstruction = new CilInstruction(CilOpCodes.Throw);
            body.Instructions.AddRange(new[]
            {
                // Extra junk value
                new CilInstruction(CilOpCodes.Ldnull),

                new CilInstruction(CilOpCodes.Ldnull),
                throwInstruction,
            });

            var exception = Assert.ThrowsAny<StackImbalanceException>(() => body.ComputeMaxStack());
            Assert.Equal(throwInstruction.Offset, exception.Offset);   
        }

        [Fact]
        public void ExceptionHandlerExpectsOneValueOnStack()
        {
            var body = CreateDummyBody(true);

            var end = new CilInstruction(CilOpCodes.Ret);
            var endLabel = new CilInstructionLabel(end);
            
            var tryStart = new CilInstruction(CilOpCodes.Nop);
            var tryEnd = new CilInstruction(CilOpCodes.Leave, endLabel);
            var handlerStart = new CilInstruction(CilOpCodes.Pop);
            var handlerEnd = new CilInstruction(CilOpCodes.Leave, endLabel);

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Exception,
                ExceptionType = body.Owner.Module.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = new CilInstructionLabel(tryStart),
                TryEnd = new CilInstructionLabel(tryEnd),
                HandlerStart = new CilInstructionLabel(handlerStart),
                HandlerEnd = new CilInstructionLabel(handlerEnd),
            });
            
            body.Instructions.AddRange(new[]
            {
                tryStart,
                tryEnd,

                handlerStart,
                handlerEnd,
                end,
            });

            Assert.Equal(1, body.ComputeMaxStack());
        }

        [Fact]
        public void FinallyHandlerExpectsNoValueOnStack()
        {
            var body = CreateDummyBody(true);

            var end = new CilInstruction(CilOpCodes.Ret);
            var endLabel = new CilInstructionLabel(end);
            
            var tryStart = new CilInstruction(CilOpCodes.Nop);
            var tryEnd = new CilInstruction(CilOpCodes.Leave, endLabel);
            var handlerStart = new CilInstruction(CilOpCodes.Nop);
            var handlerEnd = new CilInstruction(CilOpCodes.Leave, endLabel);

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Finally,
                ExceptionType = body.Owner.Module.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = new CilInstructionLabel(tryStart),
                TryEnd = new CilInstructionLabel(tryEnd),
                HandlerStart = new CilInstructionLabel(handlerStart),
                HandlerEnd = new CilInstructionLabel(handlerEnd),
            });
            
            body.Instructions.AddRange(new[]
            {
                tryStart,
                tryEnd,

                handlerStart,
                handlerEnd,
                end,
            });

            Assert.Equal(0, body.ComputeMaxStack());
        }

        [Fact]
        public void LeaveInstructionShouldClearStackAndNotFail()
        {
            var body = CreateDummyBody(true);

            var end = new CilInstruction(CilOpCodes.Ret);
            var endLabel = new CilInstructionLabel(end);
            
            var tryStart = new CilInstruction(CilOpCodes.Nop);
            var tryEnd = new CilInstruction(CilOpCodes.Leave, endLabel);
            var handlerStart = new CilInstruction(CilOpCodes.Nop);
            var handlerEnd = new CilInstruction(CilOpCodes.Leave, endLabel);

            body.ExceptionHandlers.Add(new CilExceptionHandler
            {
                HandlerType = CilExceptionHandlerType.Exception,
                ExceptionType = body.Owner.Module.CorLibTypeFactory.Object.ToTypeDefOrRef(),
                TryStart = new CilInstructionLabel(tryStart),
                TryEnd = new CilInstructionLabel(tryEnd),
                HandlerStart = new CilInstructionLabel(handlerStart),
                HandlerEnd = new CilInstructionLabel(handlerEnd),
            });
            
            body.Instructions.AddRange(new[]
            {
                tryStart,
                tryEnd,

                handlerStart,
                
                // Push junk values on the stack.
                new CilInstruction(CilOpCodes.Ldc_I4_0),
                new CilInstruction(CilOpCodes.Ldc_I4_1),
                new CilInstruction(CilOpCodes.Ldc_I4_2), 
                
                // Leave should clear.
                handlerEnd,
                end,
            });

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
    }
}