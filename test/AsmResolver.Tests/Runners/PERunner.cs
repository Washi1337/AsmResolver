using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.Tests.Runners
{
    public abstract class PERunner
    {
        protected PERunner(string basePath)
        {
            BasePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }
        
        public string BasePath
        {
            get;
        }

        protected abstract string ExecutableExtension
        {
            get;
        }

        public void RebuildAndRun(PEFile peFile, string fileName, string expectedOutput, int timeout = 5000,
            [CallerFilePath] string testClass = "File",
            [CallerMemberName] string testMethod = "Test")
        {
            string fullPath = Rebuild(peFile, fileName, testClass, testMethod);
            string actualOutput = RunAndCaptureOutput(fullPath, timeout);
            Assert.Equal(expectedOutput, actualOutput);
        }

        protected string GetTestDirectory(string testClass, string testName)
        {
            string path = Path.Combine(BasePath, testClass, testName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public string GetTestExecutablePath(string testClass, string testMethod, string fileName)
        {
            return Path.ChangeExtension(Path.Combine(GetTestDirectory(testClass, testMethod), fileName), ExecutableExtension);
        }

        public string Rebuild(PEFile peFile, string fileName, string testClass, string testMethod)
        {
            testClass = Path.GetFileNameWithoutExtension(testClass);
            string fullPath = GetTestExecutablePath(testClass, testMethod, fileName);

            using var fileStream = File.Create(fullPath);
            peFile.Write(new BinaryStreamWriter(fileStream));

            return fullPath;
        }

        private string RunAndCaptureOutput(string filePath, int timeout=5000)
        {
            var process = new Process()
            {
                StartInfo = GetStartInfo(filePath)
            };

            process.Start();

            if (!process.WaitForExit(timeout))
            {
                try
                {
                    process.Kill();
                }
                catch (InvalidOperationException)
                {
                    // Process has already exited.
                }
                
                throw new TimeoutException();
            }

            return process.StandardOutput.ReadToEnd();
        }

        protected abstract ProcessStartInfo GetStartInfo(string filePath);
    }
}