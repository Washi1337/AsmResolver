using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs;

public class LocalConstantSignature : ExtendableBlobSignature
{
    public TypeSignature ConstType
    {
        get;
        set;
    }

    public object? Value
    {
        get;
        set;
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }

    public static LocalConstantSignature FromReader(ref BlobReaderContext context, ref BinaryStreamReader reader)
    {
        var (type, value) = FromReaderCore(ref context, ref reader);
        return new LocalConstantSignature
        {
            ConstType = type,
            Value = value,
        };
    }

    private static (TypeSignature Type, object? Value) FromReaderCore(ref BlobReaderContext context, ref BinaryStreamReader reader)
    {
        var typeFactory = context.ReaderContext.ParentModule.CorLibTypeFactory;
        var encoder = context.ReaderContext.TablesStream.GetIndexEncoder(CodedIndex.TypeDefOrRef);
        var (type, value) = (ElementType)reader.ReadByte() switch
        {
            ElementType.CModOpt => ReadModifedType(isRequired: false, ref context, ref reader),
            ElementType.CModReqD => ReadModifedType(isRequired: true, ref context, ref reader),
            ElementType.Boolean => (typeFactory.Boolean, reader.ReadByte() == 1),
            ElementType.Char => (typeFactory.Char, (char)reader.ReadUInt16()),
            ElementType.I1 => (typeFactory.SByte, reader.ReadSByte()),
            ElementType.U1 => (typeFactory.Byte, reader.ReadByte()),
            ElementType.I2 => (typeFactory.Int16, reader.ReadInt16()),
            ElementType.U2 => (typeFactory.UInt16, reader.ReadUInt16()),
            ElementType.I4 => (typeFactory.Int32, reader.ReadInt32()),
            ElementType.U4 => (typeFactory.UInt32, reader.ReadUInt32()),
            ElementType.I8 => (typeFactory.Int64, reader.ReadInt64()),
            ElementType.U8 => (typeFactory.UInt64, reader.ReadUInt64()),
            ElementType.R4 => (typeFactory.Single, reader.ReadSingle()),
            ElementType.R8 => (typeFactory.Double, reader.ReadDouble()),
            ElementType.String => (typeFactory.String, reader.PeekByte() == 0xff ? null : reader.ReadUnicodeString()),
            ElementType.Class => ReadGeneralConstant(isValueType: false, ref context, ref reader),
            ElementType.ValueType => ReadGeneralConstant(isValueType: true, ref context, ref reader),
            ElementType.Object => (typeFactory.Object, null),
            var invalid => context.ReaderContext.BadImageAndReturn<(TypeSignature, object?)>($"Invalid ElementType in LocalConstSignature: {invalid}"),
        };

        if (reader.CanRead(1))
        {
            // there is more data, this is an enum type code
            type = ReadType(ref context, ref reader).ToTypeSignature(isValueType: true);
        }

        return (type, value);

        (TypeSignature, object?) ReadModifedType(bool isRequired, ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            var modifierType = ReadType(ref context, ref reader);
            var result = FromReaderCore(ref context, ref reader);
            return result with { Type = result.Type.MakeModifierType(modifierType, isRequired) };
        }
        (TypeSignature, object?) ReadGeneralConstant(bool isValueType, ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            var type = ReadType(ref context, ref reader).ToTypeSignature(isValueType);
            if (type.IsTypeOf("System", "Decimal"))
            {
                if (!reader.CanRead(1))
                {
                    return (type, 0m);
                }
                byte firstByte = reader.ReadByte();
                bool isNegative = (firstByte & 0x80) != 0;
                byte scale = (byte)(firstByte & 0x7F);
                (int low, int mid, int high) = (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                return (type, new decimal(low, mid, high, isNegative, scale));
            }
            if (type.IsTypeOf("System", "DateTime"))
            {
                if (!reader.CanRead(1))
                {
                    return (type, default(DateTime));
                }
                return (type, new DateTime(reader.ReadInt64()));
            }
            if (!reader.CanRead(1))
            {
                return (type, null);
            }
            return context.ReaderContext.BadImageAndReturn<(TypeSignature, object?)>($"Unknown general constant type {type.FullName}");
        }
        ITypeDefOrRef ReadType(ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            return context.TypeSignatureResolver.ResolveToken(ref context, encoder.DecodeIndex(reader.ReadCompressedUInt32()));
        }
    }
}
