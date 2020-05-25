using System;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides members for describing the behaviour of the .NET directory builder.
    /// </summary>
    [Flags]
    public enum MetadataBuilderFlags
    {
        /// <summary>
        /// Indicates no special metadata builder flags.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Indicates indices into the #Blob stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveBlobIndices = 0x1,
        
        /// <summary>
        /// Indicates indices into the #GUID stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveGuidIndices = 0x2,
        
        /// <summary>
        /// Indicates indices into the #Strings stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveStringIndices = 0x4,
        
        /// <summary>
        /// Indicates indices into the #US stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveUserStringIndices = 0x8,
        
        /// <summary>
        /// Indicates indices into the type references table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveModuleDefinitionIndices = 0x10,
        
        /// <summary>
        /// Indicates indices into the type references table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveTypeReferenceIndices = 0x20,
        
        /// <summary>
        /// Indicates indices into the type definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveTypeDefinitionIndices = 0x40,
        
        /// <summary>
        /// Indicates indices into the field definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveFieldDefinitionIndices = 0x80,
        
        /// <summary>
        /// Indicates indices into the method definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveMethodDefinitionIndices = 0x100,
        
        /// <summary>
        /// Indicates indices into the parameter definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveParameterDefinitionIndices = 0x200,
        
        /// <summary>
        /// Indicates indices into the member reference table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveMemberReferenceIndices = 0x400,
        
        /// <summary>
        /// Indicates indices into the stand-alone signature table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveStandAloneSignatureIndices = 0x800,
        
        /// <summary>
        /// Indicates indices into the event definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveEventDefinitionIndices = 0x1000,
        
        /// <summary>
        /// Indicates indices into the property definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreservePropertyDefinitionIndices = 0x2000,
        
        /// <summary>
        /// Indicates indices into the module reference table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveModuleReferenceIndices = 0x4000,
        
        /// <summary>
        /// Indicates indices into the type specification table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveTypeSpecificationIndices = 0x8000,
        
        /// <summary>
        /// Indicates indices into the edit-and-continue log table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveEditAndContinueLogIndices = 0x10000,
        
        /// <summary>
        /// Indicates indices into the edit-and-continue map table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveEditAndContinueMapIndices = 0x20000,
        
        /// <summary>
        /// Indicates indices into the assembly definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveAssemblyDefinitionIndices = 0x40000,
        
        /// <summary>
        /// Indicates indices into the assembly reference table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveAssemblyReferenceIndices = 0x80000,
        
        /// <summary>
        /// Indicates indices into the file reference table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveFileReferenceIndices = 0x100000,
        
        /// <summary>
        /// Indicates indices into the exported type table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveExportedTypeIndices = 0x200000,
        
        /// <summary>
        /// Indicates indices into the manifest resource table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveManifestResourceIndices = 0x400000,
        
        /// <summary>
        /// Indicates indices into the method specification table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveMethodSpecificationIndices = 0x800000,
        
        /// <summary>
        /// Indicates indices into the tables stream should be preserved whenever possible during the construction
        /// of the metadata directory. 
        /// </summary>
        PreserveTableIndices = PreserveModuleDefinitionIndices | PreserveTypeReferenceIndices | PreserveTypeDefinitionIndices
            | PreserveFieldDefinitionIndices | PreserveMethodDefinitionIndices | PreserveParameterDefinitionIndices
            | PreserveMemberReferenceIndices | PreserveStandAloneSignatureIndices | PreserveEventDefinitionIndices
            | PreservePropertyDefinitionIndices | PreserveModuleReferenceIndices | PreserveTypeSpecificationIndices
            | PreserveEditAndContinueLogIndices | PreserveEditAndContinueMapIndices | PreserveAssemblyDefinitionIndices
            | PreserveAssemblyReferenceIndices | PreserveFileReferenceIndices | PreserveExportedTypeIndices
            | PreserveManifestResourceIndices | PreserveMethodSpecificationIndices,
        
        /// <summary>
        /// Indicates any kind of index into a blob or tables stream should be preserved whenever possible during the
        /// construction of the metadata directory. 
        /// </summary>
        PreserveAll = PreserveBlobIndices | PreserveGuidIndices | PreserveStringIndices | PreserveUserStringIndices 
                      | PreserveTableIndices,
    }
}