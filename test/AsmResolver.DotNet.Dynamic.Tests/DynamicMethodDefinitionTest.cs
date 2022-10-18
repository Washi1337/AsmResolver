using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Cil;
using Xunit;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.MethodAttributes;

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
                .GetValue(generatedDynamicMethod);
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

        internal static class NestedClass
        {
            public static void TestMethod() => Console.WriteLine("TestMethod");
        }
    }
}
