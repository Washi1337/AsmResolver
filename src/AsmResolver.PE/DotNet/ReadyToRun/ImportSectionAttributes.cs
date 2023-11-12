using System;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides members describing all possible attributes that can be attached to a <see cref="ImportSection"/>.
    /// </summary>
    [Flags]
    public enum ImportSectionAttributes : ushort
    {
        /// <summary>
        /// Indicates no special attributes were assigned.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates the slots in the section have to be initialized at image load time.
        /// </summary>
        Eager = 1,

        /// <summary>
        /// Indicates the slots contain pointers to code.
        /// </summary>
        PCode = 4
    }
}
