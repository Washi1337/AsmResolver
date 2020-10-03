using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class CilMethodBodySerializerTest
    {
        [Theory]
        [InlineData(false, true, 0)]
        [InlineData(false, false, 100)]
        [InlineData(false, null, 100)]
        [InlineData(true, true, 0)]
        [InlineData(true, false, 100)]
        [InlineData(true, null, 0)]
        public void ComputeMaxStackOnBuildOverride(bool computeMaxStack, bool? computeMaxStackOverride, int expectedMaxStack)
        {
            const int maxStack = 100;

            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemPrivateCoreLib_v4_0_0_0);
            var main = new MethodDefinition(
                "Main", 
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));

            main.CilMethodBody = new CilMethodBody(main)
            {
                ComputeMaxStackOnBuild = computeMaxStack,
                MaxStack = maxStack,
                Instructions = {new CilInstruction(CilOpCodes.Ret)},
                LocalVariables = {new CilLocalVariable(module.CorLibTypeFactory.Int32)} // Force fat method body.
            };
            
            module.GetOrCreateModuleType().Methods.Add(main);
            module.ManagedEntrypoint = main;
            
            var builder = new ManagedPEImageBuilder(new DotNetDirectoryFactory
            {
                MethodBodySerializer = new CilMethodBodySerializer
                {
                    ComputeMaxStackOnBuildOverride = computeMaxStackOverride
                }
            });

            var newImage = builder.CreateImage(module).ConstructedImage;
            var newModule = ModuleDefinition.FromImage(newImage);

            Assert.Equal(expectedMaxStack, newModule.ManagedEntrypointMethod.CilMethodBody.MaxStack);
        }
    }
}