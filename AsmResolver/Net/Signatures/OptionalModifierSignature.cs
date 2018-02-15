using System;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class OptionalModifierSignature : TypeSpecificationSignature
    {
        public new static OptionalModifierSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new OptionalModifierSignature(ReadTypeDefOrRef(image, reader),
                TypeSignature.FromReader(image, reader))
            {
                StartOffset = position
            };
        }

        public OptionalModifierSignature(ITypeDefOrRef modifierType, TypeSignature baseType)
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
            get { return BaseType.Name + string.Format(" modopt({0})", ModifierType.FullName); }
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
            throw new NotImplementedException();
            //TODO
            //var writer = context.Writer;
            //writer.WriteByte((byte)ElementType);
            //WriteTypeDefOrRef(context.Assembly.NetDirectory.MetadataHeader, context.Writer, ModifierType);
            //BaseType.Write(context);
        }
    }
}
