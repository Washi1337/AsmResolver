using System.Collections.ObjectModel;
using AsmResolver.IO;

namespace AsmResolver.PE.Configuration;

/// <summary>
/// Represents a table of control flow guard function entry points in a loader configuration data directory.
/// </summary>
public class ControlFlowGuardFunctionTable : Collection<ControlFlowGuardFunction>, ISegment
{
    private readonly LoadConfiguration _owner;

    internal ControlFlowGuardFunctionTable(LoadConfiguration owner)
    {
        _owner = owner;
    }

    private bool HasMetadata => (_owner.GuardFlags & GuardFlags.CfFunctionTableSizeMask) != 0;

    /// <inheritdoc />
    public ulong Offset
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public uint Rva
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public bool CanUpdateOffsets => true;

    /// <inheritdoc />
    public void UpdateOffsets(in RelocationParameters parameters)
    {
        Offset = parameters.Offset;
        Rva =  parameters.Rva;
    }

    /// <inheritdoc />
    public uint GetPhysicalSize()
    {
        return (sizeof(uint) + (HasMetadata ? sizeof(byte) : 0u)) * (uint) Count;
    }

    /// <inheritdoc />
    public uint GetVirtualSize() => GetPhysicalSize();

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        bool hasMetadata = HasMetadata;

        for (int i = 0; i < Items.Count; i++)
        {
            var function = Items[i];

            writer.WriteUInt32(function.EntryPoint.Rva);
            if (hasMetadata)
                writer.WriteByte((byte) function.Flags);
        }
    }
}
