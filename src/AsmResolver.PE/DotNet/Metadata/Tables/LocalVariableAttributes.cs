using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all possible flags that can be assigned to a local variable.
    /// </summary>
    [Flags]
    public enum LocalVariableAttributes : ushort
    {
        /// <summary>
        /// Indicates the local variable should be hidden in a debugger view.
        /// </summary>
        DebuggerHidden = 0x0001
    }
}
