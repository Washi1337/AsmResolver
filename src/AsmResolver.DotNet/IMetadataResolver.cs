namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for resolving references to members defined in external .NET assemblies.  
    /// </summary>
    public interface IMetadataResolver
    {
        /// <summary>
        /// Gets the object responsible for the resolution of external assemblies.
        /// </summary>
        IAssemblyResolver AssemblyResolver
        {
            get;
        }
        
        /// <summary>
        /// Resolves a reference to a type.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The type definition, or <c>null</c> if the type could not be resolved.</returns>
        TypeDefinition ResolveType(ITypeDescriptor type);

        /// <summary>
        /// Resolves a reference to a method.
        /// </summary>
        /// <param name="method">The method. to resolve.</param>
        /// <returns>The method definition, or <c>null</c> if the method could not be resolved.</returns>
        MethodDefinition ResolveMethod(IMethodDescriptor method);


        /// <summary>
        /// Resolves a reference to a field.
        /// </summary>
        /// <param name="field">The field to resolve.</param>
        /// <returns>The field definition, or <c>null</c> if the field could not be resolved.</returns>
        FieldDefinition ResolveField(IFieldDescriptor field);
    }
}