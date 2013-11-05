using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.NET
{
    public delegate Win32Assembly AssemblyResolverEventHandler(object sender, AssemblyReference reference);

    public class AssemblyResolver : ICacheProvider
    {
        private static string _frameworkDirectory;
        private static string _gacDirectory;

        public AssemblyResolverEventHandler ResolutionFailed;

        private Dictionary<string, Win32Assembly> _assemblyCache = new Dictionary<string, Win32Assembly>();

        static AssemblyResolver()
        {
            _frameworkDirectory = Path.GetDirectoryName(typeof(void).Assembly.Location);
            _gacDirectory = Path.Combine(
                Path.GetDirectoryName(Path.GetDirectoryName(_frameworkDirectory)), // Get Microsoft.NET root directory
                "assembly");
        }

        public AssemblyResolver()
        {
            SearchDirectories = new List<string>() 
            { 
                _frameworkDirectory 
            };
        }

        public List<string> SearchDirectories
        {
            get;
            private set; 
        }

        public virtual Win32Assembly Resolve(AssemblyReference reference)
        {
            Win32Assembly resolvedAssembly = null;
            string name = reference.Name;

            if (reference.HasImage && !string.IsNullOrEmpty(reference.NETHeader.ParentAssembly.Path))
            {
                // Check directory of container assembly.
                TryGetAssembly(Path.GetDirectoryName(reference.NETHeader.ParentAssembly.Path), name, out resolvedAssembly);
            }

            if (resolvedAssembly == null)
            {
                // Check gac directories.
                if (!TryGetAssemblyGac(Path.Combine(_gacDirectory, "GAC_64"), name, out resolvedAssembly))
                    if (!TryGetAssemblyGac(Path.Combine(_gacDirectory, "GAC_32"), name, out resolvedAssembly))
                        TryGetAssemblyGac(Path.Combine(_gacDirectory, "GAC_MSIL"), name, out resolvedAssembly);
            }

            if (resolvedAssembly == null)
            {
                // Check search directories
                foreach (var directory in SearchDirectories)
                {
                    if (TryGetAssembly(directory, name, out resolvedAssembly))
                        break;
                }
            }

            if (resolvedAssembly == null)
                OnResolutionFailed(reference);

            if (resolvedAssembly != null)
                _assemblyCache.Add(reference.Name, resolvedAssembly);

            return resolvedAssembly;
        }

        private bool TryGetAssemblyGac(string gacDirectory, string name, out Win32Assembly resolvedAssembly)
        {
            resolvedAssembly = null;
            string folder = Path.Combine(gacDirectory, name);
            if (Directory.Exists(folder))
            {
                return TryGetAssembly(Directory.GetDirectories(folder)[0], name, out resolvedAssembly);
            }
            return false;
        }

        private bool TryGetAssembly(string directory, string name, out Win32Assembly resolvedAssembly)
        {
            resolvedAssembly = null;
            var extensions = new string[] { ".exe", ".dll" };
            foreach (var extension in extensions)
            {
                string file = Path.Combine(directory, name + extension);

                if (_assemblyCache.TryGetValue(file, out resolvedAssembly))
                    return true;

                if (File.Exists(file))
                {
                    return TryReadAssembly(file, out resolvedAssembly);
                }
            }

            return false;
        }

        private bool TryReadAssembly(string file, out Win32Assembly assembly)
        {
            assembly = null;
            try
            {
                assembly = Win32Assembly.LoadFile(file);
                _assemblyCache.Add(file, assembly);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        protected virtual void OnResolutionFailed(AssemblyReference reference)
        {
            if (ResolutionFailed != null)
                ResolutionFailed(this, reference);
        }

        public void ClearCache()
        {
            _assemblyCache.Clear();
        }

        public void LoadCache()
        {
        }
    }
}
