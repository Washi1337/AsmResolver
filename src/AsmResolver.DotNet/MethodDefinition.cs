using AsmResolver.DotNet.Blob;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single method in a type definition of a .NET module.
    /// </summary>
    public class MethodDefinition : IMetadataMember, IMemberDescriptor, IOwnedCollectionElement<TypeDefinition>
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private readonly LazyVariable<MethodSignature> _signature;

        /// <summary>
        /// Initializes a new method definition.
        /// </summary>
        /// <param name="token">The token of the method</param>
        protected MethodDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name  =new LazyVariable<string>(GetName);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
            _signature = new LazyVariable<MethodSignature>(GetSignature);
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
        /// Gets or sets the signature of the method This includes the return type, as well as the types of the
        /// parameters that this method defines.
        /// </summary>
        public MethodSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                string parameterTypesString = string.Join(", ", Signature.ParameterTypes)
                                              + (Signature.IsSentinel ? ", ..." : string.Empty);
                return DeclaringType is null
                    ? $"{Signature.ReturnType} {Name}({parameterTypesString})"
                    : $"{Signature.ReturnType} {DeclaringType}::{Name}({parameterTypesString})";
            }
        }

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

        /// <inheritdoc />
        public ModuleDefinition Module => DeclaringType.Module;

        /// <summary>
        /// Gets the type that defines the method.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            set => _declaringType.Value = value;
        }

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
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

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}