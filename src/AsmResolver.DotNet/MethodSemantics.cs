using System;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an object that associates a method definition to a property or an event.
    /// </summary>
    public partial class MethodSemantics : MetadataMember, IOwnedCollectionElement<IHasSemantics>
    {
        private readonly object _lock = new();

        /// <summary>
        /// Initializes an empty method semantics object.
        /// </summary>
        /// <param name="token">The metadata token of the semantics object.</param>
        protected MethodSemantics(MetadataToken token)
            : base(token)
        {
        }

        /// <summary>
        /// Creates a new method semantics object.
        /// </summary>
        /// <param name="method">The method to give special semantics.</param>
        /// <param name="attributes">The type of semantics to assign.</param>
        public MethodSemantics(MethodDefinition? method, MethodSemanticsAttributes attributes)
            : this(new MetadataToken(TableIndex.MethodSemantics, 0))
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Attributes = attributes;
        }

        /// <summary>
        /// Gets or sets the type of semantics that are associated to the method.
        /// </summary>
        public MethodSemanticsAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method that is given special semantics.
        /// </summary>
        [LazyProperty]
        public partial MethodDefinition? Method
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the member that the method is associated to.
        /// </summary>
        [LazyProperty]
        public partial IHasSemantics? Association
        {
            get;
            private set;
        }

        IHasSemantics? IOwnedCollectionElement<IHasSemantics>.Owner
        {
            get => Association;
            set => Association = value;
        }

        /// <summary>
        /// Obtains the method that was given special semantics.
        /// </summary>
        /// <returns>The method</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Method"/> property.
        /// </remarks>
        protected virtual MethodDefinition? GetMethod() => null;

        /// <summary>
        /// Obtains the member that the method is association to.
        /// </summary>
        /// <returns>The member</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Association"/> property.
        /// </remarks>
        protected virtual IHasSemantics? GetAssociation() => null;

        /// <inheritdoc />
        public override string ToString() => $"{Attributes} {Method?.FullName}";
    }
}
