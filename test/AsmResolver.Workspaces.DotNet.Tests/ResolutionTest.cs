using System;
using AsmResolver.DotNet;
using Xunit;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class ResolutionTest
    {
        [Fact]
        public void ReferenceAssemblyInWorkspace()
        {
            // Set up dummy assemblies.
            var assembly1 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module1 = new ModuleDefinition("Assembly1");
            var assembly2 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module2 = new ModuleDefinition("Assembly2");

            assembly1.Modules.Add(module1);
            assembly2.Modules.Add(module2);

            var reference = new AssemblyReference(assembly1);
            module2.AssemblyReferences.Add(reference);

            // Set up workspace.
            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(assembly1);
            workspace.Assemblies.Add(assembly2);

            // Verify if reference resolves to assembly inside the workspace.
            Assert.Same(assembly1, reference.Resolve());
        }

        [Fact]
        public void ReferenceAssemblyOutsideWorkspace()
        {
            // Set up dummy assemblies.
            var assembly1 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module1 = new ModuleDefinition("Assembly1");
            assembly1.Modules.Add(module1);

            var reference = (AssemblyReference) module1.CorLibTypeFactory.CorLibScope;

            // Set up workspace.
            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(assembly1);

            // Verify if reference resolves to assembly inside the workspace.
            Assert.NotNull(reference.Resolve());
        }
    }
}
