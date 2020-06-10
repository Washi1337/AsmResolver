namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Provides members describing the value type of a version resource entry. 
    /// </summary>
    public enum ResourceValueType : ushort
    {
        /// <summary>
        /// Indicates the value is binary.
        /// </summary>
        Binary = 0,
        
        /// <summary>
        /// Indicates the value is textual.
        /// </summary>
        String = 1,
    }
}