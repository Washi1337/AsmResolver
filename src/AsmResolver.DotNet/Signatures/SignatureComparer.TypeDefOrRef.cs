using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer :
        IEqualityComparer<ITypeDescriptor>,
        IEqualityComparer<ITypeDefOrRef>,
        IEqualityComparer<TypeDefinition>,
        IEqualityComparer<TypeReference>,
        IEqualityComparer<TypeSpecification>,
        IEqualityComparer<ExportedType>,
        IEqualityComparer<InvalidTypeDefOrRef>
    {
        /// <inheritdoc />
        public virtual bool Equals(ITypeDescriptor? x, ITypeDescriptor? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            x = Reduce(x);
            y = Reduce(y);

            return (x, y) switch
            {
                (null, null) => true,
                (TypeSignature ts1, TypeSignature ts2) => SignatureEquals(ts1, ts2),
                (InvalidTypeDefOrRef i1, InvalidTypeDefOrRef i2) => i1.Error == i2.Error,
                (ITypeDefOrRef or ExportedType or CorLibTypeSignature, ITypeDefOrRef or ExportedType or CorLibTypeSignature) => SimpleTypeEquals(x, y),
                _ => false,
            };

            static ITypeDescriptor? Reduce(ITypeDescriptor? desc)
            {
                if (desc is TypeDefOrRefSignature tdors)
                    desc = tdors.Type;

                if (desc is TypeSpecification ts)
                {
                    if (ts.Signature is TypeDefOrRefSignature tdors2)
                        desc = tdors2.Type;
                    else
                        desc = ts.Signature;
                }

                if ((desc?.ContextModule ?? desc?.Scope?.ContextModule)?.CorLibTypeFactory.FromType(desc!) is { } corLibType)
                    desc = corLibType;

                return desc;
            }
        }

        /// <inheritdoc />
        public virtual int GetHashCode(ITypeDescriptor obj) => obj switch
        {
            InvalidTypeDefOrRef invalidType => GetHashCode(invalidType),
            ITypeDefOrRef typeDefOrRef => GetHashCode(typeDefOrRef),
            TypeSignature signature => GetHashCode(signature),
            _ => SimpleTypeHashCode(obj)
        };

        /// <summary>
        /// Gets the hashcode of a <see cref="ITypeDescriptor"/> without specializing for unusual kinds of descriptors.
        /// </summary>
        /// <remarks>
        /// This is called by <see cref="GetHashCode(ITypeDefOrRef)"/> for non-<see cref="TypeSignature"/>s. Overrides of
        /// this method should, as much as possible, compare <paramref name="obj"/> directly.
        /// </remarks>
        /// <param name="obj">The <see cref="ITypeDescriptor"/> to get the hashcode of</param>
        /// <returns>The computed hashcode</returns>
        protected virtual int SimpleTypeHashCode(ITypeDescriptor obj)
        {
            unchecked
            {
                int hashCode = obj.Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.Namespace?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.DeclaringType is null ? 0 : GetHashCode(obj.DeclaringType));
                return hashCode;
            }
        }

        /// <summary>
        /// Compares two <see cref="ITypeDescriptor"/>s without specializing for unusual kinds of descriptors.
        /// </summary>
        /// <remarks>
        /// This is called by <see cref="Equals(ITypeDefOrRef, ITypeDefOrRef)"/> for non-<see cref="TypeSignature"/>s. Overrides of
        /// this method should, as much as possible, compare <paramref name="x"/> and <paramref name="y"/> directly.
        /// </remarks>
        /// <param name="x">The first <see cref="ITypeDescriptor"/> to compare</param>
        /// <param name="y">The second <see cref="ITypeDescriptor"/> to compare</param>
        /// <returns><see langword="true"/> if <paramref name="x"/> and <paramref name="y"/> are equal; <see langword="false"/> otherwise</returns>
        protected virtual bool SimpleTypeEquals(ITypeDescriptor x, ITypeDescriptor y)
        {
            // Check the basic properties first.
            if (!x.IsTypeOf(y.Namespace, y.Name))
                return false;

            // If scope matches, it is a perfect match.
            if (Equals(x.Scope, y.Scope))
                return true;

            // It can still be an exported type, we need to resolve the type then and check if the definitions match.
            return x.Resolve() is { } definition1
                   && y.Resolve() is { } definition2
                   && Equals(definition1.DeclaringModule!.Assembly, definition2.DeclaringModule!.Assembly)
                   && Equals(definition1.DeclaringType, definition2.DeclaringType);
        }

        /// <inheritdoc />
        public virtual bool Equals(ITypeDefOrRef? x, ITypeDefOrRef? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public virtual int GetHashCode(ITypeDefOrRef obj) => obj.MetadataToken.Table == TableIndex.TypeSpec
            ? GetHashCode((TypeSpecification) obj)
            : SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public virtual bool Equals(TypeDefinition? x, TypeDefinition? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public virtual int GetHashCode(TypeDefinition obj) => SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public virtual bool Equals(TypeReference? x, TypeReference? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public virtual int GetHashCode(TypeReference obj) => SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public virtual bool Equals(TypeSpecification? x, TypeSpecification? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return SignatureEquals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(TypeSpecification obj) => obj.Signature is not null ? GetHashCode(obj.Signature) : 0;

        /// <inheritdoc />
        public virtual bool Equals(ExportedType? x, ExportedType? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return Equals((ITypeDescriptor) x, y);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(ExportedType obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public virtual bool Equals(InvalidTypeDefOrRef? x, InvalidTypeDefOrRef? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Error == y.Error;
        }

        /// <inheritdoc />
        public virtual int GetHashCode(InvalidTypeDefOrRef obj) => (int) obj.Error;
    }
}
