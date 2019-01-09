using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata added to a method that implements another method declared in an interface.
    /// </summary>
    public class MethodImplementation : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _class;
        private readonly LazyValue<IMethodDefOrRef> _methodBody;
        private readonly LazyValue<IMethodDefOrRef> _methodDeclaration;
        private MetadataImage _image;

        public MethodImplementation(IMethodDefOrRef methodBody,
            IMethodDefOrRef methodDeclaration)
            : base(new MetadataToken(MetadataTokenType.MethodImpl))
        {
            _class = new LazyValue<TypeDefinition>();
            _methodBody = new LazyValue<IMethodDefOrRef>(methodBody);
            _methodDeclaration = new LazyValue<IMethodDefOrRef>(methodDeclaration);
        }

        internal MethodImplementation(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var tableStream = image.Header.GetStream<TableStream>();
            var encoder = tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);

            _class = new LazyValue<TypeDefinition>(() =>
            {
                var table = tableStream.GetTable(MetadataTokenType.TypeDef);
                return table.TryGetRow((int) (row.Column1 - 1), out var typeRow) 
                    ? (TypeDefinition) table.GetMemberFromRow(image, typeRow) 
                    : null;
            });

            _methodBody = new LazyValue<IMethodDefOrRef>(() =>
            {
                var methodBodyToken = encoder.DecodeIndex(row.Column2);
                return image.TryResolveMember(methodBodyToken, out var member) ? (IMethodDefOrRef) member : null;
            });

            _methodDeclaration = new LazyValue<IMethodDefOrRef>(() =>
            {
                var declarationToken = encoder.DecodeIndex(row.Column3);
                return image.TryResolveMember(declarationToken, out var member) ? (IMethodDefOrRef) member : null;
            });
        }

        /// <inheritdoc />
        public override MetadataImage Image => _class.IsInitialized && _class.Value != null 
            ? _class.Value.Image 
            : _image;

        /// <summary>
        /// Gets the interface that is implemented.
        /// </summary>
        public TypeDefinition Class
        {
            get => _class.Value;
            internal set
            {
                _class.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the method that implements the interface method.
        /// </summary>
        public IMethodDefOrRef MethodBody
        {
            get => _methodBody.Value;
            set => _methodBody.Value = value;
        }

        /// <summary>
        /// Gets or sets the method to be implemented.
        /// </summary>
        public IMethodDefOrRef MethodDeclaration
        {
            get => _methodDeclaration.Value;
            set => _methodDeclaration.Value = value;
        }
    }
}