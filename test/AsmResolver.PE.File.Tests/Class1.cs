using Xunit;

namespace AsmResolver.PE.File.Tests
{
    public class Class1
    {
        [Fact]
        public void t()
        {
            var peFile = PEFile.FromFile(typeof(HelloWorld.Program).Assembly.Location);
        }
    }
}