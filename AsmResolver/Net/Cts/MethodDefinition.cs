using System;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;
using AsmResolver.X86;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a method definition defined in the .NET assembly image.
    /// </summary>
    /// <remarks>
    /// Method definitions also include special functions such as constructors, as well as getters and setters for
    /// properties, and adders and removers for custom defined events.
    ///
    /// Method definitions can be managed or native, which is dictated by the <see cref="ImplAttributes"/>
    /// and <see cref="MethodBody"/> properties. A method's body is derived from the raw method body found in the
    /// metadata row, which has been preprocessed by the <see cref="IRawMethodBodyReader"/> specified in the
    /// <see cref="MethodDefinitionTable"/>. By default, it follows the format described by the ECMA and therefore
    /// for "normal" well behaving binaries, this is sufficient. However, when dealing with custom method body formats
    /// (such as encrypted method bodies found in obfuscated binaries, or native method bodies that do not have a clear
    /// structure defined by the CLR itself), it is necessary to implement a custom method body reader to accomodate
    /// for these shortcomings.
    /// </remarks> 
    public class MethodDefinition : MetadataMember<MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint>>, ICallableMemberReference, IHasSecurityAttribute, IMemberForwarded, IGenericParameterProvider, IMemberRefParent, ICustomAttributeType
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<MethodSignature> _signature;
        private readonly LazyValue<TypeDefinition> _declaringType;
        private readonly LazyValue<MethodBody> _methodBody;
        private readonly LazyValue<ImplementationMap> _pinvokeMap;
        
        private string _fullName;
        private MetadataImage _image;

        public MethodDefinition(string name, MethodAttributes attributes, MethodSignature signature)
            : base(new MetadataToken(MetadataTokenType.Method))
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (signature == null)
                throw new ArgumentNullException(nameof(signature));

            _name = new LazyValue<string>(name);
            Attributes = attributes;
            ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            _signature = new LazyValue<MethodSignature>(signature);
            Parameters = new DelegatedMemberCollection<MethodDefinition, ParameterDefinition>(this, GetParamOwner, SetParamOwner);
            _methodBody = new LazyValue<MethodBody>();
            _pinvokeMap = new LazyValue<ImplementationMap>();
            
            _declaringType = new LazyValue<TypeDefinition>();
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            GenericParameters = new GenericParameterCollection(this);
        }

        internal MethodDefinition(MetadataImage image, MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var stringStream = image.Header.GetStream<StringStream>();
            var blobStream = image.Header.GetStream<BlobStream>();

            ImplAttributes = row.Column2;
            Attributes = row.Column3;
            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column4));

            if (blobStream.TryCreateBlobReader(row.Column5, out var blobReader))
                _signature = new LazyValue<MethodSignature>(() => MethodSignature.FromReader(image, blobReader, true));

            _methodBody = new LazyValue<MethodBody>(() =>
            {
                if (row.Column1 is CilRawMethodBody rawBody)
                    return CilMethodBody.FromRawMethodBody(this, rawBody);

                // TODO: handler for native methods.
                return null;
            });
            
            _declaringType = new LazyValue<TypeDefinition>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);
                var typeRow = table.GetRowClosestToKey(5, row.MetadataToken.Rid);
                return (TypeDefinition) table.GetMemberFromRow(image, typeRow);
            });
            
            _pinvokeMap = new LazyValue<ImplementationMap>(() =>
            {
                if (!row.Column3.HasFlag(MethodAttributes.PInvokeImpl)) 
                    return null;
                
                var table = (ImplementationMapTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.ImplMap);
                var mapRow = table.FindImplementationMapOfOwner(row.MetadataToken);
                return mapRow != null ? (ImplementationMap) table.GetMemberFromRow(image, mapRow) : null;
            });

            Parameters = new RangedMemberCollection<MethodDefinition, ParameterDefinition>(this, MetadataTokenType.Param, 5, GetParamOwner, SetParamOwner);
            
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            GenericParameters = new GenericParameterCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image => _declaringType.IsInitialized && _declaringType.Value != null
            ? _declaringType.Value.Image 
            : _image;

        /// <summary>
        /// Gets or sets the attributes that dictate how, where and in what kind of language the method is implemented.
        /// </summary>
        public MethodImplAttributes ImplAttributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the visibility, accessibility and remaining attributes associated to the method.
        /// </summary>
        public MethodAttributes Attributes
        {
            get;
            set;
        }

        /// <inheritdoc cref="IMemberReference.Name" />
        public string Name
        {
            get => _name.Value;
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public string FullName => _fullName ?? (_fullName = this.GetFullName(Signature));

        /// <summary>
        /// Gets the type that declares the method.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            internal set
            {
                _declaringType.Value = value;
                _image = null;
            }
        }

        ITypeDefOrRef IMemberReference.DeclaringType => DeclaringType;

        /// <summary>
        /// Gets or sets the signature defining the parameters and return type of the method. 
        /// </summary>
        public MethodSignature Signature
        {
            get => _signature.Value;
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature => Signature;

        MemberSignature IMethodDefOrRef.Signature => Signature;

        /// <summary>
        /// Gets a collection of parameter definitions that the method defines. These definitions contain the names
        /// and custom attributes associated to the parameters found in the signature.
        /// </summary>
        public Collection<ParameterDefinition> Parameters
        {
            get;
        }

        /// <summary>
        /// Gets or sets the method body of the method.
        /// </summary>
        /// <remarks>
        /// This method can be managed or native. To know which one is used,
        /// query the <see cref="ImplAttributes"/>.
        ///
        /// For CIL method bodies, use the <see cref="CilMethodBody"/> property instead.
        /// </remarks>
        public MethodBody MethodBody
        {
            get => _methodBody.Value;
            set => _methodBody.Value = value;
        }

        /// <summary>
        /// Gets or sets the managed CIL method body of the method (if available).
        /// </summary>
        public CilMethodBody CilMethodBody
        {
            get => MethodBody as CilMethodBody;
            set => MethodBody = value;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <inheritdoc />
        public SecurityDeclarationCollection SecurityDeclarations
        {
            get;
        }

        /// <inheritdoc />
        public GenericParameterCollection GenericParameters
        {
            get;
        }

        /// <summary>
        /// Gets or sets the P/Invoke implementation mapping associated to the method (if available).
        /// </summary>
        /// <remarks>
        /// The property <see cref="HasPInvokeImpl"/> is NOT updated automatically when this property changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public ImplementationMap PInvokeMap
        {
            get => _pinvokeMap.Value;
            set => this.SetPInvokeMap(_pinvokeMap, value);
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the method is private.
        /// </summary>
        public bool IsPrivate
        {
            get => GetMethodAccessAttribute(MethodAttributes.Private);
            set => SetMethodAccessAttribute(MethodAttributes.Private, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the visibility of this method or constructor is described by
        /// FamANDAssem; that is, the method or constructor can be called by derived classes, but only if they are in
        /// the same assembly.
        /// </summary>
        public bool IsFamilyAndAssembly
        {
            get => GetMethodAccessAttribute(MethodAttributes.FamilyAndAssembly);
            set => SetMethodAccessAttribute(MethodAttributes.FamilyAndAssembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the potential visibility of this method or constructor is described
        /// by FamORAssem; that is, the method or constructor can be called by derived classes wherever they are, and by
        /// classes in the same assembly.
        /// </summary>
        public bool IsFamilyOrAssembly
        {
            get => GetMethodAccessAttribute(MethodAttributes.FamilyOrAssembly);
            set => SetMethodAccessAttribute(MethodAttributes.FamilyOrAssembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the potential visibility of this method or constructor is described
        /// by Assembly; that is, the method or constructor is visible at most to other types in the same assembly, and
        /// is not visible to derived types outside the assembly.
        /// </summary>
        public bool IsAssembly
        {
            get => GetMethodAccessAttribute(MethodAttributes.Assembly);
            set => SetMethodAccessAttribute(MethodAttributes.Assembly, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the visibility of this method or constructor is described by Family;
        /// that is, the method or constructor is visible only within its class and derived classes.
        /// </summary>
        public bool IsFamily
        {
            get => GetMethodAccessAttribute(MethodAttributes.Family);
            set => SetMethodAccessAttribute(MethodAttributes.Family, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is public.
        /// </summary>
        public bool IsPublic
        {
            get => GetMethodAccessAttribute(MethodAttributes.Public);
            set => SetMethodAccessAttribute(MethodAttributes.Public, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is static.
        /// </summary>
        public bool IsStatic
        {
            get => Attributes.HasFlag(MethodAttributes.Static);
            set => Attributes = Attributes.SetFlag(MethodAttributes.Static, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is final, that is, it cannot be overridden by any
        /// derived class.
        /// </summary>
        public bool IsFinal
        {
            get => Attributes.HasFlag(MethodAttributes.Final);
            set => Attributes = Attributes.SetFlag(MethodAttributes.Final, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is virtual.
        /// </summary>
        public bool IsVirtual
        {
            get => Attributes.HasFlag(MethodAttributes.Virtual);
            set => Attributes = Attributes.SetFlag(MethodAttributes.Virtual, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only a member of the same kind with exactly the same signature is
        /// hidden in the derived class.
        /// </summary>
        public bool IsHideBySig
        {
            get => Attributes.HasFlag(MethodAttributes.HideBySig);
            set => Attributes = Attributes.SetFlag(MethodAttributes.HideBySig, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is abstract and has no body.
        /// </summary>
        public bool IsAbstract
        {
            get => Attributes.HasFlag(MethodAttributes.Abstract);
            set => Attributes = Attributes.SetFlag(MethodAttributes.Abstract, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this method has a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => Attributes.HasFlag(MethodAttributes.SpecialName);
            set => Attributes = Attributes.SetFlag(MethodAttributes.SpecialName, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this method has a special name that is used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => Attributes.HasFlag(MethodAttributes.RuntimeSpecialName);
            set => Attributes = Attributes.SetFlag(MethodAttributes.RuntimeSpecialName, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this method has security constraints put on it.
        /// </summary>
        public bool HasSecurity
        {
            get => Attributes.HasFlag(MethodAttributes.HasSecurity);
            set => Attributes = Attributes.SetFlag(MethodAttributes.HasSecurity, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this method is mapped to a native procedure through P/Invoke.
        /// </summary>
        /// <remarks>
        /// This property is NOT updated automatically when <see cref="PInvokeMap"/> changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public bool HasPInvokeImpl
        {
            get => Attributes.HasFlag(MethodAttributes.PInvokeImpl);
            set => Attributes = Attributes.SetFlag(MethodAttributes.PInvokeImpl, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the body of the method is written in CIL. 
        /// </summary>
        /// <remarks>
        /// This property is NOT updated automatically when <see cref="MethodBody"/> changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public bool IsIL
        {
            get => GetMethodCodeTypeAttribute(MethodImplAttributes.IL);
            set => SetMethodCodeTypeAttribute(MethodImplAttributes.IL, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the body of the method is written in a native language. 
        /// </summary>
        /// <remarks>
        /// This property is NOT updated automatically when <see cref="MethodBody"/> changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public bool IsNative
        {
            get => GetMethodCodeTypeAttribute(MethodImplAttributes.Native);
            set => SetMethodCodeTypeAttribute(MethodImplAttributes.Native, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the body of the method is written in OPTIL. 
        /// </summary>
        /// <remarks>
        /// This property is NOT updated automatically when <see cref="MethodBody"/> changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public bool IsOPTIL
        {
            get => GetMethodCodeTypeAttribute(MethodImplAttributes.OPTIL);
            set => SetMethodCodeTypeAttribute(MethodImplAttributes.OPTIL, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the body of the method is implemented by the runtime.
        /// </summary>
        /// <remarks>
        /// This property is NOT updated automatically when <see cref="MethodBody"/> changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public bool IsRuntime
        {
            get => GetMethodCodeTypeAttribute(MethodImplAttributes.Runtime);
            set => SetMethodCodeTypeAttribute(MethodImplAttributes.Runtime, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the body of the method is managed.
        /// </summary>
        /// <remarks>
        /// This property is NOT updated automatically when <see cref="MethodBody"/> changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public bool IsManaged
        {
            get => !IsUnmanaged;
            set => IsUnmanaged = !value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the body of the method is unmanaged.
        /// </summary>
        /// <remarks>
        /// This property is NOT updated automatically when <see cref="MethodBody"/> changes.
        /// Make sure to properly set or reset the flag when necessary if a working and fully verifiable assembly is
        /// desired. 
        /// </remarks>
        public bool IsUnmanaged
        {
            get => ImplAttributes.HasFlag(MethodImplAttributes.Unmanaged);
            set => ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.Unmanaged, value);
        }

        public bool IsForwardRef
        {
            get => ImplAttributes.HasFlag(MethodImplAttributes.ForwardRef);
            set => ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.ForwardRef, value);
        }

        public bool NoOptimization
        {
            get => ImplAttributes.HasFlag(MethodImplAttributes.NoOptimization);
            set => ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.NoOptimization, value);
        }

        public bool PreserveSig
        {
            get => ImplAttributes.HasFlag(MethodImplAttributes.PreserveSig);
            set => ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.PreserveSig, value);
        }

        public bool IsInternalCall
        {
            get => ImplAttributes.HasFlag(MethodImplAttributes.InternalCall);
            set => ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.InternalCall, value);
        }

        public bool IsSynchronized
        {
            get => ImplAttributes.HasFlag(MethodImplAttributes.Synchronized);
            set => ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.Synchronized, value);
        }

        public bool NoInlining
        {
            get => ImplAttributes.HasFlag(MethodImplAttributes.NoInlining);
            set => ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.NoInlining, value);
        }

        /// <summary>
        /// Gets a value indicating whether the method is a constructor.
        /// </summary>
        public bool IsConstructor => IsRuntimeSpecialName && IsSpecialName && (Name == ".ctor" || Name == ".cctor");

        private bool GetMethodAccessAttribute(MethodAttributes attribute)
        {
            return ((uint)Attributes).GetMaskedAttribute((uint)MethodAttributes.MemberAccessMask,
                (uint)attribute);
        }

        private void SetMethodAccessAttribute(MethodAttributes attribute, bool value)
        {
            Attributes = (MethodAttributes)((uint)Attributes).SetMaskedAttribute((uint)MethodAttributes.MemberAccessMask,
                (uint)attribute, value);
        }

        private bool GetMethodCodeTypeAttribute(MethodImplAttributes attribute)
        {
            return ((uint)ImplAttributes).GetMaskedAttribute((uint)MethodImplAttributes.CodeTypeMask,
                (uint)attribute);
        }

        private void SetMethodCodeTypeAttribute(MethodImplAttributes attribute, bool value)
        {
            ImplAttributes = (MethodImplAttributes)((uint)ImplAttributes).SetMaskedAttribute((uint)MethodImplAttributes.CodeTypeMask,
                (uint)attribute, value);
        }

        public override string ToString()
        {
            return FullName;
        }

        public IMetadataMember Resolve()
        {
            return this;
        }

        // TODO
        //IGenericParameterProvider IGenericContext.Type
        //{
        //    get { return DeclaringType; }
        //}

        //IGenericParameterProvider IGenericContext.Method
        //{
        //    get { return this; }
        //}

        private static void SetParamOwner(ParameterDefinition param, MethodDefinition method)
        {
            param.Method = method;
        }

        private static MethodDefinition GetParamOwner(ParameterDefinition param)
        {
            return param.Method;
        }
    }
}