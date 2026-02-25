using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to a method or a field in an (external) .NET assembly.
    /// </summary>
    public partial class MemberReference : MetadataMember, IMethodDefOrRef, IFieldDescriptor
    {
        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes a new member reference.
        /// </summary>
        /// <param name="token">The metadata token of the reference.</param>
        protected MemberReference(MetadataToken token)
            : base(token)
        {
        }

        /// <summary>
        /// Creates a new reference to a member in an (external) .NET assembly.
        /// </summary>
        /// <param name="parent">The declaring member that defines the referenced member.</param>
        /// <param name="name">The name of the referenced member.</param>
        /// <param name="signature">The signature of the referenced member. This dictates whether the
        /// referenced member is a field or a method.</param>
        public MemberReference(IMemberRefParent? parent, Utf8String? name, MemberSignature? signature)
            : this(new MetadataToken(TableIndex.MemberRef, 0))
        {
            Parent = parent;
            Name = name;
            Signature = signature;
        }

        /// <summary>
        /// Gets or sets the member that declares the referenced member.
        /// </summary>
        [LazyProperty]
        public partial IMemberRefParent? Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the referenced member.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the member reference table.
        /// </remarks>
        [LazyProperty]
        public partial Utf8String? Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the signature of the referenced member.
        /// </summary>
        /// <remarks>
        /// This property dictates whether the referenced member is a field or a method.
        /// </remarks>
        [LazyProperty]
        public partial CallingConventionSignature? Signature
        {
            get;
            set;
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
                    return MemberNameGenerator.GetFieldFullName(this);
                if (IsMethod)
                    return MemberNameGenerator.GetMethodFullName(this);

                return Name ?? NullName;
            }
        }

        /// <inheritdoc />
        public ModuleDefinition? ContextModule => Parent?.ContextModule;

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
        public virtual bool HasCustomAttributes => CustomAttributesInternal is { Count: > 0 };

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (CustomAttributesInternal is null)
                    Interlocked.CompareExchange(ref CustomAttributesInternal, GetCustomAttributes(), null);
                return CustomAttributesInternal;
            }
        }

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module)
        {
            // the parent will check that their ContextModule is correct for us
            // this needs an explicit check on the parent in the case that it is a TypeSpec
            // where the TypeSpec is unimported in some way (like generic arguments, or a CustomMod modifier type)
            return (Parent?.IsImportedInModule(module) ?? false) && (Signature?.IsImportedInModule(module) ?? false);
        }

        /// <summary>
        /// Imports the member using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use for importing the object.</param>
        /// <returns>The imported member.</returns>
        public MemberReference ImportWith(ReferenceImporter importer) => IsMethod
            ? (MemberReference) importer.ImportMethod(this)
            : (MemberReference) importer.ImportField(this);

        /// <inheritdoc />
        IMethodDefOrRef IMethodDefOrRef.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        IMethodDescriptor IMethodDescriptor.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        IFieldDescriptor IFieldDescriptor.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        IMemberDescriptor IMemberDescriptor.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <summary>
        /// Resolves the reference to a member definition.
        /// </summary>
        /// <returns>The resolved member definition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the member reference has an invalid signature.</exception>
        public IMemberDefinition Resolve(RuntimeContext? context)
        {
            if (IsMethod)
                return ((IMethodDescriptor) this).Resolve(context);
            if (IsField)
                return ((IFieldDescriptor) this).Resolve(context);

            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Attempts to resolve the member descriptor to its definition in the provided context.
        /// </summary>
        /// <param name="context">The context to assume when resolving the member.</param>
        /// <param name="definition">The resolved member definition, or <c>null</c> if resolution failed.</param>
        /// <returns><c>true</c> if the resolution was successful, <c>false</c> otherwise.</returns>
        public bool TryResolve(RuntimeContext? context, [NotNullWhen(true)] out IMemberDefinition? definition)
        {
            if (IsMethod && ((IMethodDescriptor) this).TryResolve(context, out var method))
                definition = method;
            else if (IsField && ((IFieldDescriptor) this).TryResolve(context, out var field))
                definition = field;
            else
                definition = null;

            return definition is not null;
        }

        ResolutionStatus IMemberDescriptor.Resolve(RuntimeContext? context, out IMemberDefinition? definition)
        {
            if (IsMethod)
            {
                var status = ((IMethodDescriptor) this).Resolve(context, out var method);
                definition = method;
                return status;
            }

            if (IsField)
            {
                var status = ((IFieldDescriptor) this).Resolve(context, out var field);
                definition = field;
                return status;
            }

            definition = null;
            return ResolutionStatus.InvalidReference;
        }

        ResolutionStatus IFieldDescriptor.Resolve(RuntimeContext? context, out FieldDefinition? definition)
        {
            if (!IsField)
            {
                definition = null;
                return ResolutionStatus.InvalidReference;
            }

            if (context is null)
            {
                definition = null;
                return ResolutionStatus.MissingRuntimeContext;
            }

            return context.ResolveField(this, ContextModule, out definition);
        }

        ResolutionStatus IMethodDescriptor.Resolve(RuntimeContext? context, out MethodDefinition? definition)
        {
            if (!IsMethod)
            {
                definition = null;
                return ResolutionStatus.InvalidReference;
            }

            if (context is null)
            {
                definition = null;
                return ResolutionStatus.MissingRuntimeContext;
            }

            return context.ResolveMethod(this, ContextModule, out definition);
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
        protected virtual Utf8String? GetName() => null;

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
