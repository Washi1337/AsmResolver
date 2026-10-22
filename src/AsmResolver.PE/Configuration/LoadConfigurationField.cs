namespace AsmResolver.PE.Configuration;

/// <summary>
/// Provides members describing all fields in a load configuration data directory.
/// </summary>
/// <remarks>
/// The integer value of each field corresponds to the index within the data directory.
/// </remarks>
public enum LoadConfigurationField : int
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Size = 0,
    TimeDateStamp = 1,
    MajorVersion = 2,
    MinorVersion = 3,
    GlobalFlagsClear = 4,
    GlobalFlagsSet = 5,
    CriticalSectionDefaultTimeout = 6,
    DeCommitFreeBlockThreshold = 7,
    DeCommitTotalFreeThreshold = 8,
    LockPrefixTable = 9,
    MaximumAllocationSize = 10,
    VirtualMemoryThreshold = 11,
    ProcessAffinityMask = 12,
    ProcessHeapFlags = 13,
    CSDVersion = 14,
    DependentLoadFlags = 15,
    EditList = 16,
    SecurityCookie = 17,
    SEHandlerTable = 18,
    SEHandlerCount = 19,
    GuardCFCheckFunctionPointer = 20,
    GuardCFDispatchFunctionPointer = 21,
    GuardCFFunctionTable = 22,
    GuardCFFunctionCount = 23,
    GuardFlags = 24,
    CodeIntegrityCatalog = 25,
    CodeIntegrityOffset = 26,
    CodeIntegrityFlags = 27,
    CodeIntegrityReserved = 28,
    GuardAddressTakenIatEntryTable = 29,
    GuardAddressTakenIatEntryCount = 30,
    GuardLongJumpTargetTable = 31,
    GuardLongJumpTargetCount = 32,
    DynamicValueRelocTable = 33,
    ChpeMetadataPointer = 34,
    GuardRFFailureRoutine = 35,
    GuardRFFailureRoutineFunctionPointer = 36,
    DynamicValueRelocTableOffset = 37,
    DynamicValueRelocTableSection = 38,
    Reserved2 = 39,
    GuardRFVerifyStackPointerFunctionPointer = 40,
    HotPatchTableOffset = 41,
    Reserved3 = 42,
    EnclaveConfigurationPointer = 43,
    VolatileMetadataPointer = 44,
    GuardEHContinuationTable = 45,
    GuardEHContinuationCount = 46,
    GuardXFGCheckFunctionPointer = 47,
    GuardXFGDispatchFunctionPointer = 48,
    GuardXFGTableDispatchFunctionPointer = 49,
    CastGuardOsDeterminedFailureMode = 50,
    GuardMemcpyFunctionPointer = 51,
    UmaFunctionPointers = 52,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
