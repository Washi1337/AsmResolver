using System;
using System.IO;
using System.Linq;
using System.Threading;

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