using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;
using AsmResolver.PE.Code;
using AsmResolver.PE.File;
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
    /// Creates a new trampoline for the provided function table slot symbol.
    /// </summary>
    /// <param name="functionTableSlot">The symbol to redirect.</param>
    public void AddFunctionTableSlotTrampoline(ISymbol functionTableSlot)
    {
        var trampoline = new FunctionTrampoline(_platform, functionTableSlot);
        _trampolines.Add(trampoline);
        _trampolineTable.Add(trampoline.TrampolineCode.Segment);
    }

    /// <summary>
    /// Applies the trampoline address patches to the provided PE file.
    /// </summary>
    /// <param name="file">The file to patch.</param>
    public void ApplyPatches(IPEFile file) => ApplyPatches(file.Sections);

    /// <summary>
    /// Applies the trampoline address patches to the provided PE file sections.
    /// </summary>
    /// <param name="sections">The sections to patch.</param>
    public void ApplyPatches(IList<PESection> sections)
    {
        foreach (var trampoline in _trampolines)
        {
            if (trampoline.OriginalSymbolReference is not { } originalSymbolAddress)
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
            foreach (var codeRelocation in trampoline.TrampolineCode.Relocations)
                yield return codeRelocation;
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
    public override void Write(IBinaryStreamWriter writer) => _contents.Write(writer);

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
        public FunctionTrampoline(Platform platform, ISymbol originalSymbol)
        {
            OriginalSymbol = originalSymbol;
            OriginalSymbolReference = originalSymbol.GetReference()
                ?? throw new ArgumentException($"Symbol {originalSymbol} does not have a reference assigned to it.");

            OriginalSymbolRelocation = new BaseRelocation(
                platform.Is32Bit ? RelocationType.HighLow : RelocationType.Dir64,
                OriginalSymbolReference
            );

            TrampolineCode = platform.CreateThunkStub(originalSymbol);
            TrampolineSymbol = new Symbol(TrampolineCode.Segment.ToReference());
        }

        public ISymbol OriginalSymbol { get; }

        public ISegmentReference OriginalSymbolReference { get; }

        public BaseRelocation OriginalSymbolRelocation { get; }

        public Symbol TrampolineSymbol { get; }

        public RelocatableSegment TrampolineCode { get; }
    }
}
