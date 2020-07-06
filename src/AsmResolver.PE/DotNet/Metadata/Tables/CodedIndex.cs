namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all coded indices that can appear in the tables stream.
    /// </summary>
    public enum CodedIndex
    {
        /// <summary>
        /// Indicates the index is an index to a member in either the TypeRef, TypeDef or TypeSpec table.
        /// </summary>
        TypeDefOrRef = 45,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the Field, Parameter or Property table.
        /// </summary>
        HasConstant,
        
        /// <summary>
        /// Indicates the index is an index to a member in one of the following tables:
        /// MethodDef, Field, TypeRef, TypeDef, Parameter, InterfaceImpl, MemberRef, Module, DeclSecurity, Property, Event,
        /// StandAloneSig, ModuleRef, TypeSpec, Assembly, AssemblyRef, File, ExportedType, ManifestResource, GenericParam,
        /// GenericParamConstraint or MethodSpec.
        /// </summary>
        HasCustomAttribute,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the Field or Parameter table.
        /// </summary>
        HasFieldMarshal,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the TypeDef, MethodDef or Assembly table.
        /// </summary>
        HasDeclSecurity,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the TypeDef, TypeRef, ModuleRef, MethodDef or TypeSpec
        /// table.
        /// </summary>
        MemberRefParent,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the Event or Property table.
        /// </summary>
        HasSemantics,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the MethodDef or MemberRef table.
        /// </summary>
        MethodDefOrRef,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the Field or MethodDef table.
        /// </summary>
        MemberForwarded,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the File, AssemblyRef or ExportedType table.
        /// </summary>
        Implementation,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the MethodDef or MemberRef table. 
        /// </summary>
        CustomAttributeType,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the Module, ModuleRef, AssemblyRef or TypeRef table.
        /// </summary>
        ResolutionScope,
        
        /// <summary>
        /// Indicates the index is an index to a member in either the TypeDef or MethodDef table.
        /// </summary>
        TypeOrMethodDef,
    }
    
}