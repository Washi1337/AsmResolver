using System.Linq;
using AsmResolver.Workspaces.DotNet.TestCases;
using AsmResolver.DotNet;
using AsmResolver.Workspaces.DotNet;
using Xunit;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class InheritanceTestGeneric : IClassFixture<TestCasesFixture>
    {
        private readonly TestCasesFixture _fixture;

        public InheritanceTestGeneric(TestCasesFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void InterfaceImplementation()
        {
            var module = _fixture.Assembly.ManifestModule;
            var classType = (TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken);
            var interfaceType = (TypeDefinition) module.LookupMember(typeof(IMyInterfaceGeneric<bool,int,float>).MetadataToken);

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(classType);
            Assert.Contains(interfaceType, 
                node.GetRelatedObjects(DotNetRelations.BaseType)
                    .Select(t=> t.Resolve()));
        }

        [Fact]
        public void DerivedClassRelation()
        {
            var module = _fixture.Assembly.ManifestModule;
            var derivedType = (TypeDefinition) module.LookupMember(typeof(MyDerivedClassGeneric).MetadataToken);
            var classType = (TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken);

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(derivedType);
            Assert.Contains(classType, node.GetRelatedObjects(DotNetRelations.BaseType));
        }

        [Fact]
        public void ExplicitMethodImplementation()
        {
            const string name = "AsmResolver.Workspaces.DotNet.TestCases.IMyInterfaceGeneric<System.Boolean,System.Int32,System.Single>.Explicit";

            var module = _fixture.Assembly.ManifestModule;
            var implementationMethod =
                ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Methods.First(m => m.Name == name);
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(IMyInterfaceGeneric<bool,int,float>).MetadataToken))
                .Methods.First(m => m.Name == nameof(IMyInterfaceGeneric<bool,int,float>.Explicit));

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
                ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClassGeneric.Implicit));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(IMyInterfaceGeneric<bool,int,float>).MetadataToken))
                .Methods.First(m => m.Name == nameof(IMyInterfaceGeneric<bool,int,float>.Implicit));

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
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClassGeneric).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyDerivedClassGeneric.Implicit));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClassGeneric.Implicit));

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
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClassGeneric).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyDerivedClassGeneric.Shadowed));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClassGeneric.Shadowed));

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
                ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Methods.First(m => m.Name == nameof(MyClassGeneric.Shadowed));
            var baseMethod = ((TypeDefinition) module.LookupMember(typeof(IMyInterfaceGeneric<bool,int,float>).MetadataToken))
                .Methods.First(m => m.Name == nameof(IMyInterfaceGeneric<bool,int,float>.Shadowed));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationMethod);
            Assert.Contains(baseMethod, node.GetRelatedObjects(DotNetRelations.ImplementationMethod));
        }
        
        [Fact]
        public void ExplicitPropertyImplementation()
        {
            const string name = "AsmResolver.Workspaces.DotNet.TestCases.IMyInterfaceGeneric<System.Boolean,System.Int32,System.Single>.ExplicitP";

            var module = _fixture.Assembly.ManifestModule;
            var implementationProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Properties.First(m => m.Name == name);
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(IMyInterfaceGeneric<bool,int,float>).MetadataToken))
                .Properties.First(m => m.Name == nameof(IMyInterfaceGeneric<bool,int,float>.ExplicitP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationSemantics));
        }

        [Fact]
        public void ImplicitPropertyImplementation()
        {
            var module = _fixture.Assembly.ManifestModule;
            var implementationProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClassGeneric.ImplicitP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(IMyInterfaceGeneric<bool,int,float>).MetadataToken))
                .Properties.First(m => m.Name == nameof(IMyInterfaceGeneric<bool,int,float>.ImplicitP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationSemantics));
        }

        [Fact]
        public void ImplicitPropertyOverride()
        {
            var module = _fixture.Assembly.ManifestModule;
            var overrideProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClassGeneric).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyDerivedClassGeneric.ImplicitP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClassGeneric.ImplicitP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(overrideProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationSemantics));
        }

        [Fact]
        public void PropertyShadowingShouldNotLinkPropertiesTogether()
        {
            var module = _fixture.Assembly.ManifestModule;
            var shadowedProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyDerivedClassGeneric).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyDerivedClassGeneric.ShadowedP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClassGeneric.ShadowedP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(shadowedProperty);
            Assert.DoesNotContain(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationSemantics));
        }

        [Fact]
        public void ImplicitPropertyImplementationThatIsShadowedInDerivedType()
        {
            var module = _fixture.Assembly.ManifestModule;
            var implementationProperty =
                ((TypeDefinition) module.LookupMember(typeof(MyClassGeneric).MetadataToken))
                .Properties.First(m => m.Name == nameof(MyClassGeneric.ShadowedP));
            var baseProperty = ((TypeDefinition) module.LookupMember(typeof(IMyInterfaceGeneric<bool,int,float>).MetadataToken))
                .Properties.First(m => m.Name == nameof(IMyInterfaceGeneric<bool,int,float>.ShadowedP));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.Assembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(implementationProperty);
            Assert.Contains(baseProperty, node.GetRelatedObjects(DotNetRelations.ImplementationSemantics));
        }
    }
}
