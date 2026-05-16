using System;
using AsmResolver.DotNet.PortablePdbs.CustomRecords;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized;

public class SerializedCustomDebugInformation : CustomDebugInformation
{
    private readonly PdbReaderContext _context;
    private readonly CustomDebugInformationRow _row;

    public SerializedCustomDebugInformation(PdbReaderContext context, MetadataToken token, CustomDebugInformationRow row)
        : base(token)
    {
        _context = context;
        _row = row;
    }

    protected override CustomDebugRecord GetValue() => CustomDebugRecord.FromRow(_context, in _row);

    protected override IHasCustomDebugInformation? GetOwner()
    {
        var encoder = _context.TablesStream.GetIndexEncoder(CodedIndex.HasCustomDebugInformation);
        var decoded = encoder.DecodeIndex(_row.Parent);
        IHasCustomDebugInformation? member;
        switch (decoded.Table)
        {
            case TableIndex.Method:
            case TableIndex.Field:
            case TableIndex.TypeRef:
            case TableIndex.TypeDef:
            case TableIndex.Param:
            case TableIndex.InterfaceImpl:
            case TableIndex.MemberRef:
            case TableIndex.Module:
            case TableIndex.DeclSecurity:
            case TableIndex.Property:
            case TableIndex.Event:
            case TableIndex.StandAloneSig:
            case TableIndex.ModuleRef:
            case TableIndex.TypeSpec:
            case TableIndex.Assembly:
            case TableIndex.AssemblyRef:
            case TableIndex.File:
            case TableIndex.ExportedType:
            case TableIndex.ManifestResource:
            case TableIndex.GenericParam:
            case TableIndex.GenericParamConstraint:
            case TableIndex.MethodSpec:
                _context.OwningModule.TryLookupMember<IHasCustomDebugInformation>(decoded, out member);
                break;
            case TableIndex.Document:
            case TableIndex.LocalScope:
            case TableIndex.LocalVariable:
            case TableIndex.LocalConstant:
            case TableIndex.ImportScope:
                _context.Pdb.TryLookupMember<IHasCustomDebugInformation>(decoded, out member);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return member;
    }
}
