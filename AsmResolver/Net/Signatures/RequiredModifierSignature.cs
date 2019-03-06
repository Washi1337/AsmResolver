using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class RequiredModifierSignature : TypeSpecificationSignature
    {
        public static RequiredModifierSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }
        
        public static RequiredModifierSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new RequiredModifierSignature(ReadTypeDefOrRef(image, reader, protection),
                TypeSignature.FromReader(image, reader, false, protection));
        }

        public RequiredModifierSignature(ITypeDefOrRef modifierType, TypeSignature baseType)
            : base(baseType)
        {
            ModifierType = modifierType;
        }

        public override ElementType ElementType => ElementType.CModReqD;

        public ITypeDefOrRef ModifierType
        {
            get;
            set;
        }

        public override string Name => BaseType.Name + $" modreq({ModifierType.FullName})";

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            var encoder = ModifierType.Image.Header.GetStream<TableStream>()
                    .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof(byte) +
                   encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(ModifierType)).GetCompressedSize() +
                   BaseType.GetPhysicalLength(buffer) +
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            base.Prepare(buffer);
            buffer.TableStreamBuffer.GetTypeToken(ModifierType);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            WriteTypeDefOrRef(buffer, writer, ModifierType);
            BaseType.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
