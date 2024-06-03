namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all operations that can be performed during an edit-and-continue session.
    /// </summary>
    public enum DeltaFunctionCode : uint
    {
        /// <summary>
        /// Indicates the default operation is applied.
        /// </summary>
        FuncDefault = 0,

        /// <summary>
        /// Indicates a method is being created.
        /// </summary>
        MethodCreate,

        /// <summary>
        /// Indicates a field is being created.
        /// </summary>
        FieldCreate,

        /// <summary>
        /// Indicates a parameter is being created.
        /// </summary>
        ParamCreate,

        /// <summary>
        /// Indicates a property is being created.
        /// </summary>
        PropertyCreate,

        /// <summary>
        /// Indicates an event is being created.
        /// </summary>
        EventCreate,
    };
}
