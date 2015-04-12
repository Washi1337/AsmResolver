using System;
using System.Reflection;

namespace AsmResolver.Net.Metadata
{
    public sealed class ReflectionAssemblyNameWrapper : IAssemblyDescriptor
    {
        private readonly AssemblyName _assemblyName;

        public ReflectionAssemblyNameWrapper(AssemblyName assemblyName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");
            _assemblyName = assemblyName;
        }

        public string Name
        {
            get { return _assemblyName.Name; }
        }

        public string FullName
        {
            get { return _assemblyName.FullName; }
        }

        public string Culture
        {
            get
            {
                return string.IsNullOrEmpty(_assemblyName.CultureName)
                    ? "neutral"
                    : _assemblyName.CultureName;
            }
        }

        public Version Version
        {
            get { return _assemblyName.Version; }
        }

        public byte[] PublicKeyToken
        {
            get { return _assemblyName.GetPublicKeyToken(); }
        }

        public IMetadataMember Resolve()
        {
            throw new NotSupportedException();
        }
    }
}