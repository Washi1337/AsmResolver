using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Cil;
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
    }
}
