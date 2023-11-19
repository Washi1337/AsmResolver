namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides members defining all possible locations a native variable in a precompiled method can be placed at.
    /// </summary>
    public enum DebugInfoVariableLocationType
    {
        /// <summary>
        /// variable is in a register.
        /// </summary>
        Register,

        /// <summary>
        /// address of the variable is in a register.
        /// </summary>
        RegisterByReference,

        /// <summary>
        /// variable is in an fp register.
        /// </summary>
        RegisterFP,

        /// <summary>
        /// variable is on the stack (memory addressed relative to the frame-pointer).
        /// </summary>
        Stack,

        /// <summary>
        /// address of the variable is on the stack (memory addressed relative to the frame-pointer).
        /// </summary>
        StackByReference,

        /// <summary>
        /// variable lives in two registers.
        /// </summary>
        RegisterRegister,

        /// <summary>
        /// variable lives partly in a register and partly on the stack.
        /// </summary>
        RegisterStack,

        /// <summary>
        /// reverse of VLT_REG_STK.
        /// </summary>
        StackRegister,

        /// <summary>
        /// variable lives in two slots on the stack.
        /// </summary>
        Stack2,

        /// <summary>
        /// variable lives on the floating-point stack.
        /// </summary>
        FPStack,

        /// <summary>
        /// variable is a fixed argument in a varargs function (relative to VARARGS_HANDLE).
        /// </summary>
        FixedVA,
    }
}
