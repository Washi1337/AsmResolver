namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that is either a method definition or a method reference, and can be referenced by a
    /// MethodDefOrRef coded index.
    /// </summary>
    public interface IMethodDefOrRef : ICustomAttributeType
    {
        /// <summary>
        /// Imports the method using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported method.</returns>
        new IMethodDefOrRef ImportWith(ReferenceImporter importer);
    }
}
