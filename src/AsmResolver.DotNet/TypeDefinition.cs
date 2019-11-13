using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type (a class, interface or structure) defined in a .NET module.
    /// </summary>
    public class TypeDefinition : ITypeDefOrRef, 
        IOwnedCollectionElement<ModuleDefinition>,
        IOwnedCollectionElement<TypeDefinition>
    {
        private readonly LazyVariable<string> _namespace;
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<ITypeDefOrRef> _baseType;
        private IList<TypeDefinition> _nestedTypes;
        private string _fullName;
        
        /// <summary>
        /// Initializes a new type definition.
        /// </summary>
        /// <param name="token">The token of the type definition.</param>
        protected TypeDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _namespace = new LazyVariable<string>(GetNamespace);
            _name = new LazyVariable<string>(GetName);
            _baseType = new LazyVariable<ITypeDefOrRef>(GetBaseType);
        }

        /// <summary>
        /// Creates a new type definition.
        /// </summary>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        /// <param name="attributes">The attributes associated to the type.</param>
        public TypeDefinition(string ns, string name, TypeAttributes attributes)
            : this(ns, name, attributes, null)
        {
        }

        /// <summary>
        /// Creates a new type definition.
        /// </summary>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        /// <param name="attributes">The attributes associated to the type.</param>
        /// <param name="baseType">The super class that this type extends.</param>
        public TypeDefinition(string ns, string name, TypeAttributes attributes, ITypeDefOrRef baseType)
            : this(new MetadataToken(TableIndex.TypeDef, 0))
        {
            Namespace = ns;
            Name = name;
            Attributes = attributes;
            BaseType = baseType;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the namespace the type resides in.
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

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
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
        /// Gets the full name (including namespace or declaring type full name) of the type.
        /// </summary>
        public string FullName => _fullName ?? (_fullName = this.GetFullName());

        /// <summary>
        /// Gets or sets the attributes associated to the type.
        /// </summary>
        public TypeAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the super class that this type extends. 
        /// </summary>
        public ITypeDefOrRef BaseType
        {
            get => _baseType.Value;
            set => _baseType.Value = value;
        }

        /// <summary>
        /// Gets the module that defines the type.
        /// </summary>
        public ModuleDefinition Module
        {
            get;
            private set;
        }

        ModuleDefinition IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
        }

        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get;
            private set;
        }
        
        ITypeDefOrRef ITypeDefOrRef.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        /// <summary>
        /// Gets a collection of nested types that this type defines.
        /// </summary>
        public IList<TypeDefinition> NestedTypes
        {
            get
            {
                if (_nestedTypes is null)
                    Interlocked.CompareExchange(ref _nestedTypes, GetNestedTypes(), null);
                return _nestedTypes;
            }
        }

        IResolutionScope ITypeDefOrRef.Scope => Module;
        
        /// <summary>
        /// Obtains the namespace of the type definition.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Namespace"/> property.
        /// </remarks>
        protected virtual string GetNamespace() => null;

        /// <summary>
        /// Obtains the name of the type definition.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the base type of the type definition.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="BaseType"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef GetBaseType() => null;

        /// <summary>
        /// Obtains the list of nested types that this type defines.
        /// </summary>
        /// <returns>The nested types.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="NestedTypes"/> property.
        /// </remarks>
        protected virtual IList<TypeDefinition> GetNestedTypes() =>
            new OwnedCollection<TypeDefinition, TypeDefinition>(this);

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}