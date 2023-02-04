using System;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members defining all possible flags that can be assigned to a procedure.
/// </summary>
[Flags]
public enum FrameProcedureAttributes : uint
{
    /// <summary>
    /// Indicates the function uses <c>_alloca()</c>.
    /// </summary>
    HasAlloca = 1 << 0,

    /// <summary>
    /// Indicates the function uses <c>setjmp()</c>.
    /// </summary>
    HasSetJmp = 1 << 1,

    /// <summary>
    /// Indicates the function uses <c>longjmp()</c>.
    /// </summary>
    HasLongJmp = 1 << 2,

    /// <summary>
    /// Indicates the function uses inline asm.
    /// </summary>
    HasInlAsm = 1 << 3,

    /// <summary>
    /// Indicates the function has EH states.
    /// </summary>
    HasEH = 1 << 4,

    /// <summary>
    /// Indicates the function was speced as inline.
    /// </summary>
    InlSpec = 1 << 5,

    /// <summary>
    /// Indicates the function has SEH.
    /// </summary>
    HasSEH = 1 << 6,

    /// <summary>
    /// Indicates the function is <c>__declspec(naked)</c>.
    /// </summary>
    Naked = 1 << 7,

    /// <summary>
    /// Indicates the function has buffer security check introduced by <c>/GS</c>.
    /// </summary>
    SecurityChecks = 1 << 8,

    /// <summary>
    /// Indicates the function is compiled with <c>/EHa</c>.
    /// </summary>
    AsyncEH = 1 << 9,

    /// <summary>
    /// Indicates the function has <c>/GS</c> buffer checks, but stack ordering couldn't be done.
    /// </summary>
    GSNoStackOrdering = 1 << 10,

    /// <summary>
    /// Indicates the function was inlined within another function.
    /// </summary>
    WasInlined = 1 << 11,

    /// <summary>
    /// Indicates the function is <c>__declspec(strict_gs_check)</c>.
    /// </summary>
    GSCheck = 1 << 12,

    /// <summary>
    /// Indicates the function is <c>__declspec(safebuffers)</c>.
    /// </summary>
    SafeBuffers = 1 << 13,

    /// <summary>
    /// Indicates the record function's local pointer explicitly.
    /// </summary>
    EncodedLocalBasePointerMask = (1 << 14) | (1 << 15),

    /// <summary>
    /// Indicates the record function's parameter pointer explicitly.
    /// </summary>
    EncodedParamBasePointerMask = (1 << 16) | (1 << 17),

    /// <summary>
    /// Indicates the function was compiled with PGO/PGU.
    /// </summary>
    PogoOn = 1 << 18,

    /// <summary>
    /// Indicates the symbol has valid POGO counts.
    /// </summary>
    ValidCounts = 1 << 19,

    /// <summary>
    /// Indicates the symbol was optimized for speed.
    /// </summary>
    OptSpeed = 1 << 20,

    /// <summary>
    /// Indicates the function contains CFG checks (and no write checks).
    /// </summary>
    GuardCF = 1 << 21,

    /// <summary>
    /// Indicates the function contains CFW checks and/or instrumentation.
    /// </summary>
    GuardCFW = 1 << 22,
}
