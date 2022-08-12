using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="ProcedureTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedProcedureTypeRecord : ProcedureTypeRecord
{
    private readonly PdbReaderContext _context;
    private readonly uint _returnTypeIndex;
    private readonly ushort _parameterCount;
    private readonly uint _argumentListIndex;

    /// <summary>
    /// Reads a procedure type from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedProcedureTypeRecord(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _returnTypeIndex = reader.ReadUInt32();
        CallingConvention = (CodeViewCallingConvention) reader.ReadByte();
        Attributes = (MemberFunctionAttributes) reader.ReadByte();
        _parameterCount = reader.ReadUInt16();
        _argumentListIndex = reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetReturnType()
    {
        return _context.ParentImage.TryGetLeafRecord(_returnTypeIndex, out var leaf) && leaf is CodeViewTypeRecord type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Procedure type {TypeIndex:X8} contains an invalid return type index {_returnTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override ArgumentListLeaf? GetArguments()
    {
        if (_argumentListIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_argumentListIndex, out var leaf) && leaf is ArgumentListLeaf list
            ? list
            : _context.Parameters.ErrorListener.BadImageAndReturn<ArgumentListLeaf>(
                $"Procedure type {TypeIndex:X8} contains an invalid argument list index {_argumentListIndex:X8}.");
    }
}
