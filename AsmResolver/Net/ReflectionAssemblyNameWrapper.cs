using System;
using System.Reflection;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
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

        public AssemblyAttributes Attributes
        {
            get
            {
                return (AssemblyAttributes) (
                    _assemblyName.Flags
                    | (AssemblyNameFlags) ((int) _assemblyName.ProcessorArchitecture << 4));
            }
        }

        public string Culture
        {
            get
            {
                return string.IsNullOrEmpty(_assemblyName.CultureInfo.Name)
                    ? "neutral"
                    : _assemblyName.CultureInfo.Name;
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