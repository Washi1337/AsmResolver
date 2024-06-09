using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;
using AsmResolver.PE.Code;
using AsmResolver.PE.File;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Platforms;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Builder;

/// <summary>
/// Represents a buffer of code trampolines used to redirect functions stored in address tables such as import
/// address tables and VTable fixups.
/// </summary>
public class TrampolineTableBuffer : SegmentBase
{
    // Reference: https://blog.washi.dev/posts/import-patching/

    private readonly Platform _platform;
    private readonly List<FunctionTrampoline> _trampolines = new();

    private readonly SegmentBuilder _contents = new();
    private readonly SegmentBuilder _trampolineTable = new();

    private AddressTableInitializerStub? _initializerStub;

    /// <summary>
    /// Creates a new buffer for the designated platform.
    /// </summary>
    /// <param name="platform">The platform.</param>
    public TrampolineTableBuffer(Platform platform)
    {
        _platform = platform;
        _contents.Add(_trampolineTable, (uint) platform.PointerSize);
    }

    /// <summary>
    /// Gets a value indicating whether the trampoline table will require dynamic initialization.
    /// </summary>
    /// <remarks>
    /// Currently, this is only the case if data slots are added to the table.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(DataInitializerSymbol))]
    public bool RequiresDynamicInitialization => DataInitializerSymbol is not null;

    /// <summary>
    /// When the buffer has data initializers registered, gets the symbol for the main initializer of the trampoline.
    /// </summary>
    public ISymbol? DataInitializerSymbol => _initializerStub?.EntryPoint;

    /// <summary>
    /// Gets or sets the symbol to <c>kernel32.dll!VirtualProtect</c> that is used during dynamic initialization of the
    /// trampolined table.
    /// </summary>
    public ImportedSymbol? VirtualProtect
    {
        get;
        set;
    }

    /// <summary>
    /// Adds a new trampoline for the provided function table slot symbol.
    /// </summary>
    /// <param name="functionTableSlot">The function table slot to redirect.</param>
    /// <param name="newSymbol">The function table slot to redirect to.</param>
    public void AddFunctionTableSlotTrampoline(ISymbol functionTableSlot, ISymbol newSymbol)
    {
        var trampoline = new FunctionTrampoline(_platform, functionTableSlot, newSymbol);
        _trampolines.Add(trampoline);
        _trampolineTable.Add(trampoline.TrampolineCode.Segment);
    }

    /// <summary>
    /// Adds an initializer for the provided data table slot symbol.
    /// </summary>
    /// <param name="originalSlot">The original data table slot to initialize.</param>
    /// <param name="newSlot">The data table slot to initialize the original slot with.</param>
    public void AddDataSlotInitializer(ISymbol originalSlot, ISymbol newSlot)
    {
        EnsureAddressTableInitializerAdded();
        _initializerStub.AddInitializer(originalSlot, newSlot);
    }

    [MemberNotNull(nameof(_initializerStub))]
    private void EnsureAddressTableInitializerAdded()
    {
        if (VirtualProtect is null)
        {
            var module = new ImportedModule("KERNEL32.dll");
            VirtualProtect = new ImportedSymbol(0, "VirtualProtect");
            module.Symbols.Add(VirtualProtect);
        }

        if (_initializerStub is null)
        {
            _initializerStub = _platform.CreateAddressTableInitializer(VirtualProtect);
            _contents.Add(_initializerStub, (uint) _platform.PointerSize);
        }
    }

    /// <summary>
    /// Applies the trampoline address patches to the provided PE file.
    /// </summary>
    /// <param name="file">The file to patch.</param>
    public void ApplyPatches(PEFile file) => ApplyPatches(file.Sections);

    /// <summary>
    /// Applies the trampoline address patches to the provided PE file sections.
    /// </summary>
    /// <param name="sections">The sections to patch.</param>
    public void ApplyPatches(IList<PESection> sections)
    {
        foreach (var trampoline in _trampolines)
        {
            if (trampoline.OriginalSymbol.GetReference() is not { } originalSymbolAddress)
                continue;

            if (!TryGetSectionContainingRva(sections, originalSymbolAddress.Rva, out var section))
                continue;

            section.Contents = section.Contents?.AsPatchedSegment().Patch(
                originalSymbolAddress.Rva - section.Rva,
                _platform.Is32Bit ? AddressFixupType.Absolute32BitAddress : AddressFixupType.Absolute64BitAddress,
                trampoline.TrampolineSymbol
            );
        }
    }

    /// <summary>
    /// Obtains the base relocations that are required to be applied on all the generated trampoline code.
    /// </summary>
    /// <returns>The base relocations.</returns>
    public IEnumerable<BaseRelocation> GetRequiredBaseRelocations()
    {
        foreach (var trampoline in _trampolines)
        {
            yield return trampoline.OriginalSymbolRelocation;
            foreach (var trampolineReloc in trampoline.TrampolineCode.Relocations)
                yield return trampolineReloc;
        }

        if (_initializerStub is not null)
        {
            foreach (var reloc in _initializerStub.GetRequiredBaseRelocations())
                yield return reloc;
        }
    }

    /// <inheritdoc />
    public override void UpdateOffsets(in RelocationParameters parameters)
    {
        base.UpdateOffsets(in parameters);
        _contents.UpdateOffsets(parameters);
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize() => _contents.GetPhysicalSize();

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer) => _contents.Write(writer);

    private static bool TryGetSectionContainingRva(
        IList<PESection> sections,
        uint rva,
        [NotNullWhen(true)] out PESection? section)
    {
        for (int i = 0; i < sections.Count; i++)
        {
            if (sections[i].ContainsRva(rva))
            {
                section = sections[i];
                return true;
            }
        }

        section = null;
        return false;
    }

    private sealed class FunctionTrampoline
    {
        public FunctionTrampoline(Platform platform, ISymbol originalSymbol, ISymbol newSymbol)
        {
            OriginalSymbol = originalSymbol;

            OriginalSymbolRelocation = new BaseRelocation(
                platform.Is32Bit ? RelocationType.HighLow : RelocationType.Dir64,
                originalSymbol.GetReference() ?? throw new ArgumentException($"{originalSymbol} does not have an address assigned.")
            );

            TrampolineCode = platform.CreateThunkStub(newSymbol);
            TrampolineSymbol = new Symbol(TrampolineCode.Segment.ToReference());
        }

        public ISymbol OriginalSymbol { get; }

        public BaseRelocation OriginalSymbolRelocation { get; }

        public Symbol TrampolineSymbol { get; }

        public RelocatableSegment TrampolineCode { get; }
    }
}
