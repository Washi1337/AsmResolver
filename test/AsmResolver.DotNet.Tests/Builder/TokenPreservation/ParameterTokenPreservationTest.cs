using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class ParameterTokenPreservationTest : TokenPreservationTestBase
    {
        private static ModuleDefinition CreateSampleParameterDefsModule(int typeCount, int methodsPerType, int parametersPerMethod)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
                module.TopLevelTypes.Add(dummyType);

                for (int j = 0; j < methodsPerType; j++)
                    dummyType.Methods.Add(CreateDummyMethod(module, $"Method{j}", parametersPerMethod));
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }
        
        protected static void AssertSameParameterTokens(ModuleDefinition module, ModuleDefinition newModule, params MetadataToken[] excludeTokens)
        {
            Assert.True(module.TopLevelTypes.Count <= newModule.TopLevelTypes.Count);
            foreach (var originalType in module.TopLevelTypes)
            {
                var newType = newModule.TopLevelTypes.First(t => t.FullName == originalType.FullName);

                var originalMethods = originalType.Methods;
                var newMethods = newType.Methods.ToArray();
                foreach (var originalMethod in originalMethods)
                {
                    var newMethod = newMethods.First(m => m.Name == originalMethod.Name);

                    var originalParameters = originalMethod.ParameterDefinitions;
                    var newParameters = newMethod.ParameterDefinitions;
                    foreach (var originalParameter in originalParameters)
                    {
                        if (originalParameter.MetadataToken.Rid == 0
                            || excludeTokens.Contains(originalParameter.MetadataToken))
                        {
                            continue;
                        }

                        var newParameter = newParameters.First(p => p.Sequence == originalParameter.Sequence);
                        Assert.Equal(originalParameter.MetadataToken, newParameter.MetadataToken);
                    }
                }
            }
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
                method.ParameterDefinitions.Add(new ParameterDefinition((ushort) (i + 1), $"arg_{i}", 0));
            }
            
            method.Parameters.PullUpdatesFromMethodSignature();
            return method;
        }

        [Fact]
        public void PreserveParameterDefsNoChange()
        {
            var module = CreateSampleParameterDefsModule(3, 10, 3);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameParameterTokens(module, newModule);
        }

        [Fact]
        public void PreserveParameterDefsChangeOrderOfTypes()
        {
            var module = CreateSampleParameterDefsModule(5, 10, 3);
            
            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 3, type);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameParameterTokens(module, newModule);
        }

        [Fact]
        public void PreserveParameterDefsChangeOrderOfMethodsInType()
        {
            var module = CreateSampleParameterDefsModule(3, 10, 3);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var method = type.Methods[swapIndex];
            type.Methods.RemoveAt(swapIndex);
            type.Methods.Insert(swapIndex + 3, method);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameParameterTokens(module, newModule);
        }

        [Fact]
        public void PreserveParameterDefsAddExtraMethod()
        {
            var module = CreateSampleParameterDefsModule(3, 10, 3);

            var type = module.TopLevelTypes[2];
            var method = CreateDummyMethod(module, "ExtraMethod", 3);
            type.Methods.Insert(3, method);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameParameterTokens(module, newModule);
        }

        [Fact]
        public void PreserveParameterDefsRemoveMethod()
        {
            var module = CreateSampleParameterDefsModule(3, 10, 3);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var method = type.Methods[indexToRemove];
            type.Methods.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameParameterTokens(
                module,
                newModule,
                method.ParameterDefinitions.Select(p => p.MetadataToken).ToArray());
        }

        [Fact]
        public void PreserveParameterDefsReOrderParameters()
        {
            var module = CreateSampleParameterDefsModule(1, 1, 5);

            var parameters = module
                .TopLevelTypes.Last()
                .Methods[0]
                .ParameterDefinitions;
                
            const int swapIndex = 3;
            var parameter = parameters[swapIndex];
            parameters.RemoveAt(swapIndex);
            parameters.Insert(swapIndex + 1, parameter);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveParameterDefinitionIndices);

            AssertSameParameterTokens(module, newModule);
        }

        [Fact]
        public void PreserveMethodAndParametersAfterRemovingMethod()
        {
            var module = CreateSampleParameterDefsModule(1, 1, 5);

            var parameters = module
                .TopLevelTypes.Last()
                .Methods[0]
                .ParameterDefinitions;
                
            const int swapIndex = 3;
            var parameter = parameters[swapIndex];
            parameters.RemoveAt(swapIndex);
            parameters.Insert(swapIndex + 1, parameter);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveParameterDefinitionIndices
                                                           | MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameParameterTokens(module, newModule);
        }
    }
}