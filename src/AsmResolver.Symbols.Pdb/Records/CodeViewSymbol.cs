using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Records.Serialized;
using static AsmResolver.Symbols.Pdb.Records.CodeViewSymbolType;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a single symbol record within the symbol record stream of a PDB file.
/// </summary>
public abstract class CodeViewSymbol : ICodeViewSymbol
{
    /// <summary>
    /// Gets the type of symbol this record encodes.
    /// </summary>
    public abstract CodeViewSymbolType CodeViewSymbolType
    {
        get;
    }

    /// <summary>
    /// Reads a single symbol record from the input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream.</param>
    /// <returns>The read symbol.</returns>
    public static CodeViewSymbol FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        var type = (CodeViewSymbolType) reader.ReadUInt16();
        var dataReader = reader.ForkRelative(reader.RelativeOffset, (uint) (length - 2));
        reader.Offset += (ulong) (length - 2);

        return type switch
        {
            BPRel32 => new SerializedBasePointerRelativeSymbol(context, dataReader),
            BuildInfo => new SerializedBuildInfoSymbol(context, dataReader),
            CallSiteInfo => new SerializedCallSiteSymbol(context, dataReader),
            Callees => new SerializedFunctionListSymbol(context, dataReader, false),
            Callers => new SerializedFunctionListSymbol(context, dataReader, true),
            Compile2 => new SerializedCompile2Symbol(dataReader),
            Compile3 => new SerializedCompile3Symbol(dataReader),
            Constant => new SerializedConstantSymbol(context, dataReader),
            DefRangeFramePointerRel => new SerializedFramePointerRangeSymbol(dataReader, false),
            DefRangeFramePointerRelFullScope => new SerializedFramePointerRangeSymbol(dataReader, true),
            DefRangeRegisterRel => new SerializedRegisterRelativeRangeSymbol(dataReader),
            EnvBlock => new SerializedEnvironmentBlockSymbol(dataReader),
            FrameProc => new SerializedFrameProcedureSymbol(dataReader),
            Local => new SerializedLocalSymbol(context, dataReader),
            LProcRef => new SerializedProcedureReferenceSymbol(dataReader, true),
            ObjName => new SerializedObjectNameSymbol(dataReader),
            ProcRef => new SerializedProcedureReferenceSymbol(dataReader, false),
            Pub32 => new SerializedPublicSymbol(dataReader),
            RegRel32 => new SerializedRegisterRelativeSymbol(context, dataReader),
            Udt => new SerializedUserDefinedTypeSymbol(context, dataReader),
            UNamespace => new SerializedUsingNamespaceSymbol(dataReader),
            _ => new UnknownSymbol(type, dataReader.ReadToEnd())
        };
    }
}
