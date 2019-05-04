
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a reference to a type defined in an external .NET assembly.
    /// </summary>
    public class TypeReference : MetadataMember<MetadataRow<uint, uint, uint>>, ITypeDefOrRef, IResolutionScope
    {
        private readonly LazyValue<IResolutionScope> _resolutionScope;
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _namespace;
        private string _fullName;
        private MetadataImage _image;

        public TypeReference(IResolutionScope resolutionScope, string @namespace, string name)
            : base(new MetadataToken(MetadataTokenType.TypeRef))
        {
            _resolutionScope = new LazyValue<IResolutionScope>(resolutionScope);
            _namespace = new LazyValue<string>(@namespace);
            _name = new LazyValue<string>(name);

            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal TypeReference(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var stringStream = image.Header.GetStream<StringStream>();
            var tableStream = image.Header.GetStream<TableStream>();

            _resolutionScope = new LazyValue<IResolutionScope>(() =>
            {
                var resolutionScopeToken = tableStream.GetIndexEncoder(CodedIndex.ResolutionScope).DecodeIndex(row.Column1);
                return image.TryResolveMember(resolutionScopeToken, out var resolutionScope)
                    ? resolutionScope as IResolutionScope
                    : null;
            });

            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column2));
            _namespace = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column3));
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image => _resolutionScope.IsInitialized && _resolutionScope.Value != null 
            ? _resolutionScope.Value.Image 
            : _image;

        /// <summary>
        /// Gets the type that declares the type reference (if any).
        /// </summary>
        public ITypeDefOrRef DeclaringType => ResolutionScope as ITypeDefOrRef;

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor => DeclaringType;

        /// <summary>
        /// Gets the scope the referenced type is defined in.
        /// </summary>
        public IResolutionScope ResolutionScope
        {
            get => _resolutionScope.Value;
            set
            {
                _resolutionScope.Value = value;
                _image = null;
            }
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <summary>
        /// Gets or sets the namespace of the referenced type. 
        /// </summary>
        public string Namespace
        {
            get => _namespace.Value;
            set
            {
                _namespace.Value = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public virtual string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                if (DeclaringType != null)
                    return _fullName = DeclaringType.FullName + '+' + Name;
                return _fullName = string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the type is a value type or not.
        /// </summary>
        /// <remarks>
        /// This property tries to resolve the type to determine whether it is a value type or not.
        /// </remarks>
        public bool IsValueType
        {
            get
            {
                var definition = Resolve();
                return definition != null && definition.IsValueType;
            }
        }

        /// <inheritdoc />
        public ITypeDescriptor GetElementType()
        {
            return this;
        }

        /// <inheritdoc />
        public TypeSignature ToTypeSignature()
        {
            var corlibType = Image?.TypeSystem.GetMscorlibType(this);
            if (corlibType != null)
                return corlibType;

            return new TypeDefOrRefSignature(this)
            {
                IsValueType = IsValueType
            };
        }

        /// <inheritdoc />
        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef()
        {
            return this;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Resolves the type reference to its definition.
        /// </summary>
        /// <returns>The type definition.</returns>
        /// <exception cref="MemberResolutionException">Occurs when the resolution fails.</exception>
        public TypeDefinition Resolve()
        {
            if (Image?.MetadataResolver == null)
                throw new MemberResolutionException(this);
            return Image.MetadataResolver.ResolveType(this);
        }

        IMetadataMember IResolvable.Resolve()
        {
            return Resolve();
        }
    }
}
