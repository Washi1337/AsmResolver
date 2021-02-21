using System.Linq;
using AsmResolver.Workspaces.DotNet.TestCases;
using AsmResolver.DotNet;
using AsmResolver.Workspaces.Dotnet;
using Xunit;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class InheritanceTest : IClassFixture<TestCasesFixture>
    {
        private readonly TestCasesFixture _fixture;

        public InheritanceTest(TestCasesFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void InterfaceImplementation()
        {
            var module = _fixture.Assembly.ManifestModule;
            var classType = (TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken);
            var interfaceType = (TypeDefinition) module.LookupMember(typeof(IMyInterface).MetadataToken);

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(classType);
            Assert.Contains(interfaceType, node.GetRelatedObjects(DotNetRelations.BaseType));
        }

        [Fact]
        public void DerivedClassRelation()
        {
            var module = _fixture.Assembly.ManifestModule;
            var derivedType = (TypeDefinition) module.LookupMember(typeof(MyDerivedClass).MetadataToken);
            var classType = (TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken);

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(derivedType);
            Assert.Contains(classType, node.GetRelatedObjects(DotNetRelations.BaseType));
        }

        [Fact]
        public void ExplicitMethodImplementation()
        {
            const string name = "AsmResolver.Workspaces.DotNet.TestCases.IMyInterface.Explicit";

            var module = _fixture.Assembly.ManifestModule;
            var implementationMethod =
                ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Methods.First(m => m.Name == name);
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(IMyInterface).MetadataToken))
                .Methods.First(m => m.Name == nameof(IMyInterface.Explicit));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationMethod);
            Assert.Contains(baseMethod, node.GetRelatedObjects(DotNetRelations.ImplementationMethod));
        }

        [Fact]
        public void ImplicitMethodImplementation()
        {
            var module = _fixture.Assembly.ManifestModule;
            var implementationMethod =
                ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClass.Implicit));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(IMyInterface).MetadataToken))
                .Methods.First(m => m.Name == nameof(IMyInterface.Implicit));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationMethod);
            Assert.Contains(baseMethod, node.GetRelatedObjects(DotNetRelations.ImplementationMethod));
        }

        [Fact]
        public void ImplicitMethodOverride()
        {
            var module = _fixture.Assembly.ManifestModule;
            var overrideMethod =
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClass).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyDerivedClass.Implicit));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClass.Implicit));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(overrideMethod);
            Assert.Contains(baseMethod, node.GetRelatedObjects(DotNetRelations.ImplementationMethod));
        }

        [Fact]
        public void MethodShadowingShouldNotLinkMethodsTogether()
        {
            var module = _fixture.Assembly.ManifestModule;
            var shadowedMethod =
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClass).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyDerivedClass.Shadowed));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClass.Shadowed));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(shadowedMethod);
            Assert.DoesNotContain(baseMethod, node.GetRelatedObjects(DotNetRelations.ImplementationMethod));
        }

        [Fact]
        public void ImplicitMethodImplementationThatIsShadowedInDerivedType()
        {
            var module = _fixture.Assembly.ManifestModule;
            var implementationMethod =
                ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClass.Shadowed));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(IMyInterface).MetadataToken))
                .Methods.First(m => m.Name == nameof(IMyInterface.Shadowed));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationMethod);
            Assert.Contains(baseMethod, node.GetRelatedObjects(DotNetRelations.ImplementationMethod));
        }
        
        [Fact]
        public void ExplicitPropertyImplementation()
        {
            const string name = "AsmResolver.Workspaces.DotNet.TestCases.IMyInterface.ExplicitP";

            var module = _fixture.Assembly.ManifestModule;
            var implementationProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Properties.First(m => m.Name == name);
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(IMyInterface).MetadataToken))
                .Properties.First(m => m.Name == nameof(IMyInterface.ExplicitP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationProperty));
        }

        [Fact]
        public void ImplicitPropertyImplementation()
        {
            var module = _fixture.Assembly.ManifestModule;
            var implementationProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClass.ImplicitP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(IMyInterface).MetadataToken))
                .Properties.First(m => m.Name == nameof(IMyInterface.ImplicitP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationProperty));
        }

        [Fact]
        public void ImplicitPropertyOverride()
        {
            var module = _fixture.Assembly.ManifestModule;
            var overrideProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClass).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyDerivedClass.ImplicitP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClass.ImplicitP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(overrideProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationProperty));
        }

        [Fact]
        public void PropertyShadowingShouldNotLinkPropertiesTogether()
        {
            var module = _fixture.Assembly.ManifestModule;
            var shadowedProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClass).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyDerivedClass.ShadowedP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClass.ShadowedP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(shadowedProperty);
            Assert.DoesNotContain(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationProperty));
        }

        [Fact]
        public void ImplicitPropertyImplementationThatIsShadowedInDerivedType()
        {
            var module = _fixture.Assembly.ManifestModule;
            var implementationProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyClass).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClass.ShadowedP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(IMyInterface).MetadataToken))
                .Properties.First(m => m.Name == nameof(IMyInterface.ShadowedP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationProperty));
        }
    }
}
