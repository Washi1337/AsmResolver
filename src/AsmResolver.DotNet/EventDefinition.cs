using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single event in a type definition of a .NET module.
    /// </summary>
    public class EventDefinition : IHasSemantics, IMemberDescriptor, IHasCustomAttribute, IOwnedCollectionElement<TypeDefinition>
    { 
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private readonly LazyVariable<ITypeDefOrRef> _eventType;
        private IList<MethodSemantics> _semantics;

        /// <summary>
        /// Initializes a new property definition.
        /// </summary>
        /// <param name="token">The token of the property.</param>
        protected EventDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _eventType = new LazyVariable<ITypeDefOrRef>(GetEventType);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
        }

        /// <summary>
        /// Creates a new property definition.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="eventType">The delegate type of the event.</param>
        public EventDefinition(string name, EventAttributes attributes, ITypeDefOrRef eventType)
            : this(new MetadataToken(TableIndex.Event,0))
        {
            Name = name;
            Attributes = attributes;
            EventType = eventType;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
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

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string FullName => FullNameGenerator.GetEventFullName(Name, DeclaringType, EventType);

        /// <summary>
        /// Gets or sets the delegate type of the event.
        /// </summary>
        public ITypeDefOrRef EventType
        {
            get => _eventType.Value;
            set => _eventType.Value = value;
        }

        /// <inheritdoc />
        public ModuleDefinition Module => DeclaringType?.Module;

        /// <summary>
        /// Gets the type that defines the property.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            private set => _declaringType.Value = value;
        }

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
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
        /// Gets the method definition representing the add accessor of this event definition. 
        /// </summary>
        public MethodDefinition AddMethod => 
            Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.AddOn)?.Method;
        
        /// <summary>
        /// Gets the method definition representing the remove accessor of this event definition. 
        /// </summary>
        public MethodDefinition RemoveMethod => 
            Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.RemoveOn)?.Method;
        
        /// <summary>
        /// Gets the method definition representing the fire accessor of this event definition. 
        /// </summary>
        public MethodDefinition FireMethod => 
            Semantics.FirstOrDefault(s => s.Attributes == MethodSemanticsAttributes.Fire)?.Method;
        
        /// <summary>
        /// Obtains the name of the property definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;
        
        /// <summary>
        /// Obtains the event type of the property definition.
        /// </summary>
        /// <returns>The event type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="EventType"/> property.
        /// </remarks>
        protected virtual ITypeDefOrRef GetEventType() => null;
        
        /// <summary>
        /// Obtains the declaring type of the property definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetDeclaringType() => null;

        /// <summary>
        /// Obtains the methods associated to this property definition.
        /// </summary>
        /// <returns>The method semantic objects.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Semantics"/> property.
        /// </remarks>
        protected virtual IList<MethodSemantics> GetSemantics() =>
            new OwnedCollection<IHasSemantics, MethodSemantics>(this);
        
        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}