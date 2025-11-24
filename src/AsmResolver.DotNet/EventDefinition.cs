using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single event in a type definition of a .NET module.
    /// </summary>
    public partial class EventDefinition :
        MetadataMember,
        IHasSemantics,
        IHasCustomAttribute,
        IOwnedCollectionElement<TypeDefinition>
    {
        private IList<MethodSemantics>? _semantics;

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes a new property definition.
        /// </summary>
        /// <param name="token">The token of the property.</param>
        protected EventDefinition(MetadataToken token)
            : base(token)
        {
        }

        /// <summary>
        /// Creates a new property definition.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="eventType">The delegate type of the event.</param>
        public EventDefinition(Utf8String? name, EventAttributes attributes, ITypeDefOrRef? eventType)
            : this(new MetadataToken(TableIndex.Event,0))
        {
            Name = name;
            Attributes = attributes;
            EventType = eventType;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the field.
        /// </summary>
        public EventAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the event uses a special name.
        /// </summary>
        public bool IsSpecialName
        {
            get => (Attributes & EventAttributes.SpecialName) != 0;
            set => Attributes = (Attributes & ~EventAttributes.SpecialName)
                                | (value ? EventAttributes.SpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the event uses a special name used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => (Attributes & EventAttributes.RtSpecialName) != 0;
            set => Attributes = (Attributes & ~EventAttributes.RtSpecialName)
                                | (value ? EventAttributes.RtSpecialName : 0);
        }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the event table.
        /// </remarks>
        [LazyProperty]
        public partial Utf8String? Name
        {
            get;
            set;
        }

        string? INameProvider.Name => Name;

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetEventFullName(this);

        /// <summary>
        /// Gets or sets the delegate type of the event.
        /// </summary>
        [LazyProperty]
        public partial ITypeDefOrRef? EventType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ModuleDefinition? DeclaringModule => DeclaringType?.DeclaringModule;

        ModuleDefinition? IModuleProvider.ContextModule => DeclaringModule;

        /// <summary>
        /// Gets the type that defines the property.
        /// </summary>
        [LazyProperty]
        public partial TypeDefinition? DeclaringType
        {
            get;
            private set;
        }

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition? IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        /// <inheritdoc />
        public IList<MethodSemantics> Semantics
        {
            get
            {
                if (_semantics is null)
                    Interlocked.CompareExchange(ref _semantics, GetSemantics(), null);
                return _semantics;
            }
        }

        /// <summary>
        /// Gets the method definition representing the first add accessor of this event definition.
        /// </summary>
        public MethodDefinition? AddMethod
        {
            get => Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.AddOn)?.Method;
            set => SetSemanticMethods(value, RemoveMethod, FireMethod);
        }

        /// <summary>
        /// Gets the method definition representing the first remove accessor of this event definition.
        /// </summary>
        public MethodDefinition? RemoveMethod
        {
            get => Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.RemoveOn)?.Method;
            set => SetSemanticMethods(AddMethod, value, FireMethod);
        }

        /// <summary>
        /// Gets the method definition representing the first fire accessor of this event definition.
        /// </summary>
        public MethodDefinition? FireMethod
        {
            get => Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.Fire)?.Method;
            set => SetSemanticMethods(AddMethod, RemoveMethod, value);
        }

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

        /// <summary>
        /// Clear <see cref="Semantics"/> and apply these methods to the event definition.
        /// </summary>
        /// <param name="addMethod">The method definition representing the add accessor of this event definition.</param>
        /// <param name="removeMethod">The method definition representing the remove accessor of this event definition.</param>
        /// <param name="fireMethod">The method definition representing the fire accessor of this event definition.</param>
        public void SetSemanticMethods(MethodDefinition? addMethod, MethodDefinition? removeMethod, MethodDefinition? fireMethod)
        {
            Semantics.Clear();
            if (addMethod is not null)
                Semantics.Add(new MethodSemantics(addMethod, MethodSemanticsAttributes.AddOn));
            if (removeMethod is not null)
                Semantics.Add(new MethodSemantics(removeMethod, MethodSemanticsAttributes.RemoveOn));
            if (fireMethod is not null)
                Semantics.Add(new MethodSemantics(fireMethod, MethodSemanticsAttributes.Fire));
        }

        /// <inheritdoc />
        public bool IsAccessibleFromType(TypeDefinition type) =>
            Semantics.Any(s => s.Method?.IsAccessibleFromType(type) ?? false);

        IMemberDefinition IMemberDescriptor.Resolve() => this;

        IMemberDefinition IMemberDescriptor.Resolve(ModuleDefinition context) => this;

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module)
        {
            return DeclaringModule == module && (EventType?.IsImportedInModule(module) ?? false);
        }

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => throw new NotSupportedException();

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
        /// Obtains the name of the event definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the event type of the event definition.
        /// </summary>
        /// <returns>The event type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="EventType"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef? GetEventType() => null;

        /// <summary>
        /// Obtains the declaring type of the event definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition? GetDeclaringType() => null;

        /// <summary>
        /// Obtains the methods associated to this event definition.
        /// </summary>
        /// <returns>The method semantic objects.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Semantics"/> property.
        /// </remarks>
        protected virtual IList<MethodSemantics> GetSemantics() => new MethodSemanticsCollection(this);

        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}
