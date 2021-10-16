using System.Linq;
using AsmResolver.Workspaces.DotNet.TestCases;
using AsmResolver.DotNet;
using AsmResolver.Workspaces.DotNet;
using Xunit;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class AbstractInheritanceTest : IClassFixture<TestCasesFixture>
    {
        private readonly TestCasesFixture _fixture;

        public AbstractInheritanceTest(TestCasesFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void InterfaceImplementation()
        {
            var module = _fixture.WorkspacesAssembly.ManifestModule;
            var abstractType = (TypeDefinition)module.LookupMember(typeof(MyAboveAbstractClass).MetadataToken);
            var abstractMethod = abstractType.Methods.First(m=>m.Name == nameof(MyAboveAbstractClass.TestAboveAbstract));

            var implType1 = (TypeDefinition)module.LookupMember(typeof(MyDerivedAboveClass).MetadataToken);
            var implMethod1 = abstractType.Methods.First(m => m.Name == nameof(MyAboveAbstractClass.TestAboveAbstract));

            var implType2 = (TypeDefinition)module.LookupMember(typeof(MyDerivedInbetweenClass).MetadataToken);
            var implMethod2 = abstractType.Methods.First(m => m.Name == nameof(MyAboveAbstractClass.TestAboveAbstract));

            var implType3 = (TypeDefinition)module.LookupMember(typeof(MyDerivedClassGeneric).MetadataToken);
            var implMethod3 = abstractType.Methods.First(m => m.Name == nameof(MyAboveAbstractClass.TestAboveAbstract));


            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.WorkspacesAssembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(abstractMethod);
            var implMethods = node.BackwardRelations.GetObjects(DotNetRelations.ImplementationMethod);

            Assert.Contains(implMethod1, implMethods);
            Assert.Contains(implMethod2, implMethods);
            Assert.Contains(implMethod3, implMethods);
        }
    }
}
