namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides an extension to the normal <see cref="ReferenceImporter"/> class, that takes cloned members into account.
    /// </summary>
    public class CloneContextAwareReferenceImporter : ReferenceImporter
    {
        private readonly MemberCloneContext _context;

        /// <summary>
        /// Creates a new instance of the <see cref="CloneContextAwareReferenceImporter"/> class.
        /// </summary>
        /// <param name="context">The metadata cloning workspace.</param>
        public CloneContextAwareReferenceImporter(MemberCloneContext context)
            : base(context.Module)
        {
            _context = context;
        }

        /// <inheritdoc />
        protected override ITypeDefOrRef ImportType(TypeDefinition type)
        {
            return _context.ClonedMembers.TryGetValue(type, out var clonedType)
                ? (ITypeDefOrRef) clonedType
                : base.ImportType(type);
        }

        /// <inheritdoc />
        public override IFieldDescriptor ImportField(IFieldDescriptor field)
        {
            return _context.ClonedMembers.TryGetValue(field, out var clonedField)
                ? (IFieldDescriptor) clonedField
                : base.ImportField(field);
        }

        /// <inheritdoc />
        public override IMethodDefOrRef ImportMethod(IMethodDefOrRef method)
        {
            return _context.ClonedMembers.TryGetValue(method, out var clonedMethod)
                ? (IMethodDefOrRef) clonedMethod
                : base.ImportMethod(method);
        }

        /// <inheritdoc />
        protected override ITypeDefOrRef ImportType(TypeReference type)
        {
            return type.Namespace == "System" && type.Name == nameof(System.Object)
                ? _context.Module.CorLibTypeFactory.Object.Type
                : base.ImportType(type);
        }
    }
}
