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
        PreserveTypeReferenceIndices = 0x10,

        /// <summary>
        /// Indicates indices into the type definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveTypeDefinitionIndices = 0x20,

        /// <summary>
        /// Indicates indices into the field definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveFieldDefinitionIndices = 0x40,

        /// <summary>
        /// Indicates indices into the method definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveMethodDefinitionIndices = 0x80,

        /// <summary>
        /// Indicates indices into the parameter definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveParameterDefinitionIndices = 0x100,

        /// <summary>
        /// Indicates indices into the member reference table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveMemberReferenceIndices = 0x200,

        /// <summary>
        /// Indicates indices into the stand-alone signature table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveStandAloneSignatureIndices = 0x400,

        /// <summary>
        /// Indicates indices into the event definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveEventDefinitionIndices = 0x800,

        /// <summary>
        /// Indicates indices into the property definition table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreservePropertyDefinitionIndices = 0x1000,

        /// <summary>
        /// Indicates indices into the module reference table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveModuleReferenceIndices = 0x2000,

        /// <summary>
        /// Indicates indices into the type specification table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveTypeSpecificationIndices = 0x4000,

        /// <summary>
        /// Indicates indices into the assembly reference table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveAssemblyReferenceIndices = 0x8000,

        /// <summary>
        /// Indicates indices into the method specification table should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveMethodSpecificationIndices = 0x10000,

        /// <summary>
        /// Indicates indices into the tables stream should be preserved whenever possible during the construction
        /// of the metadata directory.
        /// </summary>
        PreserveTableIndices = PreserveTypeReferenceIndices | PreserveTypeDefinitionIndices
            | PreserveFieldDefinitionIndices | PreserveMethodDefinitionIndices | PreserveParameterDefinitionIndices
            | PreserveMemberReferenceIndices | PreserveStandAloneSignatureIndices | PreserveEventDefinitionIndices
            | PreservePropertyDefinitionIndices | PreserveModuleReferenceIndices | PreserveTypeSpecificationIndices
            | PreserveAssemblyReferenceIndices | PreserveMethodSpecificationIndices,

        /// <summary>
        /// Indicates unconventional / spurious metadata streams present in the .NET metadata directory should be
        /// preserved when possible.
        /// </summary>
        PreserveUnknownStreams = 0x20000,

        /// <summary>
        /// Indicates unconventional metadata stream order in the .NET metadata directory should be preserved when
        /// possible.
        /// </summary>
        PreserveStreamOrder = 0x40000,

        /// <summary>
        /// Indicates any kind of index into a blob or tables stream, as well as unknown spurious metadata streams
        /// should be preserved whenever possible during the construction of the metadata directory.
        /// </summary>
        PreserveAll = PreserveBlobIndices | PreserveGuidIndices | PreserveStringIndices | PreserveUserStringIndices
                      | PreserveTableIndices | PreserveUnknownStreams | PreserveStreamOrder,

        /// <summary>
        /// <para>
        /// By default, AsmResolver will optimize the #Strings stream for size. This means that strings that are a
        /// suffix of another will not be added twice to the buffer, but rather will be referenced using an offset
        /// within the longer string. This avoids lots of duplication in the final stream.
        /// </para>
        /// <para>
        /// Setting this flag will disable this optimization.
        /// </para>
        /// </summary>
        NoStringsStreamOptimization = 0x20000,

        /// <summary>
        /// <para>
        /// By default, when adding two embedded resources to a file with identical contents, AsmResolver will not
        /// add the second copy of the data to the output file and instead reuse the first blob. This can drastically
        /// reduce the size of the final output file.
        /// </para>
        /// <para>
        /// While supported by the .NET runtime, some post-processors (e.g., obfuscators) may not work well with this
        /// or depend on individual resource items to be present. Setting this flag will disable this optimization.
        /// </para>
        /// </summary>
        NoResourceDataDeduplication = 0x40000,

        /// <summary>
        /// Setting this flag will force the builder to emit edit-and-continue metadata, even if it is not required.
        /// </summary>
        ForceEncMetadata = 0x80000
    }
}
