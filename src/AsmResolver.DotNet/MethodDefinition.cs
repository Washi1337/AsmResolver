using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single method in a type definition of a .NET module.
    /// </summary>
    public class MethodDefinition :
        MetadataMember,
        IMemberDefinition,
        IOwnedCollectionElement<TypeDefinition>,
        IMemberRefParent, 
        ICustomAttributeType,
        IHasGenericParameters,
        IMemberForwarded,
        IHasSecurityDeclaration,
        IManagedEntrypoint
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private readonly LazyVariable<MethodSignature> _signature;
        private readonly LazyVariable<MethodBody> _methodBody;
        private readonly LazyVariable<ImplementationMap> _implementationMap;
        private readonly LazyVariable<MethodSemantics> _semantics;
        private IList<ParameterDefinition> _parameterDefinitions;
        private ParameterCollection _parameters;
        private IList<CustomAttribute> _customAttributes;
        private IList<SecurityDeclaration> _securityDeclarations;
        private IList<GenericParameter> _genericParameters;

        /// <summary>
        /// Initializes a new method definition.
        /// </summary>
        /// <param name="token">The token of the method</param>
        protected MethodDefinition(MetadataToken token)
            : base(token)
        {
            _name  =new LazyVariable<string>(GetName);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
            _signature = new LazyVariable<MethodSignature>(GetSignature);
            _methodBody = new LazyVariable<MethodBody>(GetBody);
            _implementationMap = new LazyVariable<ImplementationMap>(GetImplementationMap);
            _semantics = new LazyVariable<MethodSemantics>(GetSemantics);
        }

        /// <summary>
        /// Creates a new method definition.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="signature">The signature of the method</param>
        /// <remarks>
        /// For a valid .NET image, if <see cref="CallingConventionSignature.HasThis"/> of the signature referenced by
        /// <paramref name="signature"/> is set, the <see cref="MethodAttributes.Static"/> bit should be unset in
        /// <paramref name="attributes"/> and vice versa.
        /// </remarks>
        public MethodDefinition(string name, MethodAttributes attributes, MethodSignature signature)
            : this(new MetadataToken(TableIndex.Method, 0))
        {
            Name = name;
            Attributes = attributes;
            Signature = signature;
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the signature of the method This includes the return type, as well as the types of the
        /// parameters that this method defines.
        /// </summary>
        public MethodSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public string FullName => FullNameGenerator.GetMethodFullName(Name, DeclaringType, Signature);

        /// <summary>
        /// Gets or sets the attributes associated to the method.
        /// </summary>
        public MethodAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is compiler controlled and cannot be referenced directly.
        /// </summary>
        public bool IsCompilerControlled
        {
            get => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.CompilerControlled;
            set => Attributes = value ? Attributes & ~MethodAttributes.MemberAccessMask : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is marked private and can only be accessed by
        /// members within the same enclosing type.
        /// </summary>
        public bool IsPrivate
        {
            get => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
            set => Attributes = (Attributes & ~MethodAttributes.MemberAccessMask)
                                | (value ? MethodAttributes.Private : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is marked family and assembly, and can only be accessed by
        /// members within the same enclosing type and any derived type, within the same assembly.
        /// </summary>
        public bool IsFamilyAndAssembly
        {
            get => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamilyAndAssembly;
            set => Attributes = (Attributes & ~MethodAttributes.MemberAccessMask)
                                | (value ? MethodAttributes.FamilyAndAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is marked private and can only be accessed by
        /// members within the same assembly.
        /// </summary>
        public bool IsAssembly
        {
            get => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;
            set => Attributes = (Attributes & ~MethodAttributes.MemberAccessMask)
                                | (value ? MethodAttributes.Assembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is marked private and can only be accessed by
        /// members within the same enclosing type, as well as any derived type.
        /// </summary>
        public bool IsFamily
        {
            get => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;
            set => Attributes = (Attributes & ~MethodAttributes.MemberAccessMask)
                                | (value ? MethodAttributes.Family : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is marked family or assembly, and can only be accessed by
        /// members within the same enclosing type and any derived type, or within the same assembly.
        /// </summary>
        public bool IsFamilyOrAssembly
        {
            get => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamilyOrAssembly;
            set => Attributes = (Attributes & ~MethodAttributes.MemberAccessMask)
                                | (value ? MethodAttributes.FamilyOrAssembly : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is marked public, and can be accessed by
        /// any member having access to the enclosing type.
        /// </summary>
        public bool IsPublic
        {
            get => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
            set => Attributes = (Attributes & ~MethodAttributes.MemberAccessMask)
                                | (value ? MethodAttributes.Public : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the managed method is exported by a thunk to unmanaged code.
        /// </summary>
        public bool IsUnmanagedExport
        {
            get => (Attributes & MethodAttributes.UnmanagedExport) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.UnmanagedExport)
                                | (value ? MethodAttributes.UnmanagedExport : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method requires an object instance to access it.
        /// </summary>
        /// <remarks>
        /// This property does not reflect the value of <see cref="CallingConventionSignature.HasThis"/>, nor will it
        /// change the value of <see cref="CallingConventionSignature.HasThis"/> if this property is changed. For a
        /// valid .NET image, these values should match, however.
        /// </remarks>
        public bool IsStatic
        {
            get => (Attributes & MethodAttributes.Static) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.Static)
                                | (value ? MethodAttributes.Static : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is marked final and cannot be overridden by a derived
        /// class.
        /// </summary>
        public bool IsFinal
        {
            get => (Attributes & MethodAttributes.Final) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.Final)
                                | (value ? MethodAttributes.Final : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is virtual.
        /// </summary>
        public bool IsVirtual
        {
            get => (Attributes & MethodAttributes.Virtual) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.Virtual)
                                | (value ? MethodAttributes.Virtual : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is distinguished by both its name and signature.
        /// </summary>
        public bool IsHideBySig
        {
            get => (Attributes & MethodAttributes.HideBySig) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.HideBySig)
                                | (value ? MethodAttributes.HideBySig : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the runtime should reuse an existing slot in the VTable of the
        /// enclosing class for this method.
        /// </summary>
        public bool IsReuseSlot
        {
            get => (Attributes & MethodAttributes.VtableLayoutMask) == MethodAttributes.ReuseSlot;
            set => Attributes = value ? Attributes & ~MethodAttributes.VtableLayoutMask : Attributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the runtime allocate a new slot in the VTable of the
        /// enclosing class for this method.
        /// </summary>
        public bool IsNewSlot
        {
            get => (Attributes & MethodAttributes.VtableLayoutMask) == MethodAttributes.NewSlot;
            set => Attributes = (Attributes & ~MethodAttributes.VtableLayoutMask)
                                | (value ? MethodAttributes.NewSlot : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method can only be overridden if it is also accessible. 
        /// </summary>
        public bool CheckAccessOnOverride
        {
            get => (Attributes & MethodAttributes.CheckAccessOnOverride) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.CheckAccessOnOverride)
                                | (value ? MethodAttributes.CheckAccessOnOverride : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method is marked abstract, and should be overridden by a derived class.
        /// </summary>
        /// <remarks>
        /// Methods with this flag set should not have a method body assigned for a valid .NET executable. However,
        /// updating this flag will not remove the body of this method, nor does the existence of the method body reflect
        /// the value of this property. 
        /// </remarks>
        public bool IsAbstract
        {
            get => (Attributes & MethodAttributes.Abstract) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.Abstract)
                                | (value ? MethodAttributes.Abstract : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method is given a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => (Attributes & MethodAttributes.SpecialName) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.SpecialName)
                                | (value ? MethodAttributes.SpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method is given a special name that is used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => (Attributes & MethodAttributes.RuntimeSpecialName) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.RuntimeSpecialName)
                                | (value ? MethodAttributes.RuntimeSpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method contains Platform Invoke information.
        /// </summary>
        /// <remarks>
        /// Methods containing Platform Invoke information should have this flag set. This property does not
        /// update automatically however when P/Invoke information is assigned to this method, nor does it reflect
        /// the existence of P/Invoke information.
        /// </remarks>
        public bool IsPInvokeImpl
        {
            get => (Attributes & MethodAttributes.PInvokeImpl) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.PInvokeImpl)
                                | (value ? MethodAttributes.PInvokeImpl : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method has security attributes assigned to it.
        /// </summary>
        /// <remarks>
        /// Methods containing security attributes should have this flag set. This property does not automatically
        /// however when attributes are added or removed from this method, nor does it reflect the existence of
        /// attributes.
        /// </remarks>
        public bool HasSecurity
        {
            get => (Attributes & MethodAttributes.HasSecurity) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.HasSecurity)
                                | (value ? MethodAttributes.HasSecurity : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating themethod calls another method containing security code.
        /// </summary>
        public bool RequireSecObject
        {    
            get => (Attributes & MethodAttributes.RequireSecObject) != 0;
            set => Attributes = (Attributes & ~MethodAttributes.RequireSecObject)
                                | (value ? MethodAttributes.RequireSecObject : 0);
        }

        /// <summary>
        /// Gets or sets the attributes that describe the implementation of the method body.
        /// </summary>
        public MethodImplAttributes ImplAttributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the method body is implemented using the Common Intermediate Language (CIL).
        /// </summary>
        public bool IsIL
        {
            get => (ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL;
            set => ImplAttributes = value ? ImplAttributes & ~MethodImplAttributes.CodeTypeMask : ImplAttributes;
        }

        /// <summary>
        /// Gets or sets a value indicating the method body is implemented using the Common Intermediate Language (CIL).
        /// </summary>
        public bool IsNative
        {
            get => (ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.Native;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.CodeTypeMask)
                                    | (value ? MethodImplAttributes.Native : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method body is implemented using OPTIL.
        /// </summary>
        public bool IsOPTIL
        {
            get => (ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.OPTIL;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.CodeTypeMask)
                                    | (value ? MethodImplAttributes.OPTIL : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the method body is implemented by the runtime.
        /// </summary>
        public bool IsRuntime
        {
            get => (ImplAttributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.Runtime;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.CodeTypeMask)
                                    | (value ? MethodImplAttributes.Runtime : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method body is managed by the runtime.
        /// </summary>
        public bool Managed
        {
            get => (ImplAttributes & MethodImplAttributes.ManagedMask) == MethodImplAttributes.Managed;
            set => ImplAttributes = value ? ImplAttributes & ~MethodImplAttributes.ManagedMask : ImplAttributes;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method body is not managed by the runtime.
        /// </summary>
        public bool Unmanaged
        {
            get => (ImplAttributes & MethodImplAttributes.ManagedMask) == MethodImplAttributes.Unmanaged;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.ManagedMask)
                                    | (value ? MethodImplAttributes.Unmanaged : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method body is forwarded.
        /// </summary>
        public bool IsForwardReference
        {
            get => (ImplAttributes & MethodImplAttributes.ForwardRef) != 0;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.ForwardRef)
                                    | (value ? MethodImplAttributes.ForwardRef : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the runtime should not optimize the code upon generating native code.
        /// </summary>
        public bool IsNoOptimization
        {
            get => (ImplAttributes & MethodImplAttributes.NoOptimization) != 0;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.NoOptimization)
                                    | (value ? MethodImplAttributes.NoOptimization : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method's signature is not to be mangled to do HRESULT conversion.
        /// </summary>
        public bool PreserveSignature
        {
            get => (ImplAttributes & MethodImplAttributes.PreserveSig) != 0;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.PreserveSig)
                                    | (value ? MethodImplAttributes.PreserveSig : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method is an internal call into the runtime.
        /// </summary>
        public bool IsInternalCall
        {
            get => (ImplAttributes & MethodImplAttributes.InternalCall) != 0;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.InternalCall)
                                    | (value ? MethodImplAttributes.InternalCall : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating only one thread can run the method at once. 
        /// </summary>
        public bool IsSynchronized
        {
            get => (ImplAttributes & MethodImplAttributes.Synchronized) != 0;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.Synchronized)
                                    | (value ? MethodImplAttributes.Synchronized : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method can be inlined by the runtime or not.
        /// </summary>
        public bool NoInlining
        {
            get => (ImplAttributes & MethodImplAttributes.NoInlining) != 0;
            set => ImplAttributes = (ImplAttributes & ~MethodImplAttributes.NoInlining)
                                    | (value ? MethodImplAttributes.NoInlining : 0);
        }
        
        /// <inheritdoc />
        public virtual ModuleDefinition Module => DeclaringType?.Module;

        /// <summary>
        /// Gets the type that defines the method.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            set => _declaringType.Value = value;
        }

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;
        
        ITypeDefOrRef IMethodDefOrRef.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        /// <summary>
        /// Gets a collection of parameter definitions that this method defines.
        /// </summary>
        /// <remarks>
        /// This property might not reflect the list of actual parameters that the method defines and uses according
        /// to the method signature. This property only reflects the list that is inferred from the ParamList column
        /// in the metadata row. For the actual list of parameters, use the <see cref="Parameters"/> property instead.
        /// </remarks>
        public IList<ParameterDefinition> ParameterDefinitions
        {
            get
            {
                if (_parameterDefinitions is null)
                    Interlocked.CompareExchange(ref _parameterDefinitions, GetParameterDefinitions(), null);
                return _parameterDefinitions;
            }
        }

        /// <summary>
        /// Gets a collection of parameters that the method signature defines.
        /// </summary>
        public ParameterCollection Parameters
        {
            get
            {
                if (_parameters is null)
                    Interlocked.CompareExchange(ref _parameters, new ParameterCollection(this), null);
                return _parameters;
            }
        }

        /// <summary>
        /// Gets or sets the body of the method. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Updating this property does not automatically set the appropriate implementation attributes in the
        /// <see cref="ImplAttributes"/>.
        /// </para>
        /// </remarks>
        public MethodBody MethodBody
        {
            get => _methodBody.Value;
            set => _methodBody.Value = value;
        }

        /// <summary>
        /// Gets or sets the managed CIL body of the method if available. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is set to <c>null</c>, it does not necessarily mean the method does not have a method body.
        /// There could be an unmanaged method body assigned instead. See the <see cref="MethodBody"/> property instead.
        /// </para>
        /// <para>
        /// Updating this property does not automatically set the appropriate implementation attributes in the
        /// <see cref="ImplAttributes"/>.
        /// </para>
        /// </remarks>
        public CilMethodBody CilMethodBody
        {
            get => MethodBody as CilMethodBody;
            set => MethodBody = value;
        }

        /// <inheritdoc />
        public ImplementationMap ImplementationMap
        {
            get => _implementationMap.Value;
            set
            {
                if (value?.MemberForwarded is {})
                    throw new ArgumentException("Cannot add an implementation map that was already added to another member.");
                if (_implementationMap.Value is {})
                    _implementationMap.Value.MemberForwarded = null;
                _implementationMap.Value = value;
                if (value is {})
                    value.MemberForwarded = this;
            }
        }

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

        /// <inheritdoc />
        public IList<SecurityDeclaration> SecurityDeclarations
        {
            get
            {
                if (_securityDeclarations is null)
                    Interlocked.CompareExchange(ref _securityDeclarations, GetSecurityDeclarations(), null);
                return _securityDeclarations;
            }
        }

        /// <inheritdoc />
        public IList<GenericParameter> GenericParameters
        {
            get
            {
                if (_genericParameters is null)
                    Interlocked.CompareExchange(ref _genericParameters, GetGenericParameters(), null);
                return _genericParameters;
            }
        }

        /// <summary>
        /// Gets the semantics associated to this method (if available).
        /// </summary>
        public MethodSemantics Semantics
        {
            get => _semantics.Value;
            set => _semantics.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether the method is a get method for a property. 
        /// </summary>
        public bool IsGetMethod => Semantics != null && (Semantics.Attributes & MethodSemanticsAttributes.Getter) != 0;

        /// <summary>
        /// Gets a value indicating whether the method is a set method for a property. 
        /// </summary>
        public bool IsSetMethod => Semantics != null && (Semantics.Attributes & MethodSemanticsAttributes.Setter) != 0;

        /// <summary>
        /// Gets a value indicating whether the method is an add method for an event. 
        /// </summary>
        public bool IsAddMethod => Semantics != null && (Semantics.Attributes & MethodSemanticsAttributes.AddOn) != 0;

        /// <summary>
        /// Gets a value indicating whether the method is a remove method for an event. 
        /// </summary>
        public bool IsRemoveMethod => Semantics != null && (Semantics.Attributes & MethodSemanticsAttributes.RemoveOn) != 0;

        /// <summary>
        /// Gets a value indicating whether the method is a fire method for an event. 
        /// </summary>
        public bool IsFireMethod => Semantics != null && (Semantics.Attributes & MethodSemanticsAttributes.Fire) != 0;

        /// <summary>
        /// Gets a value indicating whether the method is a (class) constructor.
        /// </summary>
        public bool IsConstructor => IsSpecialName && IsRuntimeSpecialName && (Name == ".cctor" || Name == ".ctor");

        MethodDefinition IMethodDescriptor.Resolve() => this;

        IMemberDefinition IMemberDescriptor.Resolve() => this;

        /// <inheritdoc />
        public bool IsAccessibleFromType(TypeDefinition type)
        {
            if (!DeclaringType.IsAccessibleFromType(type))
                return false;
            
            var comparer = new SignatureComparer();
            bool isInSameAssembly = comparer.Equals(DeclaringType.Module, type.Module);

            return IsPublic
                   || isInSameAssembly && IsAssembly
                   || comparer.Equals(DeclaringType, type);
            // TODO: check if in the same family of declaring types.
        }

        /// <summary>
        /// Obtains the name of the method definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the declaring type of the method definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetDeclaringType() => null;

        /// <summary>
        /// Obtains the signature of the method definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual MethodSignature GetSignature() => null;

        /// <summary>
        /// Obtains the parameter definitions of the method definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ParameterDefinitions"/> property.
        /// </remarks>
        protected virtual IList<ParameterDefinition> GetParameterDefinitions() => 
            new OwnedCollection<MethodDefinition, ParameterDefinition>(this);

        /// <summary>
        /// Obtains the body of the method definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="MethodBody"/> property.
        /// </remarks>
        protected virtual MethodBody GetBody() => null;

        /// <summary>
        /// Obtains the platform invoke information assigned to the method.
        /// </summary>
        /// <returns>The mapping.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="ImplementationMap"/> property.
        /// </remarks>
        protected virtual ImplementationMap GetImplementationMap() => null;
        
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
        /// Obtains the list of security declarations assigned to the member.
        /// </summary>
        /// <returns>The security declarations</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="SecurityDeclarations"/> property.
        /// </remarks>
        protected virtual IList<SecurityDeclaration> GetSecurityDeclarations() => 
            new OwnedCollection<IHasSecurityDeclaration, SecurityDeclaration>(this);
        
        /// <summary>
        /// Obtains the list of generic parameters this member declares.
        /// </summary>
        /// <returns>The generic parameters</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="GenericParameters"/> property.
        /// </remarks>
        protected virtual IList<GenericParameter> GetGenericParameters() =>
            new OwnedCollection<IHasGenericParameters, GenericParameter>(this);

        /// <summary>
        /// Obtains the semantics associated to the method (if available).
        /// </summary>
        /// <returns>The semantics, or <c>null</c> if the method was not assigned semantics.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Semantics"/> property.
        /// </remarks>
        protected virtual MethodSemantics GetSemantics() => null;
        
        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}