namespace AsmResolver
{
    /// <summary>
    /// Provides valid values for a base relocation type.
    /// </summary>
    public enum BaseRelocationType : byte
    {
        /// <summary>
        /// Indicates the fixup is skipped.
        /// </summary>
        Absolute,

        /// <summary>
        /// Indicates the fixup adds the high 16 bits of the delta to the 16-bit field at <see cref="BaseRelocationEntry.Offset"/>. 
        /// The 16-bit field represents the high value of a 32-bit word.
        /// </summary>
        High,

        /// <summary>
        /// Indicates the fixup adds the low 16 bits of the delta to the 16-bit field at <see cref="BaseRelocationEntry.Offset"/>. 
        /// The 16-bit field represents the low value of a 32-bit word. 
        /// </summary>
        Low,

        /// <summary>
        /// Indicates the fixup adds the delta to the 32-bit field at <see cref="BaseRelocationEntry.Offset"/>.
        /// </summary>
        HighLow,

        /// <summary>
        /// Indicates the fixup adds the high 16 bits of the delta to the 16-bit field at <see cref="BaseRelocationEntry.Offset"/>. 
        /// The 16-bit field represents the high value of a 32-bit word. The low 16 bits of the 32-bit value are stored in 
        /// the 16-bit word that follows this base relocation. This means that this base relocation occupies two slots.
        /// </summary>
        HighAdj,

        /// <summary>
        /// Indicates the fixup applies to a MIPS-specific jump instruction.
        /// </summary>
        MipsJmpAddr,

        /// <summary>
        /// Reserved.
        /// </summary>
        ArmMov32A,

        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        ArmMov32T,

        /// <summary>
        /// Indicates the fixup applies to a MIPS-specific 16-bit jump instruction.
        /// </summary>
        MipsJmpAddr16,

        /// <summary>
        /// Indicates the fixup applies the delta to the 64-bit field at <see cref="BaseRelocationEntry.Offset"/>/
        /// </summary>
        Dir64,
    }
}