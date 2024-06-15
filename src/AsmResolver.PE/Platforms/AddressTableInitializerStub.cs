using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Platforms;

/// <summary>
/// Represents a stub of code that initializes slots in a trampolined address table.
/// </summary>
public abstract class AddressTableInitializerStub : ISegment
{
    private readonly RelocatableSegment _slotInitializer;
    private readonly SegmentBuilder _contents;

    /// <summary>
    /// Initializes an address table initialization stub with the provided code.
    /// </summary>
    /// <param name="prologue">The prologue of the initializer code.</param>
    /// <param name="epilogue">The epilogue of the initializer code.</param>
    /// <param name="slotInitializer">The implementation of a function that initializes one slot in a table.</param>
    protected AddressTableInitializerStub(ISegment prologue, ISegment epilogue, RelocatableSegment slotInitializer)
    {
        _slotInitializer = slotInitializer;

        Body = new RelocatableSegmentBuilder();
        EntryPoint = new Symbol(prologue.ToReference());
        SlotInitializer = new Symbol(slotInitializer.Segment.ToReference());

        _contents = new SegmentBuilder
        {
            slotInitializer.Segment,
            prologue,
            Body,
            epilogue
        };
    }

    /// <inheritdoc />
    public ulong Offset => _contents.Offset;

    /// <inheritdoc />
    public uint Rva => _contents.Rva;

    /// <inheritdoc />
    public bool CanUpdateOffsets => _contents.CanUpdateOffsets;

    /// <summary>
    /// Gets the entry point symbol of the address table initializer.
    /// </summary>
    public ISymbol EntryPoint
    {
        get;
    }

    /// <summary>
    /// Gets the symbol pointing to the function that initializes a single slot in the table.
    /// </summary>
    /// <remarks>
    /// This is a function that expects two pointer-sized parameters (original slot address and value), passed in using
    /// the <c>__stdcall</c> calling convention of the underlying ABI.
    /// </remarks>
    protected ISymbol SlotInitializer
    {
        get;
    }

    /// <summary>
    /// Gets the raw body (excluding prologue and epilogue) of the initializer stub.
    /// </summary>
    protected RelocatableSegmentBuilder Body
    {
        get;
    }

    /// <summary>
    /// Adds code a single address slot initializer to the stub.
    /// </summary>
    /// <param name="originalSlot">The original slot to initialize.</param>
    /// <param name="newSlot">The new slot to redirect the original slot to.</param>
    public abstract void AddInitializer(ISymbol originalSlot, ISymbol newSlot);

    /// <summary>
    /// Collects all required base relocations when adding the initializer stub to a PE image.
    /// </summary>
    /// <returns>The required base relocations.</returns>
    public IEnumerable<BaseRelocation> GetRequiredBaseRelocations()
    {
        return _slotInitializer.Relocations.Concat(Body.GetRequiredBaseRelocations());
    }

    /// <inheritdoc />
    public void UpdateOffsets(in RelocationParameters parameters) => _contents.UpdateOffsets(in parameters);

    /// <inheritdoc />
    public uint GetPhysicalSize() => _contents.GetPhysicalSize();

    /// <inheritdoc />
    public uint GetVirtualSize() => _contents.GetVirtualSize();

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer) => _contents.Write(writer);
}
