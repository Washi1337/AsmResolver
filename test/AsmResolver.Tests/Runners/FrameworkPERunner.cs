using System;
using System.Diagnostics;

namespace AsmResolver.Tests.Runners
{
    public class FrameworkPERunner : PERunner
    {
        public FrameworkPERunner(string basePath) 
            : base(basePath)
        {
        }

        protected override string ExecutableExtension => ".exe";

        protected override ProcessStartInfo GetStartInfo(string filePath)
        {
            var info = new ProcessStartInfo();
            
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                info.FileName = filePath;
            }
            else
            {
                info.FileName = "mono";
                info.ArgumentList.Add(filePath);
            }

            return info;
        }
    }
    
}