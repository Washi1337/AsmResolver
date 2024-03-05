using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Dynamic.Tests
{
    public class DynamicMethodDefinitionTest
    {
        [Fact]
        public void ReadDynamicMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(TDynamicMethod).Assembly.Location);
            var generatedDynamicMethod = TDynamicMethod.GenerateDynamicMethod();
            var dynamicMethodDef = new DynamicMethodDefinition(module, generatedDynamicMethod);

            Assert.NotNull(dynamicMethodDef);
            Assert.NotEmpty(dynamicMethodDef.CilMethodBody!.Instructions);
            Assert.Equal(new[]
            {
                CilCode.Ldarg_0,
                CilCode.Stloc_0,
                CilCode.Ldloc_0,
                CilCode.Call,
                CilCode.Ldarg_1,
                CilCode.Ret
            }, dynamicMethodDef.CilMethodBody.Instructions.Select(q => q.OpCode.Code));
            Assert.Equal(new TypeSignature[]
            {
                module.CorLibTypeFactory.String,
                module.CorLibTypeFactory.Int32
            }, dynamicMethodDef.Parameters.Select(q => q.ParameterType));
            Assert.Equal(new TypeSignature[]
            {
                module.CorLibTypeFactory.String,
            }, dynamicMethodDef.CilMethodBody.LocalVariables.Select(v => v.VariableType));
        }

        [Fact]
        public void RtDynamicMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(TDynamicMethod).Assembly.Location);

            var generatedDynamicMethod = TDynamicMethod.GenerateDynamicMethod();
            object rtDynamicMethod = generatedDynamicMethod
                .GetType()
                .GetField("m_dynMethod", (BindingFlags) (-1))?
                .GetValue(generatedDynamicMethod) ?? generatedDynamicMethod;
            var dynamicMethod = new DynamicMethodDefinition(module, rtDynamicMethod!);

            Assert.NotNull(dynamicMethod);
            Assert.NotEmpty(dynamicMethod.CilMethodBody!.Instructions);
            Assert.Equal(new[]
            {
                CilCode.Ldarg_0,
                CilCode.Stloc_0,
                CilCode.Ldloc_0,
                CilCode.Call,
                CilCode.Ldarg_1,
                CilCode.Ret
            }, dynamicMethod.CilMethodBody.Instructions.Select(q => q.OpCode.Code));
            Assert.Equal(new TypeSignature[]
            {
                module.CorLibTypeFactory.String,
                module.CorLibTypeFactory.Int32
            }, dynamicMethod.Parameters.Select(q => q.ParameterType));
            Assert.Equal(new TypeSignature[]
            {
                module.CorLibTypeFactory.String,
            }, dynamicMethod.CilMethodBody.LocalVariables.Select(v => v.VariableType));
        }

        [Fact]
        public void ReadDynamicMethodInitializedByDynamicILInfo()
        {
            var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
            var info = method.GetDynamicILInfo();
            info.SetLocalSignature(new byte[] { 0x7, 0x0 });
            info.SetCode(new byte[] {0x2a}, 1);

            var contextModule = ModuleDefinition.FromFile(typeof(DynamicMethodDefinitionTest).Assembly.Location);
            var definition = new DynamicMethodDefinition(contextModule, method);

            Assert.NotNull(definition.CilMethodBody);
            var instruction = Assert.Single(definition.CilMethodBody.Instructions);
            Assert.Equal(CilOpCodes.Ret, instruction.OpCode);
        }

        [Fact]
        public void ImportNestedType()
        {
            // https://github.com/Washi1337/AsmResolver/issues/363

            var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
            var cil = method.GetILGenerator();
            cil.Emit(OpCodes.Call, typeof(NestedClass).GetMethod(nameof(NestedClass.TestMethod))!);
            cil.Emit(OpCodes.Ret);

            var contextModule = ModuleDefinition.FromFile(typeof(DynamicMethodDefinitionTest).Assembly.Location);
            var definition = new DynamicMethodDefinition(contextModule, method);

            Assert.NotNull(definition.CilMethodBody);
            var reference = Assert.IsAssignableFrom<IMethodDescriptor>(definition.CilMethodBody.Instructions[0].Operand);
            var declaringType = reference.DeclaringType;
            Assert.NotNull(declaringType);
            Assert.Equal(nameof(NestedClass), declaringType.Name);
            Assert.NotNull(declaringType.DeclaringType);
            Assert.Equal(nameof(DynamicMethodDefinitionTest), declaringType.DeclaringType.Name);
        }

        [Fact]
        public void ImportGenericTypeInstantiation()
        {
            var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
            var cil = method.GetILGenerator();
            cil.Emit(OpCodes.Call, typeof(GenericType<int, string, Stream>)
                .GetMethod(nameof(GenericType<int, string, Stream>.NonGenericMethodInGenericType))!);
            cil.Emit(OpCodes.Ret);

            var contextModule = ModuleDefinition.FromFile(typeof(DynamicMethodDefinitionTest).Assembly.Location);
            var definition = new DynamicMethodDefinition(contextModule, method);

            Assert.NotNull(definition.CilMethodBody);
            var instruction = definition.CilMethodBody.Instructions[0];
            Assert.Equal(CilOpCodes.Call, instruction.OpCode);
            var operand = Assert.IsAssignableFrom<IMethodDescriptor>(instruction.Operand);
            var type = Assert.IsAssignableFrom<TypeSpecification>(operand.DeclaringType);
            var signature = Assert.IsAssignableFrom<GenericInstanceTypeSignature>(type.Signature);
            Assert.Equal("Int32",  signature.TypeArguments[0].Name);
            Assert.Equal("String", signature.TypeArguments[1].Name);
            Assert.Equal("Stream", signature.TypeArguments[2].Name);
        }

        [Fact]
        public void ImportGenericMethodInstantiation()
        {
            var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
            var cil = method.GetILGenerator();
            cil.Emit(OpCodes.Call, typeof(NonGenericType)
                .GetMethod(nameof(NonGenericType.GenericMethodInNonGenericType))!
                .MakeGenericMethod(typeof(int), typeof(string), typeof(Stream)));
            cil.Emit(OpCodes.Ret);

            var contextModule = ModuleDefinition.FromFile(typeof(DynamicMethodDefinitionTest).Assembly.Location);
            var definition = new DynamicMethodDefinition(contextModule, method);

            Assert.NotNull(definition.CilMethodBody);
            var instruction = definition.CilMethodBody.Instructions[0];
            Assert.Equal(CilOpCodes.Call, instruction.OpCode);
            var operand = Assert.IsAssignableFrom<MethodSpecification>(instruction.Operand);
            Assert.Equal(nameof(NonGenericType.GenericMethodInNonGenericType), operand.Name);
            Assert.NotNull(operand.Signature);
            Assert.Equal(3, operand.Method!.Signature!.GenericParameterCount);
            Assert.Equal("Int32", operand.Signature.TypeArguments[0].Name);
            Assert.Equal("String", operand.Signature.TypeArguments[1].Name);
            Assert.Equal("Stream", operand.Signature.TypeArguments[2].Name);
        }

        [Fact]
        public void ReadDynamicMethodInitializedByDynamicILInfoWithTokens()
        {
            // Create new dynamic method.
            var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
            var info = method.GetDynamicILInfo();
            info.SetLocalSignature(new byte[] { 0x7, 0x0 });

            // Write some IL.
            using var codeStream = new MemoryStream();
            var assembler = new CilAssembler(
                new BinaryStreamWriter(codeStream),
                new CilOperandBuilder(new OriginalMetadataTokenProvider(null), EmptyErrorListener.Instance));
            uint token = (uint)info.GetTokenFor(typeof(Console).GetMethod("WriteLine", Type.EmptyTypes).MethodHandle);
            assembler.WriteInstruction(new CilInstruction(CilOpCodes.Call, new MetadataToken(token)));
            assembler.WriteInstruction(new CilInstruction(CilOpCodes.Ret));

            // Set code.
            info.SetCode(codeStream.ToArray(), 1);

            // Pass into DynamicMethodDefinition
            var contextModule = ModuleDefinition.FromFile(typeof(DynamicMethodDefinitionTest).Assembly.Location);
            var definition = new DynamicMethodDefinition(contextModule, method);

            // Verify
            Assert.NotNull(definition.CilMethodBody);
            var instruction = definition.CilMethodBody.Instructions[0];
            Assert.Equal(CilOpCodes.Call, instruction.OpCode);
            var reference = Assert.IsAssignableFrom<IMethodDescriptor>(instruction.Operand);
            Assert.Equal("WriteLine", reference.Name);
        }

        [SkippableFact]
        public void ReadDynamicMethodInitializedByDynamicILInfoWithLocals()
        {
            Skip.IfNot(DynamicTypeSignatureResolver.IsSupported, "Current platform does not support dynamic type resolution.");

            // Create new dynamic method.
            var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
            var info = method.GetDynamicILInfo();

            var helper = SignatureHelper.GetLocalVarSigHelper();
            helper.AddArgument(typeof(int));
            helper.AddArgument(typeof(bool));
            helper.AddArgument(typeof(Stream));
            helper.AddArgument(typeof(Stream[]));
            info.SetLocalSignature(helper.GetSignature());

            // Write some IL.
            info.SetCode(new byte[] {0x2a}, 1);

            // Pass into DynamicMethodDefinition
            var contextModule = ModuleDefinition.FromFile(typeof(DynamicMethodDefinitionTest).Assembly.Location);
            var definition = new DynamicMethodDefinition(contextModule, method);

            // Verify
            Assert.NotNull(definition.CilMethodBody);
            var locals = definition.CilMethodBody.LocalVariables;
            Assert.Equal(4, locals.Count);
            Assert.Equal("Int32", locals[0].VariableType.Name);
            Assert.Equal("Boolean", locals[1].VariableType.Name);
            Assert.Equal("Stream", locals[2].VariableType.Name);
            Assert.Equal("Stream", Assert.IsAssignableFrom<SzArrayTypeSignature>(locals[3].VariableType).BaseType.Name);
        }

        internal static class NestedClass
        {
            public static void TestMethod() => Console.WriteLine("TestMethod");
        }
    }
}
