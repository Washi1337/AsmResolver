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
    private ulong _imageBase;

    /// <summary>
    /// Creates a new empty 32-bit load configuration.
    /// </summary>
    public LoadConfiguration()
        : this(true, LoadConfigurationSizes.X86.WinXP)
    {
    }

    /// <summary>
    /// Creates a new empty load configuration.
    /// </summary>
    /// <param name="is32Bit"><c>true</c> if a 32-bit directory is to be created, <c>false</c> for 64-bit.</param>
    public LoadConfiguration(bool is32Bit)
        : this(is32Bit, is32Bit ? LoadConfigurationSizes.X86.WinXP : LoadConfigurationSizes.X64.WinXP)
    {
    }

    /// <summary>
    /// Creates a new empty load configuration with the provided size.
    /// </summary>
    /// <param name="is32Bit"><c>true</c> if a 32-bit directory is to be created, <c>false</c> for 64-bit.</param>
    /// <param name="size">The size of the configuration. See also <see cref="LoadConfigurationSizes"/></param>
    public LoadConfiguration(bool is32Bit, uint size)
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
    /// <remarks>For known values, see <see cref="LoadConfigurationSizes"/>.</remarks>
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
    public partial ControlFlowGuardFunctionTable? GuardCFFunctionTable
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
    /// Obtains the control flow guard contiuation table.
    /// </summary>
    /// <returns>The table.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="GuardEHContinuationTable"/> property.
    /// </remarks>
    protected virtual ControlFlowGuardFunctionTable GetGuardEHContinuationTable() => new(this);

    /// <inheritdoc />
    public override void UpdateOffsets(in RelocationParameters parameters)
    {
        Is32Bit = parameters.Is32Bit;
        _imageBase = parameters.ImageBase;

        // Propagate image-base and bitness to VA tables.
        _lockPrefixTable?.UpdateOffsets(parameters.WithOffsetRva(_lockPrefixTable.Offset, _lockPrefixTable.Rva));

        base.UpdateOffsets(in parameters);
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize() => Size;

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IEnumerable<BaseRelocation> GetRequiredBaseRelocations()
    {
        var relocations = new List<BaseRelocation>();
        // TODO: header relocs
        relocations.AddRange(LockPrefixTable.CreateBaseRelocations());
        return relocations;
    }
}
