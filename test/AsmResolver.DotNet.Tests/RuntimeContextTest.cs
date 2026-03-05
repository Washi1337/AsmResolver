using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.Types;
using AsmResolver.IO;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class RuntimeContextTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public RuntimeContextTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void NewModuleShouldHaveNullRuntimeContext()
        {
            var module = new ModuleDefinition("Foo");
            Assert.Null(module.RuntimeContext);
        }

        [Fact]
        public void LoadAssemblyShouldCreateContextWithAssembly()
        {
            var assembly = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            Assert.NotNull(assembly.RuntimeContext);
            Assert.Contains(assembly, assembly.RuntimeContext.GetLoadedAssemblies());
        }

        [Fact]
        public void LoadModuleWithManifestShouldCreateContextWithAssembly()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            Assert.NotNull(module.RuntimeContext);
            Assert.Contains(module.Assembly, module.RuntimeContext.GetLoadedAssemblies());
        }

        [Fact]
        public void AddNewModuleShouldInheritRuntimeContext()
        {
            var assembly = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            var module = new ModuleDefinition("Foo");
            Assert.Null(module.RuntimeContext);

            assembly.Modules.Add(module);
            Assert.Same(assembly.RuntimeContext, module.RuntimeContext);
        }

        [Fact]
        public void AddExistingModuleToAssemblyShouldOverrideRuntimeContext()
        {
            var assembly = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            var module = new ModuleDefinition("Foo");

            Assert.NotNull(assembly.RuntimeContext);
            Assert.Null(module.RuntimeContext);

            assembly.Modules.Add(module);
            Assert.Same(assembly.RuntimeContext, module.RuntimeContext);
        }

        [Fact]
        public void RemoveModuleShouldUnsetRuntimeContext()
        {
            var assembly = AssemblyDefinition.FromFile(Path.Combine("Resources", "Manifest.exe"));
            Assert.NotNull(assembly.RuntimeContext);
            Assert.All(assembly.Modules,  module => Assert.Same(assembly.RuntimeContext, module.RuntimeContext));

            var module = assembly.Modules[1];
            Assert.Same(assembly.RuntimeContext, module.RuntimeContext);

            assembly.Modules.RemoveAt(1);
            Assert.Null(module.RuntimeContext);
        }

        [Fact]
        public void LoadAssemblyUsingContextShouldUseSameContext()
        {
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(3, 1), readerParameters: TestReaderParameters);
            var assembly = context.LoadAssembly(Properties.Resources.HelloWorld);

            Assert.Same(context, assembly.RuntimeContext);
            Assert.Contains(assembly, assembly.RuntimeContext!.GetLoadedAssemblies());
        }

        [Fact]
        public void ResolveDependencyShouldUseSameRuntimeContext()
        {
            var main = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var dependency = main.ManifestModule!.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve(main.RuntimeContext);

            Assert.Same(main.RuntimeContext, dependency.RuntimeContext);

            var loadedAssemblies = main.RuntimeContext!.GetLoadedAssemblies().ToArray();
            Assert.Contains(main, loadedAssemblies);
            Assert.Contains(dependency, loadedAssemblies);
        }

        [Fact]
        public void ResolveDependencyShouldUseSameFileService()
        {
            var service = new ByteArrayFileService();
            service.OpenBytesAsFile("HelloWorld.dll", Properties.Resources.HelloWorld);

            var main = ModuleDefinition.FromFile("HelloWorld.dll", new ModuleReaderParameters(service));
            var dependency = main.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve(main.RuntimeContext).ManifestModule!;

            Assert.Contains(main.FilePath, service.GetOpenedFiles());
            Assert.Contains(dependency.FilePath, service.GetOpenedFiles());
        }

        [Fact]
        public void DetectNetFrameworkContext()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Equal(
                DotNetRuntimeInfo.NetFramework(4, 0),
                module.RuntimeContext!.TargetRuntime
            );
        }

        [Fact]
        public void DetectNetCoreAppContext()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            Assert.Equal(
                DotNetRuntimeInfo.NetCoreApp(2, 2),
                module.RuntimeContext!.TargetRuntime
            );
        }

        [Fact]
        public void ForceNetFXLoadAsNetCore()
        {
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(3, 1), readerParameters: TestReaderParameters);
            var assembly = context.LoadAssembly(Properties.Resources.HelloWorld);

            Assert.Equal(context.TargetRuntime, assembly.RuntimeContext!.TargetRuntime);
            Assert.IsAssignableFrom<DotNetCoreAssemblyResolver>(assembly.RuntimeContext.AssemblyResolver);
        }

        [Fact]
        public void ForceNetStandardLoadAsNetFx()
        {
            var context = new RuntimeContext(DotNetRuntimeInfo.NetFramework(4, 8), readerParameters: TestReaderParameters);
            var assembly = context.LoadAssembly(typeof(Class).Assembly.Location);

            Assert.Equal(context.TargetRuntime, assembly.RuntimeContext!.TargetRuntime);
            Assert.Equal("mscorlib", assembly.ManifestModule!.CorLibTypeFactory.Object.Resolve(context).DeclaringModule?.Assembly?.Name);
        }

        [Fact]
        public void ForceNetStandardLoadAsNetCore()
        {
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(8, 0), readerParameters: TestReaderParameters);
            var assembly = context.LoadAssembly(typeof(Class).Assembly.Location);

            Assert.Equal(context.TargetRuntime, assembly.RuntimeContext!.TargetRuntime);
            Assert.Equal("System.Private.CoreLib", assembly.ManifestModule!.CorLibTypeFactory.Object.Resolve(context).DeclaringModule?.Assembly?.Name);
        }

        [Fact]
        public void ResolveSameDependencyInSameContextShouldResultInSameAssembly()
        {
            var module1 = ModuleDefinition.FromFile(typeof(Class).Assembly.Location, TestReaderParameters);
            var context = module1.RuntimeContext!;
            var module2 = context.LoadAssembly(typeof(SingleMethod).Assembly.Location).ManifestModule!;

            var object1 = module1.CorLibTypeFactory.Object.Resolve(context);
            var object2 = module2.CorLibTypeFactory.Object.Resolve(module2.RuntimeContext);

            Assert.Same(object1, object2);
        }

        [Fact]
        public void LoadReferenceCorLibShouldNotSetAsRuntimeCorLib()
        {
            string? path = new DotNetCoreAssemblyResolver(new Version(10, 0))
                .ProbeAssemblyFilePath(KnownCorLibs.SystemRuntime_v10_0_0_0, null);

            Assert.NotNull(path);

            var module = ModuleDefinition.FromFile(path, TestReaderParameters);
            var context = module.RuntimeContext!;

            Assert.NotSame(module.Assembly, context.RuntimeCorLib);
        }

        [Fact]
        public void LoadImplementationCorLibShouldSetAsRuntimeCorLib()
        {
            var module = ModuleDefinition.FromFile(typeof(object).Assembly.Location, TestReaderParameters);
            var context = module.RuntimeContext!;

            Assert.Same(module.Assembly, context.RuntimeCorLib);
        }
    }
}
