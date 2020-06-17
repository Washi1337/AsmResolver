using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class DynamicMethodDefinitionTest
    {
        [Fact]
        public void ReadDynamicMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(TDynamicMethod).Assembly.Location);

            DynamicMethod generateDynamicMethod = TDynamicMethod.GenerateDynamicMethod();

            var dynamicMethodDefinition = new DynamicMethodDefinition(module, generateDynamicMethod);

            Assert.NotNull(dynamicMethodDefinition);

            Assert.NotEmpty(dynamicMethodDefinition.CilMethodBody.Instructions);
            
            Assert.Equal(dynamicMethodDefinition.CilMethodBody.Instructions.Select(q=>q.OpCode),new CilOpCode[]
            {
                CilOpCodes.Ldarg_0, 
                CilOpCodes.Call,
                CilOpCodes.Ldarg_1,
                CilOpCodes.Ret
            });
            Assert.Equal(dynamicMethodDefinition.Parameters.Select(q=>q.ParameterType),new TypeSignature[]
            {
                module.CorLibTypeFactory.String,
                module.CorLibTypeFactory.Int32
            });
        }
        
        [Fact]
        public void RTDynamicMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(TDynamicMethod).Assembly.Location);

            DynamicMethod generateDynamicMethod = TDynamicMethod.GenerateDynamicMethod();

            var RTDynamicMethod = generateDynamicMethod.GetType().GetField("m_dynMethod", (BindingFlags) (-1))?.GetValue(generateDynamicMethod); 
            
            var dynamicMethodDefinition = new DynamicMethodDefinition(module, RTDynamicMethod);

            Assert.NotNull(dynamicMethodDefinition);

            Assert.NotEmpty(dynamicMethodDefinition.CilMethodBody.Instructions);
            
            Assert.Equal(dynamicMethodDefinition.CilMethodBody.Instructions.Select(q=>q.OpCode),new CilOpCode[]
            {
                CilOpCodes.Ldarg_0, 
                CilOpCodes.Call,
                CilOpCodes.Ldarg_1,
                CilOpCodes.Ret
            });
            Assert.Equal(dynamicMethodDefinition.Parameters.Select(q=>q.ParameterType),new TypeSignature[]
            {
                module.CorLibTypeFactory.String,
                module.CorLibTypeFactory.Int32
            });
        }
    }
}