using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AsmResolver.Tests.Runners
{
    public class TemporaryDirectoryFixture : IDisposable
    {
        private readonly IList<PERunner> _runners;
        
        public TemporaryDirectoryFixture()
        {
            BasePath = Path.Combine(Path.GetTempPath(), "AsmResolver.Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(BasePath);

            _runners = new PERunner[]
            {
                new NativePERunner(BasePath),
                new FrameworkPERunner(BasePath),
                new DotNetPERunner(BasePath), 
            };
        }

        public string BasePath
        {
            get;
        }

        public TRunner GetRunner<TRunner>()
        {
            return _runners.OfType<TRunner>().First();
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