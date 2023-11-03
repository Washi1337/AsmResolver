namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides members describing all available helper calls for a single <see cref="ReadyToRunFixupKind.Helper"/> fixup.
    /// </summary>
    public enum ReadyToRunHelper : uint
    {
        // Reference: https://github.com/dotnet/runtime/blob/aac9923eb6b94d24232cf87d37c8aaccef5fff93/src/coreclr/inc/readytorun.h#L284
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Invalid = 0x00,

        Module = 0x01,
        GSCookie = 0x02,
        IndirectTrapThreads = 0x03,

        //
        // Delay load helpers
        //

        // All delay load helpers use custom calling convention:
        // - scratch register - address of indirection cell. 0 = address is inferred from callsite.
        // - stack - section index, module handle
        DelayLoadMethodCall = 0x08,

        DelayLoadHelper = 0x10,
        DelayLoadHelperObj = 0x11,
        DelayLoadHelperObjObj = 0x12,

        // JIT helpers

        // Exception handling helpers
        Throw = 0x20,
        Rethrow = 0x21,
        Overflow = 0x22,
        RngChkFail = 0x23,
        FailFast = 0x24,
        ThrowNullRef = 0x25,
        ThrowDivZero = 0x26,

        // Write barriers
        WriteBarrier = 0x30,
        CheckedWriteBarrier = 0x31,
        ByRefWriteBarrier = 0x32,

        // Array helpers
        StelemRef = 0x38,
        LdelemaRef = 0x39,

        MemSet = 0x40,
        MemCpy = 0x41,

        // PInvoke helpers
        PInvokeBegin = 0x42,
        PInvokeEnd = 0x43,
        GCPoll = 0x44,
        ReversePInvokeEnter = 0x45,
        ReversePInvokeExit = 0x46,

        // Get string handle lazily
        GetString = 0x50,

        // Used by /Tuning for Profile optimizations
        LogMethodEnter = 0x51,

        // Reflection helpers
        GetRuntimeTypeHandle = 0x54,
        GetRuntimeMethodHandle = 0x55,
        GetRuntimeFieldHandle = 0x56,

        Box = 0x58,
        BoxNullable = 0x59,
        Unbox = 0x5A,
        UnboxNullable = 0x5B,
        NewMultiDimArr = 0x5C,

        // Helpers used with generic handle lookup cases
        NewObject = 0x60,
        NewArray = 0x61,
        CheckCastAny = 0x62,
        CheckInstanceAny = 0x63,
        GenericGcStaticBase = 0x64,
        GenericNonGcStaticBase = 0x65,
        GenericGcTlsBase = 0x66,
        GenericNonGcTlsBase = 0x67,
        VirtualFuncPtr = 0x68,
        IsInstanceOfException = 0x69,
        NewMaybeFrozenArray = 0x6A,
        NewMaybeFrozenObject = 0x6B,

        // Long mul/div/shift ops
        LMul = 0xC0,
        LMulOfv = 0xC1,
        ULMulOvf = 0xC2,
        LDiv = 0xC3,
        LMod = 0xC4,
        ULDiv = 0xC5,
        ULMod = 0xC6,
        LLsh = 0xC7,
        LRsh = 0xC8,
        LRsz = 0xC9,
        Lng2Dbl = 0xCA,
        ULng2Dbl = 0xCB,

        // 32-bit division helpers
        Div = 0xCC,
        Mod = 0xCD,
        UDiv = 0xCE,
        UMod = 0xCF,

        // Floating point conversions
        Dbl2Int = 0xD0,
        Dbl2IntOvf = 0xD1,
        Dbl2Lng = 0xD2,
        Dbl2LngOvf = 0xD3,
        Dbl2UInt = 0xD4,
        Dbl2UIntOvf = 0xD5,
        Dbl2ULng = 0xD6,
        Dbl2ULngOvf = 0xD7,

        // Floating point ops
        DblRem = 0xE0,
        FltRem = 0xE1,
        DblRound = 0xE2,
        FltRound = 0xE3,

        // Personality routines
        PersonalityRoutine = 0xF0,
        PersonalityRoutineFilterFunclet = 0xF1,

        // Synchronized methods
        MonitorEnter = 0xF8,
        MonitorExit = 0xF9,

        //
        // Deprecated/legacy
        //

        // JIT32 x86-specific write barriers
        WriteBarrierEax = 0x100,
        WriteBarrierEbx = 0x101,
        WriteBarrierEcx = 0x102,
        WriteBarrierEsi = 0x103,
        WriteBarrierEdi = 0x104,
        WriteBarrierEbp = 0x105,
        CheckedWriteBarrierEax = 0x106,
        CheckedWriteBarrierEbx = 0x107,
        CheckedWriteBarrierEcx = 0x108,
        CheckedWriteBarrierEsi = 0x109,
        CheckedWriteBarrierEdi = 0x10A,
        CheckedWriteBarrierEbp = 0x10B,

        // JIT32 x86-specific exception handling
        EndCatch = 0x110,

        // Stack probing helper
        StackProbe = 0x111,

        GetCurrentManagedThreadId = 0x112,

        // Array helpers for use with native ints
        StelemRefI = 0x113,
        LdelemaRefI = 0x114,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
