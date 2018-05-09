using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class RequiredModifierSignature : TypeSpecificationSignature
    {
        public new static RequiredModifierSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new RequiredModifierSignature(ReadTypeDefOrRef(image, reader),
                TypeSignature.FromReader(image, reader));
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

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            var encoder = buffer.TableStreamBuffer.GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof (byte) +
                   encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(ModifierType)).GetCompressedSize() +
                   BaseType.GetPhysicalLength(buffer);
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
        }
    }
}
