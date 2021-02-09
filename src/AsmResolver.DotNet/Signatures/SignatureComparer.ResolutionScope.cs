using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer :
        IEqualityComparer<IResolutionScope>,
        IEqualityComparer<AssemblyDescriptor>
    {
        /// <summary>
        /// Gets or sets a value indicating whether version numbers should be excluded in the comparison of two
        /// assembly descriptors.
        /// </summary>
        public bool IgnoreAssemblyVersionNumbers
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        public bool Equals(IResolutionScope x, IResolutionScope y)
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
            ModuleDefinition module => GetHashCode(module.Assembly),
            TypeReference typeRef => GetHashCode(typeRef),
            _ => throw new ArgumentOutOfRangeException(nameof(obj))
        };

        /// <inheritdoc />
        public bool Equals(AssemblyDescriptor x, AssemblyDescriptor y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
                
            return (IgnoreAssemblyVersionNumbers || x.Version == y.Version)
                    && x.Name == y.Name
                    && x.Culture == y.Culture
                    && Equals(x.GetPublicKeyToken(), y.GetPublicKeyToken());
        }

        /// <inheritdoc />
        public int GetHashCode(AssemblyDescriptor obj)
        {
            unchecked
            {
                int hashCode = (obj.Name != null ? obj.Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Culture != null ? obj.Culture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) TableIndex.AssemblyRef;
                hashCode = (hashCode * 397) ^ (obj.Version != null ? obj.Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) obj.Attributes;
                var publicKeyToken = obj.GetPublicKeyToken();
                hashCode = (hashCode * 397) ^ (publicKeyToken != null ? GetHashCode(publicKeyToken) : 0);
                return hashCode;
            }
        }
    }
}