using System.IO;
using System.Runtime.CompilerServices;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public static class TestUtils
    {
        public static void RebuildAndRun(this PERunner runner, ModuleDefinition module,  string fileName, string expectedOutput, int timeout = 5000,
            [CallerFilePath] string testClass = "File",
            [CallerMemberName] string testMethod = "Test")
        {
            testClass = Path.GetFileNameWithoutExtension(testClass);
            string path = runner.GetTestExecutablePath(testClass, testMethod, fileName);
            module.Write(path);
            string actualOutput = runner.RunAndCaptureOutput(path, timeout);
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}