namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides members describing all possible import section types.
    /// </summary>
    public enum ImportSectionType : byte
    {
        /// <summary>
        /// Indicates the type of the section was unspecified.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates the section contains dispatcher stubs.
        /// </summary>
        StubDispatch = 2,

        /// <summary>
        /// Indicates the section contains string handles.
        /// </summary>
        StringHandle = 3,

        /// <summary>
        /// Indicates the section contains IL body fixups.
        /// </summary>
        ILBodyFixups = 7
    }
}
