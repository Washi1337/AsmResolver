namespace AsmResolver.X86
{
    // Reference: http://ref.x86asm.net/

    public enum X86OperandType
    {
        None,
        DirectAddress,
        MemoryAddress,
        ControlRegister,
        DebugRegister,
        RegisterOrMemoryAddress,
        StackRegisterOrMemoryAddress,
        StackRegister,
        Register,
        ImmediateData,
        RelativeOffset,
        SegmentRegister,
        OpCodeRegister,
        RegisterEax,
        RegisterAl,
        ImmediateOne,
        RegisterCl,
        RegisterDx
    }
}