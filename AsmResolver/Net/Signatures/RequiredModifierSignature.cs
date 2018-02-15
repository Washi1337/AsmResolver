using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class RequiredModifierSignature : TypeSpecificationSignature
    {
        public new static RequiredModifierSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new RequiredModifierSignature(ReadTypeDefOrRef(header, reader),
                TypeSignature.FromReader(header, reader))
            {
                StartOffset = position
            };
        }

        public RequiredModifierSignature(ITypeDefOrRef modifierType, TypeSignature baseType)
            : base(baseType)
        {
            ModifierType = modifierType;
        }

        public override ElementType ElementType
        {
            get { return ElementType.CModReqD; }
        }

        public ITypeDefOrRef ModifierType
        {
            get;
            set;
        }

        public override string Name
        {
            get { return BaseType.Name + string.Format(" modreq({0})", ModifierType.FullName); }
        }

        public override uint GetPhysicalLength()
        {
            var encoder = ModifierType.Image.Header.GetStream<TableStream>()
                    .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof (byte) +
                   encoder.EncodeToken(ModifierType.MetadataToken).GetCompressedSize() +
                   BaseType.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)ElementType);
            WriteTypeDefOrRef(context.Assembly.NetDirectory.MetadataHeader, context.Writer, ModifierType);
            BaseType.Write(context);
        }
    }
}
