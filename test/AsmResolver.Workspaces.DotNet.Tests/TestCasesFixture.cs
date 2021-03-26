using System.Collections.Generic;
using AsmResolver.Workspaces.DotNet.TestCases;
using AsmResolver.DotNet;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.MultiModules.ManifestModule;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.DotNet.TestCases.Properties;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class TestCasesFixture
    {
        public TestCasesFixture()
        {
            WorkspacesAssembly = AssemblyDefinition.FromFile(typeof(MyClass).Assembly.Location);
            CustomAttributesAssembly = AssemblyDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            EventsAssembly = AssemblyDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            FieldsAssembly = AssemblyDefinition.FromFile(typeof(NoFields).Assembly.Location);
            GenericsAssembly = AssemblyDefinition.FromFile(typeof(GenericsTestClass).Assembly.Location);
            MethodsAssembly = AssemblyDefinition.FromFile(typeof(MethodBodyTypes).Assembly.Location);
            NestedClassesAssembly = AssemblyDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);
            PropertiesAssembly = AssemblyDefinition.FromFile(typeof(NoProperties).Assembly.Location);
            ResourcesAssembly = AssemblyDefinition.FromFile(typeof(AsmResolver.DotNet.TestCases.Resources.Resources).Assembly.Location);
            TypesAssembly = AssemblyDefinition.FromFile(typeof(MyClass).Assembly.Location);
            MultiModuleAssembly = AssemblyDefinition.FromFile(typeof(Manifest).Assembly.Location);

            HelloWorld_Forwarder = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld_Forwarder);
            Assembly1_Forwarder = AssemblyDefinition.FromBytes(Properties.Resources.Assembly1_Forwarder);
            Assembly2_Actual = AssemblyDefinition.FromBytes(Properties.Resources.Assembly2_Actual);

            AllAssemblies = new[]
            {
                WorkspacesAssembly,
                CustomAttributesAssembly,
                EventsAssembly,
                FieldsAssembly,
                GenericsAssembly,
                MethodsAssembly,
                NestedClassesAssembly,
                PropertiesAssembly,
                ResourcesAssembly,
                TypesAssembly,
                MultiModuleAssembly,
                HelloWorld_Forwarder,
                Assembly1_Forwarder,
                Assembly2_Actual
            };
        }

        public AssemblyDefinition WorkspacesAssembly
        {
            get;
        }

        public AssemblyDefinition CustomAttributesAssembly
        {
            get;
        }

        public AssemblyDefinition EventsAssembly
        {
            get;
        }

        public AssemblyDefinition FieldsAssembly
        {
            get;
        }

        public AssemblyDefinition GenericsAssembly
        {
            get;
        }

        public AssemblyDefinition MethodsAssembly
        {
            get;
        }
        public AssemblyDefinition NestedClassesAssembly
        {
            get;
        }
        public AssemblyDefinition PropertiesAssembly
        {
            get;
        }

        public AssemblyDefinition ResourcesAssembly
        {
            get;
        }

        public AssemblyDefinition TypesAssembly
        {
            get;
        }

        public AssemblyDefinition MultiModuleAssembly
        {
            get;
        }

        public IReadOnlyCollection<AssemblyDefinition> AllAssemblies
        {
            get;
        }

        public AssemblyDefinition HelloWorld_Forwarder
        {
            get;
        }

        public AssemblyDefinition Assembly1_Forwarder
        {
            get;
        }

        public AssemblyDefinition Assembly2_Actual
        {
            get;
        }
    }
}
