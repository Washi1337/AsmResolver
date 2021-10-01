namespace AsmResolver.DotNet
{
    /// <summary>
    /// Defines members for various errors that might occur during the parsing of a blob type signature.
    /// </summary>
    public enum InvalidTypeSignatureError
    {
        /// <summary>
        /// Indicates the blob signature was too short.
        /// </summary>
        BlobTooShort,

        /// <summary>
        /// Indicates a TypeDefOrRef coded index could not be decoded to a valid index.
        /// </summary>
        InvalidCodedIndex,

        /// <summary>
        /// Indicates there exists a reference loop between the metadata tables and the blob stream.
        /// </summary>
        MetadataLoop,

        /// <summary>
        /// Indicates a TypeDefOrRef coded index decoded to a TypeSpec, but is not allowed by the runtime to be one.
        /// </summary>
        IllegalTypeSpec,

        /// <summary>
        /// Indicates the type was parsed from an invalid value of a FieldOrPropType.
        /// </summary>
        InvalidFieldOrProptype,
    }
}
