using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Configuration;

/// <summary>
/// Defines options for the Windows PE loader.
/// </summary>
public partial class LoadConfiguration : SegmentBase, IRelocatable
{
    private static readonly uint[] FieldOffsets32 =
    [
        0x00,  // Size
        0x04,  // TimeDateStamp
        0x08,  // MajorVersion
        0x0a,  // MinorVersion
        0x0c,  // GlobalFlagsClear
        0x10,  // GlobalFlagsSet
        0x14,  // CriticalSectionDefaultTimeout
        0x18,  // DeCommitFreeBlockThreshold
        0x1c,  // DeCommitTotalFreeThreshold
        0x20,  // LockPrefixTable
        0x24,  // MaximumAllocationSize
        0x28,  // VirtualMemoryThreshold
        0x2c,  // ProcessHeapFlags
        0x30,  // ProcessAffinityMask
        0x34,  // CSDVersion
        0x36,  // DependentLoadFlags
        0x38,  // EditList
        0x3c,  // SecurityCookie
        0x40,  // SEHandlerTable
        0x44,  // SEHandlerCount
        0x48,  // GuardCFCheckFunctionPointer
        0x4c,  // GuardCFDispatchFunctionPointer
        0x50,  // GuardCFFunctionTable
        0x54,  // GuardCFFunctionCount
        0x58,  // GuardFlags
        0x5c,  // CodeIntegrityCatalog
        0x5e,  // CodeIntegrityOffset
        0x62,  // CodeIntegrityFlags
        0x64,  // CodeIntegrityReserved
        0x68,  // GuardAddressTakenIatEntryTable
        0x6c,  // GuardAddressTakenIatEntryCount
        0x70,  // GuardLongJumpTargetTable
        0x74,  // GuardLongJumpTargetCount
        0x78,  // DynamicValueRelocTable
        0x7c,  // CHPEMetadataPointer
        0x80,  // GuardRFFailureRoutine
        0x84,  // GuardRFFailureRoutineFunctionPointer
        0x88,  // DynamicValueRelocTableOffset
        0x8c,  // DynamicValueRelocTableSection
        0x8e,  // Reserved2
        0x90,  // GuardRFVerifyStackPointerFunctionPointer
        0x94,  // HotPatchTableOffset
        0x98,  // Reserved3
        0x9c,  // EnclaveConfigurationPointer
        0xa0,  // VolatileMetadataPointer
        0xa4,  // GuardEHContinuationTable
        0xa8,  // GuardEHContinuationCount
        0xac,  // GuardXFGCheckFunctionPointer
        0xb0,  // GuardXFGDispatchFunctionPointer
        0xb4,  // GuardXFGTableDispatchFunctionPointer
        0xb8,  // CastGuardOsDeterminedFailureMode
        0xbc,  // GuardMemcpyFunctionPointer
        0xc0,  // UmaFunctionPointers
        0xc4,  // <end>
    ];

    private static readonly uint[] FieldOffsets64 =
    [
        0x00,  // Size
        0x04,  // TimeDateStamp
        0x08,  // MajorVersion
        0x0a,  // MinorVersion
        0x0c,  // GlobalFlagsClear
        0x10,  // GlobalFlagsSet
        0x14,  // CriticalSectionDefaultTimeout
        0x18,  // DeCommitFreeBlockThreshold
        0x20,  // DeCommitTotalFreeThreshold
        0x28,  // LockPrefixTable
        0x30,  // MaximumAllocationSize
        0x38,  // VirtualMemoryThreshold
        0x40,  // ProcessAffinityMask
        0x48,  // ProcessHeapFlags
        0x4c,  // CSDVersion
        0x4e,  // DependentLoadFlags
        0x50,  // EditList
        0x58,  // SecurityCookie
        0x60,  // SEHandlerTable
        0x68,  // SEHandlerCount
        0x70,  // GuardCFCheckFunctionPointer
        0x78,  // GuardCFDispatchFunctionPointer
        0x80,  // GuardCFFunctionTable
        0x88,  // GuardCFFunctionCount
        0x90,  // GuardFlags
        0x94,  // CodeIntegrityCatalog
        0x96,  // CodeIntegrityOffset
        0x9a,  // CodeIntegrityFlags
        0x9c,  // CodeIntegrityReserved
        0xa0,  // GuardAddressTakenIatEntryTable
        0xa8,  // GuardAddressTakenIatEntryCount
        0xb0,  // GuardLongJumpTargetTable
        0xb8,  // GuardLongJumpTargetCount
        0xc0,  // DynamicValueRelocTable
        0xc8,  // CHPEMetadataPointer
        0xd0,  // GuardRFFailureRoutine
        0xd8,  // GuardRFFailureRoutineFunctionPointer
        0xe0,  // DynamicValueRelocTableOffset
        0xe4,  // DynamicValueRelocTableSection
        0xe6,  // Reserved2
        0xe8,  // GuardRFVerifyStackPointerFunctionPointer
        0xf0,  // HotPatchTableOffset
        0xf4,  // Reserved3
        0xf8,  // EnclaveConfigurationPointer
        0x100, // VolatileMetadataPointer
        0x108, // GuardEHContinuationTable
        0x110, // GuardEHContinuationCount
        0x118, // GuardXFGCheckFunctionPointer
        0x120, // GuardXFGDispatchFunctionPointer
        0x128, // GuardXFGTableDispatchFunctionPointer
        0x130, // CastGuardOsDeterminedFailureMode
        0x138, // GuardMemcpyFunctionPointer
        0x140, // UmaFunctionPointers
        0x148, // <end>
    ];

    private ulong _imageBase;

    /// <summary>
    /// Creates a new empty minimal 32-bit load configuration.
    /// </summary>
    public LoadConfiguration()
        : this(LoadConfigurationField.SecurityCookie, true)
    {
    }

    /// <summary>
    /// Creates a new empty load configuration with the provided size.
    /// </summary>
    /// <param name="lastIncludedField">Specifies the last field that is to be included in the new configuration.</param>
    /// <param name="is32Bit"><c>true</c> if a 32-bit directory is to be created, <c>false</c> for 64-bit.</param>
    public LoadConfiguration(LoadConfigurationField lastIncludedField, bool is32Bit = true)
    {
        Is32Bit = is32Bit;
        Size = GetFieldEndOffset(lastIncludedField);
    }

    /// <summary>
    /// Creates a new empty load configuration with the provided size.
    /// </summary>
    /// <param name="is32Bit"><c>true</c> if a 32-bit directory is to be created, <c>false</c> for 64-bit.</param>
    /// <param name="size">The size of the configuration</param>
    public LoadConfiguration(uint size, bool is32Bit)
    {
        Is32Bit = is32Bit;
        Size = size;
    }

    /// <summary>
    /// Gets a value indicating whether this configuration directory is using the 32-bit or 64-bit format.
    /// </summary>
    public bool Is32Bit
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the size in bytes of the load configuration that is persisted.
    /// </summary>
    /// <remarks>
    /// This value determines which fields are included in the configuration.
    /// Use <see cref="HasField"/> and <see cref="GetFieldOffset"/> to inspect which fields are included based on
    /// this value.
    /// </remarks>
    public uint Size
    {
        get;
        set;
    }

    /// <summary>
    /// Date and time stamp value. The value is represented in the number of seconds that have elapsed since midnight
    /// (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock. The time stamp can be
    /// printed by using the C runtime (CRT) time function.
    /// </summary>
    public uint TimeDateStamp
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the major version number of the loader configuration data.
    /// </summary>
    public ushort MajorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minor version number of the loader configuration data.
    /// </summary>
    public ushort MinorVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the global loader flags to clear for this process as the loader starts the process.
    /// </summary>
    public uint GlobalFlagsClear
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the global loader flags to set for this process as the loader starts the process.
    /// </summary>
    public uint GlobalFlagsSet
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the default timeout value to use for this process's critical sections that are abandoned.
    /// </summary>
    public uint CriticalSectionDefaultTimeout
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the amount of memory that must be freed before it is returned to the system, in bytes.
    /// </summary>
    /// <remarks>On 32-bit platforms, this field is restricted to 32-bit sizes (max 4GB).</remarks>
    public ulong DeCommitFreeBlockThreshold
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the total amount of free memory, in bytes.
    /// </summary>
    /// <remarks>On 32-bit platforms, this field is restricted to 32-bit sizes (max 4GB).</remarks>
    public ulong DeCommitTotalFreeThreshold
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a list of addresses where the LOCK prefix is used so that they can be replaced with NOP on single processor machines.
    /// </summary>
    /// <remarks>This field is only used in x86 PE images.</remarks>
    [LazyProperty]
    public partial ReferenceTable LockPrefixTable
    {
        get;
    }

    /// <summary>
    /// Gets or sets the maximum allocation size, in bytes.
    /// </summary>
    /// <remarks>On 32-bit platforms, this field is restricted to 32-bit sizes (max 4GB).</remarks>
    public ulong MaximumAllocationSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the maximum virtual memory size, in bytes.
    /// </summary>
    /// <remarks>On 32-bit platforms, this field is restricted to 32-bit sizes (max 4GB).</remarks>
    public ulong VirtualMemoryThreshold
    {
        get;
        set;
    }

    /// <summary>
    /// When non-zero, gets or sets the affinity mask during process startup (executables only).
    /// </summary>
    /// <remarks>On 32-bit platforms, this field is restricted to 32-bit sizes.</remarks>
    public ulong ProcessAffinityMask
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the process heap flags that correspond to the first argument of the <c>HeapCreate</c> function.
    /// These flags apply to the process heap that is created during process startup.
    /// </summary>
    public uint ProcessHeapFlags
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the service pack version identifier.
    /// </summary>
    public ushort CsdVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the default load flags used when the operating system resolves the statically linked imports
    /// of a module.
    /// </summary>
    public ushort DependentLoadFlags
    {
        get;
        set;
    }

    /// <summary>
    /// Reserved for use by the system.
    /// </summary>
    public ISegmentReference EditList
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the cookie that is used by Visual C++ or GS implementation.
    /// </summary>
    /// <remarks>For valid PEs, this is always either pointing to a 32-bits or a 64-bits cookie.</remarks>
    [LazyProperty]
    public partial ISegment? SecurityCookie
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the sorted table of RVAs of each valid, unique SE handler in the image.
    /// </summary>
    /// <remarks>This field is only used in x86 PE images.</remarks>
    [LazyProperty]
    public partial ReferenceTable SEHandlerTable
    {
        get;
    }

    /// <summary>
    /// Gets or sets the address where the Control Flow Guard check-function pointer is stored.
    /// </summary>
    public ISegmentReference GuardCFCheckFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the address where the Control Flow Guard dispatch-function pointer is stored.
    /// </summary>
    public ISegmentReference GuardCFDispatchFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the sorted table of RVAs of each Control Flow Guard function in the image.
    /// </summary>
    [LazyProperty]
    public partial ControlFlowGuardFunctionTable GuardCFFunctionTable
    {
        get;
    }

    /// <summary>
    /// Gets or sets Control Flow Guard related flags.
    /// </summary>
    public GuardFlags GuardFlags
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets flags regarding code integrity encofrement.
    /// </summary>
    public ushort CodeIntegrityFlags
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the catalog index or ID.
    /// </summary>
    /// <remarks>This value is set to <c>0xFFFF</c> if unavailable.</remarks>
    public ushort CodeIntegrityCatalog
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset to the catalog location.
    /// </summary>
    public uint CodeIntegrityOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Reserved for future OS/bitmask expansion
    /// </summary>
    public uint CodeIntegrityReserved
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the Control Flow Guard address taken IAT table.
    /// </summary>
    [LazyProperty]
    public partial ControlFlowGuardFunctionTable GuardAddressTakenIatEntryTable
    {
        get;
    }

    /// <summary>
    /// Gets or sets the Control Flow Guard long jump target table.
    /// </summary>
    [LazyProperty]
    public partial ControlFlowGuardFunctionTable GuardLongJumpTargetTable
    {
        get;
    }

    /// <summary>
    /// Gets or sets a reference to the extended dynamic relocation table used for security features like
    /// DVRT / Retpoline / Shadow Stacks / Address Space Layout Randomization (ASLR).
    /// </summary>
    public ISegmentReference DynamicValueRelocTable
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets a reference to Compiled Hybrid Portable Executable (CHPE) metadata for ARM64EC/ARM64X emulation.
    /// </summary>
    public ISegmentReference ChpeMetadataPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets a reference to the Return Flow Guard (RFG) failure handler routine.
    /// </summary>
    public ISegmentReference GuardRFFailureRoutine
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets a reference to the pointer variable holding the RFG failure routine.
    /// </summary>
    public ISegmentReference GuardRFFailureRoutineFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the byte offset relative to the beginning of the section specified by
    /// <see cref="DynamicValueRelocTableSection"/> where the dynamic relocation table starts.
    /// </summary>
    public uint DynamicValueRelocTableOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the 1-based index of the PE section that contains the dynamic value table.
    /// </summary>
    public ushort DynamicValueRelocTableSection
    {
        get;
        set;
    }

    /// <summary>
    /// Reserved
    /// </summary>
    public ushort Reserved2
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a reference to the function that verifies stack integrity under RFG.
    /// </summary>
    public ISegmentReference GuardRFVerifyStackPointerFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the location of the binary's Hotpatch Header / Table, relative to the beginning of the
    /// load configuration directory.
    /// </summary>
    public uint HotPatchTableOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Reserved
    /// </summary>
    public uint Reserved3
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the reference to the Intel SGX / VBS Enclave configuration directory.
    /// </summary>
    public ISegmentReference EnclaveConfigurationPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the reference to metadata describing volatile code/data regions for hot-patching.
    /// </summary>
    public ISegmentReference VolatileMetadataPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets a table of valid continuation targets for Exception Handling (XFG / EH Continuation Guard,
    /// protecting exception return points like catch blocks from ROP attacks).
    /// </summary>
    [LazyProperty]
    public partial ControlFlowGuardFunctionTable GuardEHContinuationTable
    {
        get;
    }

    /// <summary>
    /// Gets or sets the reference to the Extended Flow Guard (XFG) type-checking function pointer.
    /// </summary>
    public ISegmentReference GuardXFGCheckFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the reference to the XFG dispatch function pointer.
    /// </summary>
    public ISegmentReference GuardXFGDispatchFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the reference to the XFG table-based dispatch function pointer.
    /// </summary>
    public ISegmentReference GuardXFGTableDispatchFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets a reference to the variable that holds a bitmask/enum dictating how the runtime reacts when an
    /// invalid cast is detected.
    /// </summary>
    public ISegmentReference CastGuardOsDeterminedFailureMode
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the reference to the pointer that points to a high-performance, guarded memcpy routine provided
    /// by the OS.
    /// </summary>
    public ISegmentReference GuardMemcpyFunctionPointer
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Gets or sets the reference to the pointer to User-Mode Application / User-Mode Execution pointers
    /// (reserved for specialized OS runtime hooks).
    /// </summary>
    public ISegmentReference UmaFunctionPointers
    {
        get;
        set;
    } = SegmentReference.Null;

    /// <summary>
    /// Obtains the lock prefix table.
    /// </summary>
    /// <returns>The lock prefix table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="LockPrefixTable"/> property.
    /// </remarks>
    protected virtual ReferenceTable GetLockPrefixTable()
    {
        return new ReferenceTable(ReferenceTableAttributes.Va | ReferenceTableAttributes.ZeroTerminated);
    }

    /// <summary>
    /// Obtains the security cookie table.
    /// </summary>
    /// <returns>The cookie.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SecurityCookie"/> property.
    /// </remarks>
    protected virtual ISegment? GetSecurityCookie() => null;

    /// <summary>
    /// Obtains the SEH handler table.
    /// </summary>
    /// <returns>The function table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SEHandlerTable"/> property.
    /// </remarks>
    protected virtual ReferenceTable GetSEHandlerTable()
    {
        return new ReferenceTable(ReferenceTableAttributes.Rva | ReferenceTableAttributes.Force32Bit);
    }

    /// <summary>
    /// Obtains the Guard CF function table.
    /// </summary>
    /// <returns>The function table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="GuardCFFunctionTable"/> property.
    /// </remarks>
    protected virtual ControlFlowGuardFunctionTable GetGuardCFFunctionTable() => new(this);

    /// <summary>
    /// Obtains the control flow guard IAT function table.
    /// </summary>
    /// <returns>The function table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="GuardAddressTakenIatEntryTable"/> property.
    /// </remarks>
    protected virtual ControlFlowGuardFunctionTable GetGuardAddressTakenIatEntryTable() => new(this);

    /// <summary>
    /// Obtains the control flow guard jump target table.
    /// </summary>
    /// <returns>The jump target table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="GuardLongJumpTargetTable"/> property.
    /// </remarks>
    protected virtual ControlFlowGuardFunctionTable GetGuardLongJumpTargetTable() => new(this);

    /// <summary>
    /// Obtains the control flow guard continuation table.
    /// </summary>
    /// <returns>The table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="GuardEHContinuationTable"/> property.
    /// </remarks>
    protected virtual ControlFlowGuardFunctionTable GetGuardEHContinuationTable() => new(this);

    /// <inheritdoc />
    public override void UpdateOffsets(in RelocationParameters parameters)
    {
        // Auto adjust size property.
        if (Is32Bit != parameters.Is32Bit)
        {
            (uint[] sourceOffsets, uint[] targetOffsets) = Is32Bit
                ? (FieldOffsets32, FieldOffsets64)
                : (FieldOffsets64, FieldOffsets32);

            int fieldIndex = GetLastFittingFieldIndex(sourceOffsets);
            uint extraPadding = Size - sourceOffsets[fieldIndex];
            Size = targetOffsets[fieldIndex] + extraPadding;
        }

        Is32Bit = parameters.Is32Bit;
        _imageBase = parameters.ImageBase;

        // Propagate image-base and bitness to VA tables, but preserve its offset/rva (it's not part of the directory itself).
        _lockPrefixTable?.UpdateOffsets(parameters.WithOffsetRva(_lockPrefixTable.Offset, _lockPrefixTable.Rva));

        base.UpdateOffsets(in parameters);
    }

    private uint[] GetFieldOffsets() => Is32Bit ? FieldOffsets32 : FieldOffsets64;

    /// <summary>
    /// Gets the offset of the provided field, relative to the start of the directory.
    /// </summary>
    /// <param name="field">The field</param>
    /// <returns>The offset</returns>
    public uint GetFieldOffset(LoadConfigurationField field) => GetFieldOffsets()[(int) field];

    /// <summary>
    /// Gets the end offset of the provided field, relative to the start of the directory.
    /// </summary>
    /// <param name="field">The field</param>
    /// <returns>The end offset</returns>
    public uint GetFieldEndOffset(LoadConfigurationField field) => GetFieldOffset(field + 1);

    /// <summary>
    /// Gets a value indicating whether the field is captured by the load configuration based on its <see cref="Size"/>.
    /// </summary>
    /// <param name="field">The field</param>
    /// <returns><c>true</c> if the field was defined by this configuration, <c>false</c> otherwise.</returns>
    public bool HasField(LoadConfigurationField field) => GetFieldOffset(field + 1) <= Size;

    /// <summary>
    /// Ensures the directory <see cref="Size"/> is large enough to include the provided field in the configuration.
    /// </summary>
    /// <param name="field">The field to include.</param>
    public void EnsureFieldIsIncluded(LoadConfigurationField field)
    {
        Size = Math.Max(Size, GetFieldEndOffset(field));
    }

    /// <summary>
    /// Resizes the directory to include exactly up to the provided field in the configuration.
    /// </summary>
    /// <param name="lastIncludedField">The last field to include in the configuration.</param>
    public void Resize(LoadConfigurationField lastIncludedField)
    {
        Size = GetFieldEndOffset(lastIncludedField);
    }

    private int GetLastFittingFieldIndex(uint[] fieldOffsets)
    {
        uint size = Size;

        for (int i = 0; i < fieldOffsets.Length; i++)
        {
            if (size < fieldOffsets[i])
                return i - 1;
        }

        return fieldOffsets.Length - 1;
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize() => Size;

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer)
    {
        if (Size <= sizeof(uint))
            throw new ArgumentException("Size of loader configuration is below 4 bytes");

        uint totalLength = 0;
        bool @continue =
            TryWriteUInt32(writer, Size, ref totalLength)
            && TryWriteUInt32(writer, TimeDateStamp, ref totalLength)
            && TryWriteUInt16(writer, MajorVersion, ref totalLength)
            && TryWriteUInt16(writer, MinorVersion, ref totalLength)
            && TryWriteUInt32(writer, GlobalFlagsClear, ref totalLength)
            && TryWriteUInt32(writer, GlobalFlagsSet, ref totalLength)
            && TryWriteUInt32(writer, CriticalSectionDefaultTimeout, ref totalLength)
            && TryWriteNativeInt(writer, DeCommitFreeBlockThreshold, ref totalLength)
            && TryWriteNativeInt(writer, DeCommitTotalFreeThreshold, ref totalLength)
            && TryWriteNativeInt(writer, LockPrefixTable.Count > 0 ? _imageBase + LockPrefixTable.Rva : 0ul, ref totalLength)
            && TryWriteNativeInt(writer, MaximumAllocationSize, ref totalLength)
            && TryWriteNativeInt(writer, VirtualMemoryThreshold, ref totalLength)
            ;

        if (!@continue)
            return;

        // 32-bits/64-bits swap for some reason process affinity mask and heap flags.
        if (Is32Bit)
        {
            @continue = TryWriteUInt32(writer, ProcessHeapFlags, ref totalLength)
                && TryWriteUInt32(writer, (uint) ProcessAffinityMask , ref totalLength);
        }
        else
        {
            @continue = TryWriteUInt64(writer, ProcessAffinityMask, ref totalLength)
                && TryWriteUInt32(writer, ProcessHeapFlags, ref totalLength);
        }

        if (!@continue)
            return;

        @continue = TryWriteUInt16(writer, CsdVersion, ref totalLength)
            && TryWriteUInt16(writer, DependentLoadFlags, ref totalLength)
            && TryWriteVa(writer, EditList, ref totalLength)
            && TryWriteNativeInt(writer, _imageBase + SecurityCookie?.Rva ?? 0ul, ref totalLength)
            && TryWriteNativeInt(writer, SEHandlerTable.Count > 0 ? _imageBase + SEHandlerTable.Rva : 0ul, ref totalLength)
            && TryWriteNativeInt(writer, (ulong) SEHandlerTable.Count, ref totalLength)
            && TryWriteVa(writer, GuardCFCheckFunctionPointer, ref totalLength)
            && TryWriteVa(writer, GuardCFDispatchFunctionPointer, ref totalLength)
            && TryWriteNativeInt(writer, GuardCFFunctionTable.Count > 0 ? _imageBase + GuardCFFunctionTable.Rva : 0ul, ref totalLength)
            && TryWriteNativeInt(writer, (ulong) GuardCFFunctionTable.Count, ref totalLength)
            && TryWriteUInt32(writer, (uint) GuardFlags, ref totalLength)
            && TryWriteUInt16(writer, CodeIntegrityFlags, ref totalLength)
            && TryWriteUInt16(writer, CodeIntegrityCatalog, ref totalLength)
            && TryWriteUInt32(writer, CodeIntegrityOffset, ref totalLength)
            && TryWriteUInt32(writer, CodeIntegrityReserved, ref totalLength)
            && TryWriteNativeInt(writer, GuardAddressTakenIatEntryTable.Count > 0 ? _imageBase + GuardAddressTakenIatEntryTable.Rva : 0ul, ref totalLength)
            && TryWriteNativeInt(writer, (ulong) GuardAddressTakenIatEntryTable.Count, ref totalLength)
            && TryWriteNativeInt(writer, GuardLongJumpTargetTable.Count > 0 ? _imageBase + GuardLongJumpTargetTable.Rva : 0ul, ref totalLength)
            && TryWriteNativeInt(writer, (ulong) GuardLongJumpTargetTable.Count, ref totalLength)
            && TryWriteVa(writer, DynamicValueRelocTable, ref totalLength)
            && TryWriteVa(writer, GuardRFFailureRoutine, ref totalLength)
            && TryWriteVa(writer, GuardRFFailureRoutineFunctionPointer, ref totalLength)
            && TryWriteUInt32(writer, DynamicValueRelocTableOffset, ref totalLength)
            && TryWriteUInt16(writer, DynamicValueRelocTableSection, ref totalLength)
            && TryWriteUInt16(writer, Reserved2, ref totalLength)
            && TryWriteVa(writer, GuardRFVerifyStackPointerFunctionPointer, ref totalLength)
            && TryWriteUInt32(writer, HotPatchTableOffset, ref totalLength)
            && TryWriteUInt32(writer, Reserved3, ref totalLength)
            && TryWriteVa(writer, EnclaveConfigurationPointer, ref totalLength)
            && TryWriteVa(writer, VolatileMetadataPointer, ref totalLength)
            && TryWriteNativeInt(writer, GuardEHContinuationTable.Count > 0 ? _imageBase + GuardEHContinuationTable.Rva : 0ul, ref totalLength)
            && TryWriteNativeInt(writer, (ulong) GuardEHContinuationTable.Count, ref totalLength)
            && TryWriteVa(writer, GuardXFGCheckFunctionPointer, ref totalLength)
            && TryWriteVa(writer, GuardXFGDispatchFunctionPointer, ref totalLength)
            && TryWriteVa(writer, GuardXFGTableDispatchFunctionPointer, ref totalLength)
            && TryWriteVa(writer, CastGuardOsDeterminedFailureMode, ref totalLength)
            && TryWriteVa(writer, GuardMemcpyFunctionPointer, ref totalLength)
            && TryWriteVa(writer, UmaFunctionPointers, ref totalLength)
            ;

        // Write any remaining (invalid) padding.
        if (@continue)
            writer.WriteZeroes((int) (Size - totalLength));
    }

    /// <inheritdoc />
    public IEnumerable<BaseRelocation> GetRequiredBaseRelocations()
    {
        int pointerSize = Is32Bit ? sizeof(uint) : sizeof(ulong);
        var relocType = Is32Bit ? RelocationType.HighLow : RelocationType.Dir64;

        foreach (uint offset in GetRelocatableHeaderOffsets())
        {
            // `GetRelocatableHeaderOffsets` returns sorted offsets, so we can exit early when we surpass `Size`.
            if (offset >= Size - pointerSize)
                break;

            yield return new BaseRelocation(relocType, this.ToReference((int) offset));
        }

        foreach (var reloc in LockPrefixTable.CreateBaseRelocations())
        {
            yield return reloc;
        }
    }

    private IEnumerable<uint> GetRelocatableHeaderOffsets()
    {
        uint[] fieldOffsets = Is32Bit ? FieldOffsets32 : FieldOffsets64;

        if (LockPrefixTable.Count > 0)
            yield return fieldOffsets[(int) LoadConfigurationField.LockPrefixTable];
        if (EditList != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.EditList];
        if (SecurityCookie is not null)
            yield return fieldOffsets[(int) LoadConfigurationField.SecurityCookie];
        if (SEHandlerTable.Count > 0)
            yield return fieldOffsets[(int) LoadConfigurationField.SEHandlerTable];
        if (GuardCFCheckFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardCFCheckFunctionPointer];
        if (GuardCFDispatchFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardCFDispatchFunctionPointer];
        if (GuardCFFunctionTable.Count > 0)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardCFFunctionTable];
        if (GuardAddressTakenIatEntryTable.Count > 0)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardAddressTakenIatEntryTable];
        if (GuardLongJumpTargetTable.Count > 0)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardLongJumpTargetTable];
        if (DynamicValueRelocTable != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.DynamicValueRelocTable];
        if (ChpeMetadataPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.ChpeMetadataPointer];
        if (GuardRFFailureRoutine != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardRFFailureRoutine];
        if (GuardRFFailureRoutineFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardRFFailureRoutineFunctionPointer];
        if (GuardRFVerifyStackPointerFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardRFVerifyStackPointerFunctionPointer];
        if (EnclaveConfigurationPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.EnclaveConfigurationPointer];
        if (VolatileMetadataPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.VolatileMetadataPointer];
        if (GuardEHContinuationTable.Count > 0)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardEHContinuationTable];
        if (GuardXFGCheckFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardXFGCheckFunctionPointer];
        if (GuardXFGDispatchFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardXFGDispatchFunctionPointer];
        if (GuardXFGTableDispatchFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardXFGTableDispatchFunctionPointer];
        if (CastGuardOsDeterminedFailureMode != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.CastGuardOsDeterminedFailureMode];
        if (GuardMemcpyFunctionPointer != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.GuardMemcpyFunctionPointer];
        if (UmaFunctionPointers != SegmentReference.Null)
            yield return fieldOffsets[(int) LoadConfigurationField.UmaFunctionPointers];
    }

    private bool IsWithinSize(BinaryStreamWriter writer, uint fieldSize, ref uint totalLength)
    {
        if (totalLength + fieldSize > Size)
        {
            // We are passing the allocated size of the structure if we would write this field.
            // Fill up anyways to ensure Size bytes are actually used (even if incorrect).
            int count = Math.Max(0, (int) Size - (int) totalLength);
            writer.WriteZeroes(count);

            totalLength += (uint) count;
            return false;
        }

        return true;
    }

    private bool TryWriteUInt16(BinaryStreamWriter writer, ushort value, ref uint totalLength)
    {
        if (IsWithinSize(writer, sizeof(ushort), ref totalLength))
        {
            writer.WriteUInt16(value);
            totalLength += sizeof(ushort);
            return true;
        }

        return false;
    }

    private bool TryWriteUInt32(BinaryStreamWriter writer, uint value, ref uint totalLength)
    {
        if (IsWithinSize(writer, sizeof(uint), ref totalLength))
        {
            writer.WriteUInt32(value);
            totalLength += sizeof(uint);
            return true;
        }

        return false;
    }

    private bool TryWriteUInt64(BinaryStreamWriter writer, ulong value, ref uint totalLength)
    {
        if (IsWithinSize(writer, sizeof(ulong), ref totalLength))
        {
            writer.WriteUInt64(value);
            totalLength += sizeof(ulong);
            return true;
        }

        return false;
    }

    private bool TryWriteNativeInt(BinaryStreamWriter writer, ulong value, ref uint totalLength)
    {
        return Is32Bit
            ? TryWriteUInt32(writer, (uint) value, ref totalLength)
            : TryWriteUInt64(writer, value, ref totalLength);

    }

    private bool TryWriteVa(BinaryStreamWriter writer, ISegmentReference value, ref uint totalLength)
    {
        return TryWriteNativeInt(writer, value != SegmentReference.Null ? _imageBase + value.Rva : 0, ref totalLength);
    }
}
