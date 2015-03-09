using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class RequiredModifierSignature : TypeSignature
    {
        public new static RequiredModifierSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new RequiredModifierSignature(ReadTypeDefOrRef(header, reader),
                TypeSignature.FromReader(header, reader));
        }

        public RequiredModifierSignature(ITypeDefOrRef modifierType, TypeSignature baseType)
        {
            ModifierType = modifierType;
            BaseType = baseType;
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

        public TypeSignature BaseType
        {
            get;
            set;
        }

        public override string Name
        {
            get { return BaseType.Name + string.Format(" modreq({0})", BaseType.FullName); }
        }

        public override string Namespace
        {
            get { return BaseType.Namespace; }
        }

        public override IResolutionScope ResolutionScope
        {
            get { return BaseType.ResolutionScope; }
        }

        public override ITypeDescriptor GetElementType()
        {
            return BaseType.GetElementType();
        }

        public override uint GetPhysicalLength()
        {
            var encoder = ModifierType.Header.GetStream<TableStream>()
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
