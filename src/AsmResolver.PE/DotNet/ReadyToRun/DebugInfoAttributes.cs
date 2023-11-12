using System;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides members defining all possible attributes that can be assigned to a single <see cref="DebugInfoBounds"/>
    /// entry.
    /// </summary>
    [Flags]
    public enum DebugInfoAttributes
    {
        /// <summary>
        /// Indicates that no other options apply
        /// </summary>
        SourceTypeInvalid = 0x00,

        /// <summary>
        /// Indicates the debugger asked for it.
        /// </summary>
        SequencePoint = 0x01,

        /// <summary>
        /// Indicates the stack is empty here.
        /// </summary>
        StackEmpty = 0x02,

        /// <summary>
        /// Indicates this is a call site.
        /// </summary>
        CallSite = 0x04,

        /// <summary>
        /// Indicates an epilog endpoint.
        /// </summary>
        NativeEndOffsetUnknown = 0x08,

        /// <summary>
        /// Indicates the actual instruction of a call.
        /// </summary>
        CallInstruction = 0x10
    }
}
