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

        public AssemblyResolverEventHandler ResolutionFailed;

        private List<string> _searchDirectories = new List<string>();
        private Dictionary<AssemblyReference, Win32Assembly> _assemblyCache = new Dictionary<AssemblyReference, Win32Assembly>();

        static AssemblyResolver()
        {
            _frameworkDirectory = Path.GetDirectoryName(typeof(void).Assembly.Location);
        }

        public AssemblyResolver()
        {
            _searchDirectories.Add(_frameworkDirectory);
        }

        public virtual Win32Assembly Resolve(AssemblyReference reference)
        {
            Win32Assembly resolvedAssembly = null;
            string[] extensions = new string[] {".exe", ".dll"};
            foreach (var directory in _searchDirectories)
            {
                foreach (var extension in extensions)
                {
                    string file = Path.Combine(directory, reference.Name + extension);
                    if (File.Exists(file))
                    {
                        try
                        {
                            if (TryReadAssembly(reference, file, out resolvedAssembly))
                                return resolvedAssembly;
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (resolvedAssembly == null)
                OnResolutionFailed(reference);

            if (resolvedAssembly != null)
                _assemblyCache.Add(reference, resolvedAssembly);

            return resolvedAssembly;
        }

        private bool TryReadAssembly(AssemblyReference reference, string file, out Win32Assembly assembly)
        {
            assembly = null;
            try
            {
                assembly = Win32Assembly.LoadFile(file);
                _assemblyCache.Add(reference, assembly);
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
