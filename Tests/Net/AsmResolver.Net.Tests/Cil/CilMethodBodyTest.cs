using System;
using System.Reflection;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;
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
            image.Assembly.Modules[0].Types.Add(type);

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
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldstr, "Lorem Ipsum"));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

            Assert.Equal(1, methodBody.ComputeMaxStack());
        }

        [Fact]
        public void ComputeMaxStackTestSimple()
        {
            var methodBody = CreateDummyMethodBody();
            var importer = new ReferenceImporter(methodBody.Method.Image);

            var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) }));

            var instructions = methodBody.Instructions;
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(CilInstruction.Create(CilOpCodes.Add));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

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
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Br, target));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(target);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

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

            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_0));
            instructions.Add(CilInstruction.Create(CilOpCodes.Brtrue, elseInstr));
            instructions.Add(CilInstruction.Create(CilOpCodes.Add));
            instructions.Add(CilInstruction.Create(CilOpCodes.Br, endInstr));
            instructions.Add(elseInstr);
            instructions.Add(endInstr);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

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

            instructions.Add(loopHead);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(CilInstruction.Create(CilOpCodes.Add));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_0));
            instructions.Add(CilInstruction.Create(CilOpCodes.Brtrue, loopHead));

            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

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

            instructions.Add(tryStart);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_0));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Leave, handlerEnd));
            instructions.Add(handlerStart);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(CilInstruction.Create(CilOpCodes.Add));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Leave, handlerEnd));
            instructions.Add(handlerEnd);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

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
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Brtrue, target));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(target);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));
            
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

            instructions.Add(loopHead);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(CilInstruction.Create(CilOpCodes.Add));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_0));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Brtrue, loopHead));

            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

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
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldstr, "Lorem Ipsum"));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));
            
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

            instructions.Add(tryStart);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_0));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Leave, handlerEnd));
            instructions.Add(handlerStart);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_1));
            instructions.Add(CilInstruction.Create(CilOpCodes.Ldc_I4_2));
            instructions.Add(CilInstruction.Create(CilOpCodes.Add));
            instructions.Add(CilInstruction.Create(CilOpCodes.Call, writeLine));
            instructions.Add(CilInstruction.Create(CilOpCodes.Leave, handlerEnd));
            instructions.Add(handlerEnd);
            instructions.Add(CilInstruction.Create(CilOpCodes.Ret));

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

            var newMethod = (MethodDefinition)image.ResolveMember(mapping[methodBody.Method]);
            Assert.Equal(1, newMethod.CilMethodBody.ExceptionHandlers.Count);
            var handler = newMethod.CilMethodBody.ExceptionHandlers[0];
            Assert.Equal(tryStart, handler.TryStart, instructionComparer);
            Assert.Equal(handlerStart, handler.TryEnd, instructionComparer);
            Assert.Equal(handlerStart, handler.HandlerStart, instructionComparer);
            Assert.Equal(handlerEnd, handler.HandlerEnd, instructionComparer);
            Assert.Equal(exceptionType, handler.CatchType, _comparer);
        }
    }
}
