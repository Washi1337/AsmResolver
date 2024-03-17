using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer :
        IEqualityComparer<IResolutionScope>,
        IEqualityComparer<AssemblyDescriptor>,
        IEqualityComparer<AssemblyReference>
    {
        private bool IgnoreAssemblyVersionNumbers
        {
            get
            {
                return (Flags & SignatureComparisonFlags.VersionAgnostic) == SignatureComparisonFlags.VersionAgnostic;
            }
        }

        private bool AcceptNewerAssemblyVersionNumbers
        {
            get
            {
                return (Flags & SignatureComparisonFlags.AcceptNewerVersions) == SignatureComparisonFlags.AcceptNewerVersions;
            }
        }

        private bool AcceptOlderAssemblyVersionNumbers
        {
            get
            {
                return (Flags & SignatureComparisonFlags.AcceptOlderVersions) == SignatureComparisonFlags.AcceptOlderVersions;
            }
        }

        /// <inheritdoc />
        public bool Equals(IResolutionScope? x, IResolutionScope? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return (x, y) switch
            {
                (AssemblyDescriptor xx, AssemblyDescriptor yy) => Equals(xx, yy),
                (ModuleDefinition xx, AssemblyDescriptor yy) => Equals(xx.Assembly, yy),
                (AssemblyDescriptor xx, ModuleDefinition yy) => Equals(xx, yy.Assembly),
                (ModuleDefinition xx, ModuleDefinition yy) => Equals(xx.Assembly, yy.Assembly),
                (TypeReference xx, TypeReference yy) => Equals(xx, yy),
                _ => false
            };
        }

        /// <inheritdoc />
        public int GetHashCode(IResolutionScope obj) => obj switch
        {
            AssemblyDescriptor assembly => GetHashCode(assembly),
            ModuleDefinition module => module.Assembly is not null ? GetHashCode(module.Assembly) : 0,
            TypeReference typeRef => GetHashCode(typeRef),
            _ => throw new ArgumentOutOfRangeException(nameof(obj))
        };

        /// <inheritdoc />
        public bool Equals(AssemblyDescriptor? x, AssemblyDescriptor? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            bool versionMatch;
            if (IgnoreAssemblyVersionNumbers)
                versionMatch = true;
            else if (AcceptNewerAssemblyVersionNumbers)
                versionMatch = x.Version <= y.Version;
            else if (AcceptOlderAssemblyVersionNumbers)
                versionMatch = x.Version >= y.Version;
            else
                versionMatch = x.Version == y.Version;

            return versionMatch
                   && x.Name == y.Name
                   && (x.Culture == y.Culture || (Utf8String.IsNullOrEmpty(x.Culture) && Utf8String.IsNullOrEmpty(y.Culture)))
                   && Equals(x.GetPublicKeyToken(), y.GetPublicKeyToken());
        }

        /// <inheritdoc />
        public int GetHashCode(AssemblyDescriptor obj)
        {
            unchecked
            {
                int hashCode = obj.Name is null ? 0 : obj.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (!Utf8String.IsNullOrEmpty(obj.Culture) ? obj.Culture.GetHashCode() : 0);

                if (!AcceptNewerAssemblyVersionNumbers && !AcceptOlderAssemblyVersionNumbers)
                    hashCode = (hashCode * 397) ^ obj.Version.GetHashCode();

                byte[]? publicKeyToken = obj.GetPublicKeyToken();
                hashCode = (hashCode * 397) ^ (publicKeyToken is not null ? GetHashCode(publicKeyToken) : 0);

                return hashCode;
            }
        }

        /// <inheritdoc />
        public bool Equals(AssemblyReference x, AssemblyReference y) => Equals((AssemblyDescriptor)x, y);

        /// <inheritdoc />
        public int GetHashCode(AssemblyReference obj) => GetHashCode((AssemblyDescriptor)obj);
    }
}
