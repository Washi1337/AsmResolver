namespace AsmResolver.DotNet.Tests
{
    public class CurrentModuleFixture
    {
        public CurrentModuleFixture()
        {
            Module = ModuleDefinition.FromFile(typeof(CurrentModuleFixture).Assembly.Location, TestReaderParameters);
        }

        public ModuleDefinition Module
        {
            get;
        }
    }
}
