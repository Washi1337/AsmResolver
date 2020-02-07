using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single definition for a parameter that is defined by a method in a .NET executable file.
    /// </summary>
    /// <remarks>
    /// A method is not required to provide parameter definitions for all its parameters that are defined by its
    /// signature. Parameter definitions only provide additional information, such as a name, attributes or a default
    /// value.
    /// </remarks>
    public class ParameterDefinition :
        IHasCustomAttribute,
        IHasConstant,
        IOwnedCollectionElement<MethodDefinition>
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<MethodDefinition> _method;
        private readonly LazyVariable<Constant> _constant;
        private IList<CustomAttribute> _customAttributes;

        /// <summary>
        /// Initializes a new parameter definition.
        /// </summary>
        /// <param name="token">The token of the parameter definition.</param>
        protected ParameterDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _method = new LazyVariable<MethodDefinition>(GetMethod);
            _constant = new LazyVariable<Constant>(GetConstant);
        }

        /// <summary>
        /// Creates a new parameter definition using the provided name.
        /// </summary>
        /// <param name="name">The name of the new parameter.</param>
        public ParameterDefinition(string name)
            : this(new MetadataToken(TableIndex.Param, 0))
        {
            Name = name;
        }

        /// <summary>
        /// Creates a new parameter definition using the provided name and attributes.
        /// </summary>
        /// <param name="name">The name of the new parameter.</param>
        /// <param name="attributes">The attributes to assign to the parameter.</param>
        public ParameterDefinition(string name, ParameterAttributes attributes)
            : this(new MetadataToken(TableIndex.Param, 0))
        {
            Name = name;
            Attributes = attributes;
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
        /// Gets or sets the index for which this parameter definition provides information for.
        /// </summary>
        public ushort Sequence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the parameter definition.
        /// </summary>
        public ParameterAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the method that defines the parameter.
        /// </summary>
        public MethodDefinition Method
        {
            get => _method.Value;
            private set => _method.Value = value;
        }

        MethodDefinition IOwnedCollectionElement<MethodDefinition>.Owner
        {
            get => Method;
            set => Method = value;
        }

        /// <inheritdoc />
        public ModuleDefinition Module => Method?.Module;

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
        public Constant Constant
        {
            get => _constant.Value;
            set => _constant.Value = value;
        }

        /// <summary>
        /// Obtains the name of the parameter.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the method that owns the parameter.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Method"/> property.
        /// </remarks>
        protected virtual MethodDefinition GetMethod() => null;

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
        /// Obtains the constant value assigned to the parameter definition.
        /// </summary>
        /// <returns>The constant.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Constant"/> property.
        /// </remarks>
        protected virtual Constant GetConstant() => null;

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}