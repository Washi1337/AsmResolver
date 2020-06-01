using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class ParameterTokenPreservationTest : TokenPreservationTestBase
    {
        private static ModuleDefinition CreateSampleParameterDefsModule(int methodsPerType, int parametersPerMethod)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);

            var dummyType = new TypeDefinition("Namespace", $"Type",
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            module.TopLevelTypes.Add(dummyType);
            
            for (int j = 0; j < methodsPerType; j++)
                dummyType.Methods.Add(CreateDummyMethod(module, $"Method{j}", parametersPerMethod));
            
            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static MethodDefinition CreateDummyMethod(ModuleDefinition module, string name, int parameterCount)
        {
            var method = new MethodDefinition(name,
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));
            
            method.CilMethodBody = new CilMethodBody(method);
            method.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));

            for (int i = 0; i < parameterCount; i++)
            {
                method.Signature.ParameterTypes.Add(module.CorLibTypeFactory.Object);
                method.ParameterDefinitions.Add(new ParameterDefinition((ushort) (i + 1), null, 0));
            }
            
            method.Parameters.PullUpdatesFromMethodSignature();
            return method;
        }

        [Fact]
        public void PreserveParameterDefsNoChange()
        {
            var module = CreateSampleParameterDefsModule(10, 3);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveParameterDefsChangeOrderOfTypes()
        {
            var module = CreateSampleParameterDefsModule(10, 3);
            
            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveParameterDefsChangeOrderOfMethodsInType()
        {
            var module = CreateSampleParameterDefsModule(10, 3);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var method = type.Methods[swapIndex];
            type.Methods.RemoveAt(swapIndex);
            type.Methods.Insert(swapIndex + 1, method);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveParameterDefsAddExtraMethod()
        {
            var module = CreateSampleParameterDefsModule(10, 3);

            var type = module.TopLevelTypes[2];
            var method = CreateDummyMethod(module, "ExtraMethod", 3);
            type.Methods.Insert(3, method);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveParameterDefsRemoveMethod()
        {
            var module = CreateSampleParameterDefsModule(10, 3);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var method = type.Methods[indexToRemove];
            type.Methods.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameTokens(module, newModule, m => m.Methods, method.MetadataToken);
        }
        
    }
}