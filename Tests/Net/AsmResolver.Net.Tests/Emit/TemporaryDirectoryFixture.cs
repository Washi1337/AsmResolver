using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using AsmResolver.Net.Emit;
using Xunit;

namespace AsmResolver.Tests.Net.Emit
{
    public class TemporaryDirectoryFixture : IDisposable
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
        public TemporaryDirectoryFixture()
        {
            TemporaryDirectory = GenerateRandomName();
            Directory.CreateDirectory(TemporaryDirectory);
        }

        public string TemporaryDirectory
        {
            get;
            private set;
        }

        private static string GenerateRandomName()
        {
            return string.Concat(Alphabet.OrderBy(x => Guid.NewGuid()).Take(15));
        }

        public string GenerateRandomFileName()
        {
            return Path.Combine(TemporaryDirectory, GenerateRandomName() + ".exe");
        }
        
        public void VerifyOutput(WindowsAssembly assembly, string expectedOutput)
        {
            string path =  GenerateRandomFileName();
            assembly.Write(path, new CompactNetAssemblyBuilder(assembly));

            string contents = null;
            string error = null;
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    ErrorDialog = false
                }
            })
            {
                process.Start();

                contents= process.StandardOutput.ReadToEnd().Trim();
                error = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }
            
            Assert.Empty(error);                
            Assert.Equal(expectedOutput, contents);
        }
        
        public void Dispose()
        {
            try
            {
                Directory.Delete(TemporaryDirectory, true);
            }
            catch (AccessViolationException)
            {
                Thread.Sleep(1000);
                Directory.Delete(TemporaryDirectory, true);
            }
        }
    }
}