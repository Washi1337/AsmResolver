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

            DynamicMethod generateDynamicMethod = TDynamicMethod.GenerateDynamicMethod();

            var dynamicMethodDefinition = new DynamicMethodDefinition(module, generateDynamicMethod);

            Assert.NotNull(dynamicMethodDefinition);

            Assert.NotEmpty(dynamicMethodDefinition.CilMethodBody.Instructions);
            
            Assert.Equal(dynamicMethodDefinition.CilMethodBody.Instructions.Select(q=>q.OpCode),new []
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
        public void RtDynamicMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(TDynamicMethod).Assembly.Location);

            DynamicMethod generateDynamicMethod = TDynamicMethod.GenerateDynamicMethod();

            var rtDynamicMethod = generateDynamicMethod.GetType().GetField("m_dynMethod", (BindingFlags) (-1))?.GetValue(generateDynamicMethod); 
            
            var dynamicMethodDefinition = new DynamicMethodDefinition(module, rtDynamicMethod);

            Assert.NotNull(dynamicMethodDefinition);

            Assert.NotEmpty(dynamicMethodDefinition.CilMethodBody.Instructions);
            
            Assert.Equal(dynamicMethodDefinition.CilMethodBody.Instructions.Select(q=>q.OpCode),new []
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
        public void ReadDynamicMethodToCilMethodBody()
        {
            var module = ModuleDefinition.FromFile(typeof(TDynamicMethod).Assembly.Location);

            var type = module.TopLevelTypes.First(t => t.Name == nameof(TDynamicMethod));

            var method = type.Methods.FirstOrDefault(m => m.Name == nameof(TDynamicMethod.GenerateDynamicMethod));

            DynamicMethod generateDynamicMethod = TDynamicMethod.GenerateDynamicMethod();

            //Dynamic method => CilMethodBody
            var body = DynamicMethodDefinition.ToCilMethodBody(method, generateDynamicMethod);

            Assert.NotNull(body);

            Assert.NotEmpty(body.Instructions);

            Assert.Equal(body.Instructions.Select(q => q.OpCode), new CilOpCode[]
            {
                CilOpCodes.Ldarg_0,
                CilOpCodes.Call,
                CilOpCodes.Ldarg_1,
                CilOpCodes.Ret
            });
        }
    }
}
