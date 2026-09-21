using System;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.PE.Configuration;

/// <summary>
/// Provides an implementation of a loader configuration directory that was read from an existing PE file.
/// </summary>
public class SerializedLoadConfiguration : LoadConfiguration
{
    private readonly PEReaderContext _context;
    private readonly ulong _lockPrefixTableVa;
    private readonly ulong _securityCookieVa;
    private readonly ulong _sehHandlerTableVa;
    private readonly ulong _sehHandlerTableCount;
    private readonly ulong _guardCFFunctionTableVa;
    private readonly ulong _guardCFFunctionTableCount;
    private readonly ulong _guardAddressTakenIatEntryTableVa;
    private readonly ulong _guardAddressTakenIatEntryCount;
    private readonly ulong _guardLongJumpTargetTableVa;
    private readonly ulong _guardLongJumpTargetCount;
    private readonly ulong _guardEHContinuationTableVa;
    private readonly ulong _guardEHContinuationCount;
    private GuardFlags _originalFlags;

    /// <summary>
    /// Reads a single loader configuration directory from an input stream.
    /// </summary>
    /// <param name="context">The reader context.</param>
    /// <param name="reader">The input stream.</param>
    public SerializedLoadConfiguration(PEReaderContext context, ref BinaryStreamReader reader)
    {
        _context = context;

        UpdateOffsets(context.GetRelocation(reader.Offset, reader.Rva));

        // We use a somewhat convoluted way of conditionally reading fields here as loader configs are dynamically sized
        // based on its first field `Size`. By chaining the TryConsumeX with && we can exit early without explicitly
        // having to `return` on every line.

        Size = reader.ReadUInt32();

        uint actualSize = Math.Min(reader.RemainingLength, Size - sizeof(uint));

        var boundedReader = reader.Fork();
        boundedReader.ChangeSize(actualSize);

        reader.Offset += actualSize;

        bool @continue =
            TryConsumeUInt32(ref boundedReader, static (self, v) => self.TimeDateStamp = v)
            && TryConsumeUInt16(ref boundedReader, static (self, v) => self.MajorVersion = v)
            && TryConsumeUInt16(ref boundedReader, static (self, v) => self.MinorVersion = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.GlobalFlagsClear = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.GlobalFlagsSet = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.CriticalSectionDefaultTimeout = v)
            && TryConsumeNativeInt(ref boundedReader, static (self, v) => self.DeCommitFreeBlockThreshold = v)
            && TryConsumeNativeInt(ref boundedReader, static (self, v) => self.DeCommitTotalFreeThreshold = v)
            && boundedReader.TryReadNativeInt(Is32Bit, out _lockPrefixTableVa)
            && TryConsumeNativeInt(ref boundedReader, static (self, v) => self.MaximumAllocationSize = v)
            && TryConsumeNativeInt(ref boundedReader, static (self, v) => self.VirtualMemoryThreshold = v)
            ;

        if (!@continue)
            return;

        // 32-bits/64-bits swap for some reason process affinity mask and heap flags.
        if (Is32Bit)
        {
            @continue = TryConsumeUInt32(ref boundedReader, static (self, v) => self.ProcessHeapFlags = v)
                && TryConsumeUInt32(ref boundedReader, static (self, v) => self.ProcessAffinityMask = v);
        }
        else
        {
            @continue = TryConsumeUInt64(ref boundedReader, static (self, v) => self.ProcessAffinityMask = v)
                && TryConsumeUInt32(ref boundedReader, static (self, v) => self.ProcessHeapFlags = v);
        }

        if (!@continue)
            return;

        _ = TryConsumeUInt16(ref boundedReader, static (self, v) => self.CsdVersion = v)
            && TryConsumeUInt16(ref boundedReader, static (self, v) => self.DependentLoadFlags = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.EditList = v)
            && boundedReader.TryReadNativeInt(Is32Bit, out _securityCookieVa)
            && boundedReader.TryReadNativeInt(Is32Bit, out _sehHandlerTableVa)
            && boundedReader.TryReadNativeInt(Is32Bit, out _sehHandlerTableCount)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardCFCheckFunctionPointer = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardCFDispatchFunctionPointer = v)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardCFFunctionTableVa)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardCFFunctionTableCount)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.GuardFlags = self._originalFlags = (GuardFlags) v)
            && TryConsumeUInt16(ref boundedReader, static (self, v) => self.CodeIntegrityFlags = v)
            && TryConsumeUInt16(ref boundedReader, static (self, v) => self.CodeIntegrityCatalog = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.CodeIntegrityOffset = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.CodeIntegrityReserved = v)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardAddressTakenIatEntryTableVa)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardAddressTakenIatEntryCount)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardLongJumpTargetTableVa)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardLongJumpTargetCount)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.DynamicValueRelocTable = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardRFFailureRoutine = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardRFFailureRoutineFunctionPointer = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.DynamicValueRelocTableOffset = v)
            && TryConsumeUInt16(ref boundedReader, static (self, v) => self.DynamicValueRelocTableSection = v)
            && TryConsumeUInt16(ref boundedReader, static (self, v) => self.Reserved2 = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardRFVerifyStackPointerFunctionPointer = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.HotPatchTableOffset = v)
            && TryConsumeUInt32(ref boundedReader, static (self, v) => self.Reserved3 = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.EnclaveConfigurationPointer = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.VolatileMetadataPointer = v)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardEHContinuationTableVa)
            && boundedReader.TryReadNativeInt(Is32Bit, out _guardEHContinuationCount)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardXFGCheckFunctionPointer = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardXFGDispatchFunctionPointer = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardXFGTableDispatchFunctionPointer = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.CastGuardOsDeterminedFailureMode = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.GuardMemcpyFunctionPointer = v)
            && TryConsumeVa(context, ref boundedReader, static (self, v) => self.UmaFunctionPointers = v)
            ;
    }

    private bool TryConsumeUInt16(ref BinaryStreamReader reader, Action<SerializedLoadConfiguration, ushort> callback)
    {
        if (reader.TryReadUInt16(out ushort value))
        {
            callback(this, value);
            return true;
        }

        return false;
    }

    private bool TryConsumeUInt32(ref BinaryStreamReader reader, Action<SerializedLoadConfiguration, uint> callback)
    {
        if (reader.TryReadUInt32(out uint value))
        {
            callback(this, value);
            return true;
        }

        return false;
    }

    private bool TryConsumeUInt64(ref BinaryStreamReader reader, Action<SerializedLoadConfiguration, ulong> callback)
    {
        if (reader.TryReadUInt64(out ulong value))
        {
            callback(this, value);
            return true;
        }

        return false;
    }

    private bool TryConsumeNativeInt(ref BinaryStreamReader reader, Action<SerializedLoadConfiguration, ulong> callback)
    {
        if (reader.TryReadNativeInt(Is32Bit, out ulong value))
        {
            callback(this, value);
            return true;
        }

        return false;
    }

    private bool TryConsumeVa(
        PEReaderContext context,
        ref BinaryStreamReader reader,
        Action<SerializedLoadConfiguration, ISegmentReference> callback
    )
    {
        if (reader.TryReadNativeInt(Is32Bit, out ulong value))
        {
            var reference = value != 0
                ? context.File.GetReferenceToRva((uint) (value - context.File.OptionalHeader.ImageBase))
                : SegmentReference.Null;

            callback(this, reference);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    protected override ReferenceTable GetLockPrefixTable()
    {
        var table = base.GetLockPrefixTable();

        uint rva = (uint) (_lockPrefixTableVa - _context.File.OptionalHeader.ImageBase);
        if (!_context.File.TryCreateReaderAtRva(rva, out var reader))
            return table;

        return reader.ReadReferenceTable(_context, table);
    }

    /// <inheritdoc />
    protected override ISegment? GetSecurityCookie()
    {
        uint rva = (uint) (_securityCookieVa - _context.File.OptionalHeader.ImageBase);
        if (!_context.File.TryCreateReaderAtRva(rva, out var reader))
            return null;

        return reader.ReadSegment((uint) _context.Platform.PointerSize);
    }

    /// <inheritdoc />
    protected override ReferenceTable GetSEHandlerTable()
    {
        var table = base.GetSEHandlerTable();

        uint rva = (uint) (_sehHandlerTableVa - _context.File.OptionalHeader.ImageBase);
        if (!_context.File.TryCreateReaderAtRva(rva, out var reader))
            return table;

        return reader.ReadReferenceTable(_context, table, count: (int?) _sehHandlerTableCount);
    }

    /// <inheritdoc />
    protected override ControlFlowGuardFunctionTable GetGuardCFFunctionTable()
    {
        return ReadControlFlowGuardFunctionTable(_guardCFFunctionTableVa, _guardCFFunctionTableCount);
    }

    /// <inheritdoc />
    protected override ControlFlowGuardFunctionTable GetGuardAddressTakenIatEntryTable()
    {
        return ReadControlFlowGuardFunctionTable(_guardAddressTakenIatEntryTableVa, _guardAddressTakenIatEntryCount);
    }

    /// <inheritdoc />
    protected override ControlFlowGuardFunctionTable GetGuardLongJumpTargetTable()
    {
        return ReadControlFlowGuardFunctionTable(_guardLongJumpTargetTableVa, _guardLongJumpTargetCount);
    }

    /// <inheritdoc />
    protected override ControlFlowGuardFunctionTable GetGuardEHContinuationTable()
    {
        return ReadControlFlowGuardFunctionTable(_guardEHContinuationTableVa, _guardEHContinuationCount);
    }

    private ControlFlowGuardFunctionTable ReadControlFlowGuardFunctionTable(ulong va, ulong count)
    {
        var table = new ControlFlowGuardFunctionTable(this);

        uint rva = (uint) (va - _context.File.OptionalHeader.ImageBase);
        if (!_context.File.TryCreateReaderAtRva(rva, out var reader))
            return table;

        bool hasMetadata = (_originalFlags & GuardFlags.CfFunctionTableSizeMask) != 0;

        table.UpdateOffsets(_context.GetRelocation(reader.Offset, reader.Rva));

        for (ulong i = 0; i < count; i++)
        {
            uint functionRva = reader.ReadUInt32();
            var flags = hasMetadata
                ? (ControlFlowGuardFunctionFlags) reader.ReadByte()
                : 0;

            table.Add(new ControlFlowGuardFunction(_context.File.GetReferenceToRva(functionRva), flags));
        }

        return table;
    }
}
