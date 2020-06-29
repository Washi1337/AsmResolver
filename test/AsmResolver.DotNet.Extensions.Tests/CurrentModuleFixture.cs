namespace AsmResolver.DotNet.Tests
{
    public class CurrentModuleFixture
    {
        public CurrentModuleFixture()
        {
            Module = ModuleDefinition.FromFile(typeof(CurrentModuleFixture).Assembly.Location);
        }
        
        public ModuleDefinition Module
        {
            get;
        }
    }
}