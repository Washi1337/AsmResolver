using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to a method or a field in an (external) .NET assembly.
    /// </summary>
    public class MemberReference : MetadataMember, ICustomAttributeType, IFieldDescriptor
    {
        private readonly LazyVariable<IMemberRefParent?> _parent;
        private readonly LazyVariable<string?> _name;
        private readonly LazyVariable<CallingConventionSignature?> _signature;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes a new member reference.
        /// </summary>
        /// <param name="token">The metadata token of the reference.</param>
        protected MemberReference(MetadataToken token)
            : base(token)
        {
            _parent = new LazyVariable<IMemberRefParent?>(GetParent);
            _name = new LazyVariable<string?>(GetName);
            _signature = new LazyVariable<CallingConventionSignature?>(GetSignature);
        }

        /// <summary>
        /// Creates a new reference to a member in an (external) .NET assembly.
        /// </summary>
        /// <param name="parent">The declaring member that defines the referenced member.</param>
        /// <param name="name">The name of the referenced member.</param>
        /// <param name="signature">The signature of the referenced member. This dictates whether the
        /// referenced member is a field or a method.</param>
        public MemberReference(IMemberRefParent parent, string name, MemberSignature signature)
            : this(new MetadataToken(TableIndex.MemberRef, 0))
        {
            Parent = parent;
            Name = name;
            Signature = signature;
        }

        /// <summary>
        /// Gets or sets the member that declares the referenced member.
        /// </summary>
        public IMemberRefParent? Parent
        {
            get => _parent.Value;
            set => _parent.Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the referenced member.
        /// </summary>
        public string? Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the signature of the referenced member.
        /// </summary>
        /// <remarks>
        /// This property dictates whether the referenced member is a field or a method.
        /// </remarks>
        public CallingConventionSignature? Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        MethodSignature? IMethodDescriptor.Signature => Signature as MethodSignature;

        FieldSignature? IFieldDescriptor.Signature => Signature as FieldSignature;

        /// <summary>
        /// Gets a value indicating whether the referenced member is a field.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Signature))]
        public bool IsField => Signature is FieldSignature;

        /// <summary>
        /// Gets a value indicating whether the referenced member is a method
        /// </summary>
        [MemberNotNullWhen(true, nameof(Signature))]
        public bool IsMethod => Signature is MethodSignature;

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                if (IsField)
                    return FullNameGenerator.GetFieldFullName(Name, DeclaringType, (FieldSignature) Signature);
                if (IsMethod)
                    return FullNameGenerator.GetMethodFullName(Name, DeclaringType, (MethodSignature) Signature);
                return Name ?? NullName;
            }
        }

        /// <inheritdoc />
        public ModuleDefinition? Module => Parent?.Module;

        /// <summary>
        /// Gets the type that declares the referenced member, if available.
        /// </summary>
        public ITypeDefOrRef? DeclaringType => Parent switch
        {
            ITypeDefOrRef typeDefOrRef => typeDefOrRef,
            MethodDefinition method => method.DeclaringType,
            _ => null
        };

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

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
        /// Resolves the reference to a member definition.
        /// </summary>
        /// <returns>The resolved member definition, or <c>null</c> if the member could not be resolved.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the member reference has an invalid signature.</exception>
        /// <remarks>
        /// This method can only be invoked if the reference was added to a module.
        /// </remarks>
        public IMemberDefinition? Resolve()
        {
            if (IsMethod)
                return ((IMethodDescriptor) this).Resolve();
            if (IsField)
                return ((IFieldDescriptor) this).Resolve();
            throw new ArgumentOutOfRangeException();
        }

        FieldDefinition? IFieldDescriptor.Resolve()
        {
            if (!IsField)
                throw new InvalidOperationException("Member reference must reference a field.");
            return Module?.MetadataResolver.ResolveField(this);
        }

        MethodDefinition? IMethodDescriptor.Resolve()
        {
            if (!IsMethod)
                throw new InvalidOperationException("Member reference must reference a method.");
            return Module?.MetadataResolver.ResolveMethod(this);
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
        /// Obtains the parent of the member reference.
        /// </summary>
        /// <returns>The parent</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Parent"/> property.
        /// </remarks>
        protected virtual IMemberRefParent? GetParent() => null;

        /// <summary>
        /// Obtains the name of the member reference.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string? GetName() => null;

        /// <summary>
        /// Obtains the signature of the member reference.
        /// </summary>
        /// <returns>The signature</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual CallingConventionSignature? GetSignature() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}
