using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Describes the explicit layout of a type, including its total and packing size. 
    /// </summary>
    public class ClassLayout : MetadataMember
    {
        private readonly LazyVariable<TypeDefinition> _parent;
        
        /// <summary>
        /// Initializes the class layout with a metadata token.
        /// </summary>
        /// <param name="token"></param>
        protected ClassLayout(MetadataToken token)
            : base(token)
        {
            _parent = new LazyVariable<TypeDefinition>(GetParent);
        }

        /// <summary>
        /// Creates a new explicit layout for a type.
        /// </summary>
        /// <param name="packingSize">The alignment in bytes of each field in the type.
        /// This value should be a power of two between 0 and 128.</param>
        /// <param name="classSize">The size in bytes of the type.</param>
        public ClassLayout(ushort packingSize, uint classSize)
            : this(new MetadataToken(TableIndex.ClassLayout, 0))
        {
            PackingSize = packingSize;
            ClassSize = classSize;
        }

        /// <summary>
        /// Gets the alignment in bytes of each field in the type. 
        /// </summary>
        /// <remarks>
        /// This value should be a power of two between 0 and 128.
        /// </remarks>
        public ushort PackingSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the size in bytes of the type.
        /// </summary>
        public uint ClassSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type that this layout is assigned to.
        /// </summary>
        public TypeDefinition Parent
        {
            get => _parent.Value;
            internal set => _parent.Value = value;
        }

        /// <summary>
        /// Obtains the type this layout is assigned to.
        /// </summary>
        /// <returns>The parent type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Parent"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetParent() => null;

        /// <inheritdoc />
        public override string ToString() => 
            $"{nameof(PackingSize)}: {PackingSize}, {nameof(ClassSize)}: {ClassSize}";
    }
}