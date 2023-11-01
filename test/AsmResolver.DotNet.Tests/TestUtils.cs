using System.IO;
using System.Runtime.CompilerServices;
using AsmResolver.PE.DotNet.Cil;
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
            string actualOutput = runner.RunAndCaptureOutput(path, null, timeout);
            Assert.Equal(expectedOutput.Replace("\r\n", "\n"), actualOutput);
        }

        public static FieldDefinition FindInitializerField(this FieldDefinition field)
        {
            var cctor = field.DeclaringType.GetStaticConstructor();

            var instructions = cctor.CilMethodBody.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode.Code == CilCode.Ldtoken
                    && instructions[i + 2].OpCode.Code == CilCode.Stsfld
                    && instructions[i+2].Operand is FieldDefinition f
                    && f == field)
                {
                    return (FieldDefinition) instructions[i].Operand;
                }
            }

            return null;
        }
    }
}
