using System;
using System.Collections;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class DynamicLocalVariablesRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("83C563C4-B4F3-47D5-B824-BA5441477EA8");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public BitArray? BitSequence { get; set; }

    public static DynamicLocalVariablesRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        return new DynamicLocalVariablesRecord
        {
            BitSequence = new BitArray(reader.ReadToEnd()),
        };
    }

    // TODO: decide whether these are wanted/figure out a better way to encode this
    public static TypeSignature DecodeDynamicBits(BitArray bitSequence, TypeSignature sig)
    {
        throw new NotImplementedException();
    }

    public static BitArray EncodeDynamicBits(TypeSignature sig)
    {
        throw new NotImplementedException();
    }

    public sealed class DynamicMarkerTypeSignature : TypeSignature
    {
        public CorLibTypeSignature ObjectType { get; }

        public DynamicMarkerTypeSignature(CorLibTypeSignature objectType)
        {
            if (objectType.ElementType != ElementType.Object)
            {
                throw new ArgumentException("Object type must have ElementType == ElementType.Object", nameof(objectType));
            }

            ObjectType = objectType;
        }

        public override string Name => ObjectType.Name;

        public override string Namespace => ObjectType.Namespace;

        public override IResolutionScope? Scope => ObjectType.Scope;

        public override bool IsValueType => false;

        public override ElementType ElementType => ElementType.Object;

        public override ITypeDefOrRef GetUnderlyingTypeDefOrRef() => ObjectType.GetUnderlyingTypeDefOrRef();

        public override bool IsImportedInModule(ModuleDefinition module) => ObjectType.IsImportedInModule(module);

        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => visitor.VisitCorLibType(ObjectType);

        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor, TState state) => visitor.VisitCorLibType(ObjectType, state);

        protected override void WriteContents(in BlobSerializationContext context) => context.Writer.WriteByte((byte)ElementType.Object);
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }
}
