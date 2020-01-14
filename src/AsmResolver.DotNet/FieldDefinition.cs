using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single field in a type definition of a .NET module.
    /// </summary>
    public class FieldDefinition : IFieldDescriptor, IHasCustomAttribute, IOwnedCollectionElement<TypeDefinition>
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<FieldSignature> _signature;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private IList<CustomAttribute> _customAttributes;

        /// <summary>
        /// Initializes a new field definition.
        /// </summary>
        /// <param name="token">The token of the field.</param>
        protected FieldDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _signature = new LazyVariable<FieldSignature>(GetSignature);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
        }

        /// <summary>
        /// Creates a new field definition.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="signature">The signature of the field.</param>
        /// <remarks>
        /// For a valid .NET image, if <see cref="CallingConventionSignature.HasThis"/> of the signature referenced by
        /// <paramref name="signature"/> is set, the <see cref="FieldAttributes.Static"/> bit should be unset in
        /// <paramref name="attributes"/> and vice versa.
        /// </remarks>
        public FieldDefinition(string name, FieldAttributes attributes, FieldSignature signature)
            : this(new MetadataToken(TableIndex.Field, 0))
        {
            Name = name;
            Attributes = attributes;
            Signature = signature;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the signature of the field. This includes the field type.
        /// </summary>
        public FieldSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public string FullName => FullNameGenerator.GetFieldFullName(Name, DeclaringType, Signature);

        /// <summary>
        /// Gets or sets the attributes associated to the field.
        /// </summary>
        public FieldAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is in a private scope.
        /// </summary>
        public bool IsPrivateScope
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.PrivateScope;
            set => Attributes = value ? Attributes & ~FieldAttributes.FieldAccessMask : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked private and can only be accessed by
        /// members within the same enclosing type.
        /// </summary>
        public bool IsPrivate
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Private : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked family and assembly, and can only be accessed by
        /// members within the same enclosing type and any derived type, within the same assembly.
        /// </summary>
        public bool IsFamilyAndAssembly
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamilyAndAssembly;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.FamilyAndAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked private and can only be accessed by
        /// members within the same assembly.
        /// </summary>
        public bool IsAssembly
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Assembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked private and can only be accessed by
        /// members within the same enclosing type, as well as any derived type.
        /// </summary>
        public bool IsFamily
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Family : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked family or assembly, and can only be accessed by
        /// members within the same enclosing type and any derived type, or within the same assembly.
        /// </summary>
        public bool IsFamilyOrAssembly
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamilyOrAssembly;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.FamilyOrAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked public, and can be accessed by
        /// any member having access to the enclosing type.
        /// </summary>
        public bool IsPublic
        {
            get => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
            set => Attributes = (Attributes & ~FieldAttributes.FieldAccessMask)
                                | (value ? FieldAttributes.Public : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field requires an object instance to access it.
        /// </summary>
        /// <remarks>
        /// This property does not reflect the value of <see cref="CallingConventionSignature.HasThis"/>, nor will it
        /// change the value of <see cref="CallingConventionSignature.HasThis"/> if this property is changed. For a
        /// valid .NET image, these values should match, however.
        /// </remarks>
        public bool IsStatic
        {
            get => (Attributes & FieldAttributes.Static) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.Static)
                                | (value ? FieldAttributes.Static : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked init-only, and can only be assigned a value by
        /// a constructor of the enclosing type.
        /// </summary>
        public bool IsInitOnly
        {
            get => (Attributes & FieldAttributes.InitOnly) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.InitOnly)
                                | (value ? FieldAttributes.InitOnly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked as a literal, and its value is decided upon
        /// compile time.
        /// </summary>
        public bool IsLiteral
        {
            get => (Attributes & FieldAttributes.Literal) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.Literal)
                                | (value ? FieldAttributes.Literal : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is marked as not serialized, indicating the field does
        /// not have to be serialized when the enclosing type is remoted.
        /// </summary>
        public bool IsNotSerialized
        {
            get => (Attributes & FieldAttributes.NotSerialized) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.NotSerialized)
                                | (value ? FieldAttributes.NotSerialized : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field uses a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => (Attributes & FieldAttributes.SpecialName) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.SpecialName)
                                | (value ? FieldAttributes.SpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field's implementation is forwarded through Platform Invoke.
        /// </summary>
        public bool IsPInvokeImpl
        {
            get => (Attributes & FieldAttributes.PInvokeImpl) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.PInvokeImpl)
                                | (value ? FieldAttributes.PInvokeImpl : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field uses a name that is used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => (Attributes & FieldAttributes.RuntimeSpecialName) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.RuntimeSpecialName)
                                | (value ? FieldAttributes.RuntimeSpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has marshalling information associated to it.
        /// </summary>
        public bool HasFieldMarshal
        {
            get => (Attributes & FieldAttributes.HasFieldMarshal) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.HasFieldMarshal)
                                | (value ? FieldAttributes.HasFieldMarshal : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has a default value associated to it.
        /// </summary>
        public bool HasDefault
        {
            get => (Attributes & FieldAttributes.HasDefault) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.HasDefault)
                                | (value ? FieldAttributes.HasDefault : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field has an initial value associated to it that is referenced
        /// by a relative virtual address.
        /// </summary>
        public bool HasFieldRva
        {
            get => (Attributes & FieldAttributes.HasFieldRva) != 0;
            set => Attributes = (Attributes & ~FieldAttributes.HasFieldRva)
                                | (value ? FieldAttributes.HasFieldRva : 0);
        }
        
        /// <inheritdoc />
        public ModuleDefinition Module => DeclaringType.Module;

        /// <summary>
        /// Gets the type that defines the field.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            private set => _declaringType.Value = value;
        }

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, GetCustomAttributes(), null);
                return _customAttributes;
            }
        }

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);
        
        /// <summary>
        /// Obtains the name of the field definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the signature of the field definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual FieldSignature GetSignature() => null;

        /// <summary>
        /// Obtains the declaring type of the field definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetDeclaringType() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}