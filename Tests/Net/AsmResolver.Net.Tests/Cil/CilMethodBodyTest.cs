using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Signatures;
using Xunit;
using MethodAttributes = AsmResolver.Net.Metadata.MethodAttributes;

namespace AsmResolver.Tests.Net.Cil
{
    public class CilMethodBodyTest
    {
        private const string DummyAssemblyName = "SomeAssemblyName";
        private readonly SignatureComparer _comparer = new SignatureComparer();

        private static CilMethodBody CreateDummyMethodBody()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();

            var type = new TypeDefinition("SomeType", "SomeMethod");
            image.Assembly.Modules[0].TopLevelTypes.Add(type);

            var method = new MethodDefinition("SomeMethod",
                MethodAttributes.Public,
                new MethodSignature(image.TypeSystem.Void));
            type.Methods.Add(method);

            var methodBody = new CilMethodBody(method);
            method.MethodBody = methodBody;
            return methodBody;
        }

        [Fact]
        public void ComputeMaxStackTestCall()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

            var instructions = methodBody.Instructions;
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldstr, "Lorem Ipsum"),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ret)
            });

            Assert.Equal(1, methodBody.ComputeMaxStack());
        }

        [Fact]
        public void ComputeMaxStackTestSimple()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                CilInstruction.Create(CilOpCodes.Add),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ret)
            });

            Assert.Equal(2, methodBody.ComputeMaxStack());
        }

        [Fact]
        public void ComputeMaxStackBranch()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            var target = CilInstruction.Create(CilOpCodes.Call, writeLine);
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Br, target),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                target,
                CilInstruction.Create(CilOpCodes.Ret),
            });
            Assert.Equal(1, methodBody.ComputeMaxStack());
        }

        [Fact]
        public void ComputeMaxStackConditionalBranch()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            var elseInstr = CilInstruction.Create(CilOpCodes.Sub);
            var endInstr = CilInstruction.Create(CilOpCodes.Call, writeLine);

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                CilInstruction.Create(CilOpCodes.Ldc_I4_0),
                CilInstruction.Create(CilOpCodes.Brtrue, elseInstr),
                CilInstruction.Create(CilOpCodes.Add),
                CilInstruction.Create(CilOpCodes.Br, endInstr),
                elseInstr,
                endInstr,
                CilInstruction.Create(CilOpCodes.Ret)
            });

            Assert.Equal(3, methodBody.ComputeMaxStack());
        }

        [Fact]
        public void ComputeMaxStackLoop()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            var loopHead = CilInstruction.Create(CilOpCodes.Nop);

            instructions.AddRange(new[]
            {
                loopHead,
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                CilInstruction.Create(CilOpCodes.Add),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ldc_I4_0),
                CilInstruction.Create(CilOpCodes.Brtrue, loopHead),
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            Assert.Equal(2, methodBody.ComputeMaxStack());
        }

        [Fact]
        public void ComputeMaxStackExceptionHandlers()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;

            var tryStart = CilInstruction.Create(CilOpCodes.Nop);
            var handlerStart = CilInstruction.Create(CilOpCodes.Pop);
            var handlerEnd = CilInstruction.Create(CilOpCodes.Nop);

            instructions.AddRange(new[]
            {
                tryStart,
                CilInstruction.Create(CilOpCodes.Ldc_I4_0),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Leave, handlerEnd),
                handlerStart,
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                CilInstruction.Create(CilOpCodes.Add),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Leave, handlerEnd),
                handlerEnd,
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            methodBody.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Exception)
            {
                TryStart = tryStart,
                TryEnd = handlerStart,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
                CatchType = importer.ImportType(typeof(Exception))
            });

            Assert.Equal(2, methodBody.ComputeMaxStack());
        }

        [Fact]
        public void StackInbalanceFallthrough()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            var target = CilInstruction.Create(CilOpCodes.Call, writeLine);

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Brtrue, target),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                target,
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            Assert.Throws<StackInbalanceException>(() => methodBody.ComputeMaxStack());
        }

        [Fact]
        public void StackInbalanceLoop()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            var loopHead = CilInstruction.Create(CilOpCodes.Nop);

            instructions.AddRange(new[]
            {
                loopHead,
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                CilInstruction.Create(CilOpCodes.Add),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ldc_I4_0),
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Brtrue, loopHead),
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            Assert.Throws<StackInbalanceException>(() => methodBody.ComputeMaxStack());
        }

        [Fact]
        public void StackInsufficientValues()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_I4, -1),
                CilInstruction.Create(CilOpCodes.And),
                CilInstruction.Create(CilOpCodes.Ldc_I4, -1),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ret), 
            });

            Assert.Throws<StackInbalanceException>(() => methodBody.ComputeMaxStack());
        }

        [Fact]
        public void PersistentInstructions()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;
            var importer = new ReferenceImporter(image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

            var instructions = methodBody.Instructions;
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldstr, "Lorem Ipsum"),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition) image.ResolveMember(mapping[methodBody.Method]);
            Assert.Equal(instructions, newMethod.CilMethodBody.Instructions, new CilInstructionComparer());
        }

        [Fact]
        public void PersistentExceptionHandlers()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;
            var importer = new ReferenceImporter(image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;

            var tryStart = CilInstruction.Create(CilOpCodes.Nop);
            var handlerStart = CilInstruction.Create(CilOpCodes.Pop);
            var handlerEnd = CilInstruction.Create(CilOpCodes.Nop);

            instructions.AddRange(new[]
            {
                tryStart,
                CilInstruction.Create(CilOpCodes.Ldc_I4_0),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Leave, handlerEnd),
                handlerStart,
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Ldc_I4_2),
                CilInstruction.Create(CilOpCodes.Add),
                CilInstruction.Create(CilOpCodes.Call, writeLine),
                CilInstruction.Create(CilOpCodes.Leave, handlerEnd),
                handlerEnd,
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            var exceptionType = importer.ImportType(typeof(Exception));
            methodBody.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Exception)
            {
                TryStart = tryStart,
                TryEnd = handlerStart,
                HandlerStart = handlerStart,
                HandlerEnd = handlerEnd,
                CatchType = exceptionType
            });

            var mapping = image.Header.UnlockMetadata();
            
            image = image.Header.LockMetadata();
            
            var instructionComparer = new CilInstructionComparer();

            var newMethod = (MethodDefinition) image.ResolveMember(mapping[methodBody.Method]);
            Assert.Equal(1, newMethod.CilMethodBody.ExceptionHandlers.Count);
            var handler = newMethod.CilMethodBody.ExceptionHandlers[0];
             Assert.Equal(tryStart, handler.TryStart, instructionComparer);
            Assert.Equal(handlerStart, handler.TryEnd, instructionComparer);
            Assert.Equal(handlerStart, handler.HandlerStart, instructionComparer);
            Assert.Equal(handlerEnd, handler.HandlerEnd, instructionComparer);
            Assert.Equal(exceptionType, handler.CatchType, _comparer);
        }

        [Fact]
        public void PersistentVariables()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;
            var importer = new ReferenceImporter(image);

            var var1 = new VariableSignature(image.TypeSystem.Int32);
            var var2 = new VariableSignature(importer.ImportTypeSignature(typeof(Stream)));
            methodBody.Signature = new StandAloneSignature(new LocalVariableSignature(new[] { var1, var2 }));

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            methodBody = ((MethodDefinition) image.ResolveMember(mapping[methodBody.Method])).CilMethodBody;
            Assert.NotNull(methodBody.Signature);
            Assert.IsType<LocalVariableSignature>(methodBody.Signature.Signature);

            var localVarSig = (LocalVariableSignature) methodBody.Signature.Signature;
            Assert.Equal(2, localVarSig.Variables.Count);
            Assert.Equal(var1.VariableType, localVarSig.Variables[0].VariableType, _comparer);
            Assert.Equal(var2.VariableType, localVarSig.Variables[1].VariableType, _comparer);
        }

        [Fact]
        public void PersistentExtraData()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var extraData = new byte[] {1, 2, 3, 4};
            methodBody.Signature = new StandAloneSignature(new LocalVariableSignature(new[] {image.TypeSystem.Boolean})
            {
                ExtraData = extraData
            });

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            methodBody = ((MethodDefinition) image.ResolveMember(mapping[methodBody.Method])).CilMethodBody;
            Assert.NotNull(methodBody.Signature);
            Assert.IsType<LocalVariableSignature>(methodBody.Signature.Signature);

            var localVarSig = (LocalVariableSignature) methodBody.Signature.Signature;
            Assert.Equal(extraData, localVarSig.ExtraData);
        }

        [Fact]
        public void OperandTypeInts()
        {
            const sbyte shortOperand = 0x12;
            const int operand = 0x1234;
            const long longOperand = 0x12345678;

            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var instructions = methodBody.Instructions;
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_I4_S, shortOperand),
                CilInstruction.Create(CilOpCodes.Ldc_I4, operand),
                CilInstruction.Create(CilOpCodes.Ldc_I8, longOperand),
                CilInstruction.Create(CilOpCodes.Pop),
                CilInstruction.Create(CilOpCodes.Pop),
                CilInstruction.Create(CilOpCodes.Pop),
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(shortOperand, instructions[0].Operand);
            Assert.Equal(operand, instructions[1].Operand);
            Assert.Equal(longOperand, instructions[2].Operand);
        }

        [Fact]
        public void OperandTypeReals()
        {
            const float shortOperand = 0.1234F;
            const double operand = 0.1234;

            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var instructions = methodBody.Instructions;
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_R4, shortOperand),
                CilInstruction.Create(CilOpCodes.Ldc_R8, operand),
                CilInstruction.Create(CilOpCodes.Pop),
                CilInstruction.Create(CilOpCodes.Pop),
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(shortOperand, instructions[0].Operand);
            Assert.Equal(operand, instructions[1].Operand);
        }


        [Fact]
        public void OperandTypeBranchTarget()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var instructions = methodBody.Instructions;
            var target1 = CilInstruction.Create(CilOpCodes.Nop);
            var target2 = CilInstruction.Create(CilOpCodes.Nop);

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Br, target1),
                target2,
                CilInstruction.Create(CilOpCodes.Ret),
                target1,
                CilInstruction.Create(CilOpCodes.Br, target2),
            });
            
            methodBody.Instructions.CalculateOffsets();
            
            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition) image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(target1.Offset, ((CilInstruction) instructions[0].Operand).Offset);
            Assert.Equal(target2.Offset, ((CilInstruction) instructions[4].Operand).Offset);
        }

        [Fact]
        public void OperandTypeSwitch()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var instructions = methodBody.Instructions;
            var target1 = CilInstruction.Create(CilOpCodes.Nop);
            var target2 = CilInstruction.Create(CilOpCodes.Nop);
            var target3 = CilInstruction.Create(CilOpCodes.Nop);

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldc_I4_1),
                CilInstruction.Create(CilOpCodes.Switch, new[] {target1, target2, target3}),
                target2,
                CilInstruction.Create(CilOpCodes.Ret),
                target1,
                CilInstruction.Create(CilOpCodes.Ret),
                target3,
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            methodBody.Instructions.CalculateOffsets();

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            var targets = (IList<CilInstruction>) newMethod.CilMethodBody.Instructions[1].Operand;

            Assert.Equal(target1.Offset, targets[0].Offset);
            Assert.Equal(target2.Offset, targets[1].Offset);
            Assert.Equal(target3.Offset, targets[2].Offset);
        }

        [Fact]
        public void OperandTypeVar()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var instructions = methodBody.Instructions;
            var var1 = new VariableSignature(image.TypeSystem.Boolean);
            var var2 = new VariableSignature(image.TypeSystem.Int32);
            methodBody.Signature = new StandAloneSignature(new LocalVariableSignature(new[] { var1, var2 }));

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldloc, var1),
                CilInstruction.Create(CilOpCodes.Stloc, var1),
                CilInstruction.Create(CilOpCodes.Ldloc, var2),
                CilInstruction.Create(CilOpCodes.Stloc, var2),
                CilInstruction.Create(CilOpCodes.Ret) 
            });

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(var1.VariableType, ((VariableSignature)instructions[0].Operand).VariableType, _comparer);
            Assert.Equal(var1.VariableType, ((VariableSignature)instructions[1].Operand).VariableType, _comparer);
            Assert.Equal(var2.VariableType, ((VariableSignature)instructions[2].Operand).VariableType, _comparer);
            Assert.Equal(var2.VariableType, ((VariableSignature)instructions[3].Operand).VariableType, _comparer);
        }

        [Fact]
        public void OperandTypeArgument()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var instructions = methodBody.Instructions;
            var param1 = new ParameterSignature(image.TypeSystem.Boolean);
            var param2 = new ParameterSignature(image.TypeSystem.Int32);
            methodBody.Method.Signature = new MethodSignature(new[] { param1, param2 }, image.TypeSystem.Void);

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldarg, param1),
                CilInstruction.Create(CilOpCodes.Starg, param1),
                CilInstruction.Create(CilOpCodes.Ldarg, param2),
                CilInstruction.Create(CilOpCodes.Starg, param2),
                CilInstruction.Create(CilOpCodes.Ret), 
            });

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(param1.ParameterType, ((ParameterSignature)instructions[0].Operand).ParameterType, _comparer);
            Assert.Equal(param1.ParameterType, ((ParameterSignature)instructions[1].Operand).ParameterType, _comparer);
            Assert.Equal(param2.ParameterType, ((ParameterSignature)instructions[2].Operand).ParameterType, _comparer);
            Assert.Equal(param2.ParameterType, ((ParameterSignature)instructions[3].Operand).ParameterType, _comparer);
        }

        [Fact]
        public void OperandTypeString()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;

            var instructions = methodBody.Instructions;

            const string operand = "Lorem Ipsum Dolor Sit Amet";
            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldstr, operand),
                CilInstruction.Create(CilOpCodes.Pop), 
                CilInstruction.Create(CilOpCodes.Ret),
            });

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(operand, instructions[0].Operand);
        }

        [Fact]
        public void OperandTypeMethod()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;
            var importer = new ReferenceImporter(image);

            var instructions = methodBody.Instructions;

            var simpleMethod = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

            var genericInstanceMethod = new MethodSpecification(
                new MemberReference(
                    importer.ImportType(typeof(Activator)),
                    "CreateInstance",
                    new MethodSignature(new GenericParameterSignature(GenericParameterType.Method, 0))
                    {
                        GenericParameterCount = 1,
                        IsGeneric = true
                    }),
                new GenericInstanceMethodSignature(importer.ImportTypeSignature(typeof(Stream))));


            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldstr, "Some String"),
                CilInstruction.Create(CilOpCodes.Call, simpleMethod),
                CilInstruction.Create(CilOpCodes.Call, importer.ImportMethod(genericInstanceMethod)),
                CilInstruction.Create(CilOpCodes.Pop),
                CilInstruction.Create(CilOpCodes.Ret)
            });
            
            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;
            
            Assert.Equal(simpleMethod, instructions[1].Operand as IMemberReference, _comparer);
            Assert.Equal(genericInstanceMethod, instructions[2].Operand as IMemberReference, _comparer);
        }

        [Fact]
        public void OperandTypeType()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;
            var importer = new ReferenceImporter(image);

            var instructions = methodBody.Instructions;

            var simpleType = importer.ImportType(typeof(XmlReader));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldtoken, simpleType));
            instructions.Add(CilInstruction.Create(CilOpCodes.Pop));

            var genericType = importer.ImportType(typeof(List<XmlReader>));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldtoken, genericType));
            instructions.Add(CilInstruction.Create(CilOpCodes.Pop));

            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(simpleType, instructions[0].Operand as ITypeDescriptor, _comparer);
            Assert.Equal(genericType, instructions[2].Operand as ITypeDescriptor, _comparer);
        }

        [Fact]
        public void OperandTypeField()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;
            var importer = new ReferenceImporter(image);

            var instructions = methodBody.Instructions;

            var simpleField = importer.ImportField(typeof(Type).GetField("EmptyTypes", BindingFlags.Public | BindingFlags.Static));

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Ldsfld, simpleField),
                CilInstruction.Create(CilOpCodes.Pop),
                CilInstruction.Create(CilOpCodes.Ret),
            });
            
            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            Assert.Equal(simpleField, instructions[0].Operand as IMemberReference, _comparer);
        }

        [Fact]
        public void OperandTypeSig()
        {
            var methodBody = CreateDummyMethodBody();
            var image = methodBody.Method.Image;
            var importer = new ReferenceImporter(image);

            var instructions = methodBody.Instructions;

            var signature = importer.ImportStandAloneSignature(
                new StandAloneSignature(new MethodSignature(image.TypeSystem.Void) { HasThis = false }));

            instructions.AddRange(new[]
            {
                CilInstruction.Create(CilOpCodes.Calli, signature),
                CilInstruction.Create(CilOpCodes.Ret)
            });

            var mapping = image.Header.UnlockMetadata();
            image = image.Header.LockMetadata();

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            instructions = newMethod.CilMethodBody.Instructions;

            var newSignature = (StandAloneSignature) instructions[0].Operand;
            Assert.Equal(signature.Signature as MethodSignature, newSignature.Signature as MethodSignature, _comparer);
        }
        
    }
}
