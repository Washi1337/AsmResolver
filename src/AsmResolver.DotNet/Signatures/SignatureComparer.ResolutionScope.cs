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

        /// <summary>
        /// Gets or sets a value indicating whether the containing assembly of the second member to compare is
        /// allowed to be a newer version than the containing assembly of the first member.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is set to <c>true</c>, then any member reference that is contained in a certain
        /// assembly (e.g. with version 1.0.0.0), will be considered equal to a member reference with the
        /// same name or signature contained in an assembly with a newer version (e.g. 1.1.0.0). When this
        /// property is set to <c>false</c>, the exact version number must match instead.
        /// </para>
        /// <para>
        /// This property is ignored if <see cref="IgnoreAssemblyVersionNumbers"/> is <c>true</c>.
        /// </para>
        /// </remarks>
        public bool AcceptNewerAssemblyVersionNumbers
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

            bool versionMatch;
            if (IgnoreAssemblyVersionNumbers)
                versionMatch = true;
            else if (AcceptNewerAssemblyVersionNumbers)
                versionMatch = x.Version < y.Version;
            else
                versionMatch = x.Version == y.Version;

            return versionMatch
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
