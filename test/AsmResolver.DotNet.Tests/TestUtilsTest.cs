using System;
using System.Diagnostics;
using System.IO;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public class TestUtilsTest
{
    [Fact]
    public void RebuildAndRunUsesRequestedFileAndTestDirectory()
    {
        using var directory = new TemporaryDirectoryFixture();
        var runner = new RecordingRunner(directory.BasePath);
        var module = new ModuleDefinition("Rebuilt.dll");

        Assert.Throws<NotSupportedException>(() => runner.RebuildAndRun(
            module, "Rebuilt.dll", "", testClass: "Source.cs", testMethod: "Case"));

        var expectedPath = Path.Combine(directory.BasePath, "Source", "Case", "Rebuilt.dll");
        Assert.Equal(expectedPath, runner.FilePath);
        Assert.True(File.Exists(expectedPath));
    }

    private sealed class RecordingRunner(string basePath) : PERunner(basePath)
    {
        public string? FilePath { get; private set; }
        protected override string ExecutableExtension => ".dll";

        protected override ProcessStartInfo GetStartInfo(string filePath, string[]? arguments)
        {
            FilePath = filePath;
            // Stop after writing the module so this path regression does not require a native runtime.
            throw new NotSupportedException();
        }
    }
}
