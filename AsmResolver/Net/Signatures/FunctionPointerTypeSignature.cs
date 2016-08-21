using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class FunctionPointerTypeSignature : TypeSignature
    {
        public new static FunctionPointerTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new FunctionPointerTypeSignature(MethodSignature.FromReader(header, reader));
        }

        public FunctionPointerTypeSignature(MethodSignature signature)
        {
            Signature = signature;
        }

        public override ElementType ElementType
        {
            get { return ElementType.FnPtr; }
        }

        public MethodSignature Signature
        {
            get;
            set;
        }

        public override string Name
        {
            get { return "*"; }
        }

        public override string Namespace
        {
            get { return string.Empty; }
        }

        public override string FullName
        {
            get { return Signature.ToString(); }
        }

        public override IResolutionScope ResolutionScope
        {
            get { return null; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte)
                   + Signature.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteByte((byte) ElementType.FnPtr);
            Signature.Write(context);
        }
    }
}
