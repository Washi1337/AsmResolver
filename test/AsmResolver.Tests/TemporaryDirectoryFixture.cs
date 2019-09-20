using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.Tests
{
    public class TemporaryDirectoryFixture : IDisposable
    {
        public TemporaryDirectoryFixture()
        {
            BasePath = Path.Combine(Path.GetTempPath(), "AsmResolver.Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(BasePath);
        }

        public string BasePath
        {
            get;
        }

        public string GetTestDirectory(string testClass, string testName)
        {
            string path = Path.Combine(BasePath, testClass, testName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public string GetTestExecutable(string testClass, string testMethod, string fileName)
        {
            return Path.Combine(GetTestDirectory(testClass, testMethod), fileName + ".exe");
        }

        public void RebuildAndRunExe(string testClass, string testMethod, 
            PEFile peFile, 
            string fileName, 
            string expectedOutput,
            int timeout = 5000)
        {
            string fullName = GetTestExecutable(testClass, testMethod, fileName);

            using (var fs = File.Create(fullName))
            {
                peFile.Write(new BinaryStreamWriter(fs));
            }

            RunExe(fullName, expectedOutput, timeout);
        }

        public void RunExe(string filePath, string expectedOutput, int timeout=5000)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
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

            string output = process.StandardOutput.ReadToEnd();
            Assert.Equal(expectedOutput, output);
        }

        public void Dispose()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Directory.Delete(BasePath, true);
                    return;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }
        
    }
}