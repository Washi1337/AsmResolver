using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Blob
{
    /// <summary>
    /// Provides a base for blob signatures that reference a type. 
    /// </summary>
    public abstract class TypeSignature : BlobSignature, ITypeDescriptor
    {
        /// <inheritdoc />
        public abstract string Name
        {
            get;
        }

        /// <inheritdoc />
        public abstract string Namespace
        {
            get;
        }

        /// <inheritdoc />
        public string FullName => this.GetFullName();

        /// <inheritdoc />
        public abstract IResolutionScope Scope
        {
            get;
        }

        /// <summary>
        /// Gets the element type of the 
        /// </summary>
        public abstract ElementType ElementType
        {
            get;
        }

        /// <inheritdoc />
        public ModuleDefinition Module => Scope.Module;
        
        /// <inheritdoc />
        public ITypeDescriptor DeclaringType => Scope as ITypeDescriptor;

        /// <inheritdoc />
        public override string ToString() => FullName;
        
    }
}