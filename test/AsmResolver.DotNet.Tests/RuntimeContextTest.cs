using System;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.Types;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class RuntimeContextTest
    {
        [Fact]
        public void ResolveDependencyShouldUseSameRuntimeContext()
        {
            var main = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var dependency = main.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve()!.ManifestModule!;

            Assert.Same(main.RuntimeContext, dependency.RuntimeContext);
        }

        [Fact]
        public void ResolveDependencyShouldUseSameFileService()
        {
            var service = new ByteArrayFileService();
            service.OpenBytesAsFile("HelloWorld.dll", Properties.Resources.HelloWorld);

            var main = ModuleDefinition.FromFile("HelloWorld.dll", new ModuleReaderParameters(service));
            var dependency = main.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve()!.ManifestModule!;

            Assert.Contains(main.FilePath, service.GetOpenedFiles());
            Assert.Contains(dependency.FilePath, service.GetOpenedFiles());
        }

        [Fact]
        public void DetectNetFrameworkContext()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Equal(
                new DotNetRuntimeInfo(DotNetRuntimeInfo.NetFramework, new Version(4, 0)),
                module.RuntimeContext.TargetRuntime
            );
        }

        [Fact]
        public void DetectNetCoreAppContext()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            Assert.Equal(
                new DotNetRuntimeInfo(DotNetRuntimeInfo.NetCoreApp, new Version(2, 2)),
                module.RuntimeContext.TargetRuntime
            );
        }

        [Fact]
        public void ForceNetFXLoadAsNetCore()
        {
            var context = new RuntimeContext(new DotNetRuntimeInfo(DotNetRuntimeInfo.NetCoreApp, new Version(3, 1)));
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, new ModuleReaderParameters(context));

            Assert.Equal(context.TargetRuntime, module.RuntimeContext.TargetRuntime);
            Assert.IsAssignableFrom<DotNetCoreAssemblyResolver>(module.MetadataResolver.AssemblyResolver);
        }

        [Fact]
        public void ForceNetStandardLoadAsNetFx()
        {
            var context = new RuntimeContext(new DotNetRuntimeInfo(DotNetRuntimeInfo.NetFramework, new Version(4, 8)));
            var module = ModuleDefinition.FromFile(typeof(Class).Assembly.Location, new ModuleReaderParameters(context));

            Assert.Equal(context.TargetRuntime, module.RuntimeContext.TargetRuntime);
            Assert.Equal("mscorlib", module.CorLibTypeFactory.Object.Resolve()?.Module?.Assembly?.Name);
        }

        [Fact]
        public void ForceNetStandardLoadAsNetCore()
        {
            var context = new RuntimeContext(new DotNetRuntimeInfo(DotNetRuntimeInfo.NetCoreApp, new Version(3, 1)));
            var module = ModuleDefinition.FromFile(typeof(Class).Assembly.Location, new ModuleReaderParameters(context));

            Assert.Equal(context.TargetRuntime, module.RuntimeContext.TargetRuntime);
            Assert.Equal("System.Private.CoreLib", module.CorLibTypeFactory.Object.Resolve()?.Module?.Assembly?.Name);
        }

        [Fact]
        public void ResolveSameDependencyInSameContextShouldResultInSameAssembly()
        {
            var module1 = ModuleDefinition.FromFile(typeof(Class).Assembly.Location, TestReaderParameters);
            var module2 = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location, new ModuleReaderParameters
            {
                RuntimeContext = module1.RuntimeContext
            });

            var object1 = module1.CorLibTypeFactory.Object.Resolve();
            var object2 = module2.CorLibTypeFactory.Object.Resolve();

            Assert.NotNull(object1);
            Assert.Same(object1, object2);
        }
    }
}
