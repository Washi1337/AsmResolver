using AsmResolver.Workspaces.DotNet.TestCases;
using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class TestCasesFixture
    {
        public TestCasesFixture()
        {
            Assembly = AssemblyDefinition.FromFile(typeof(MyClass).Assembly.Location);
        }

        public AssemblyDefinition Assembly
        {
            get;
        }
    }
}
