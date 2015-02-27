using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public abstract class MemberSignature : BlobSignature, IHasTypeSignature
    {
        public static MemberSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            var flag = reader.ReadByte();
            reader.Position--;
            if (flag == 0x6)
                return FieldSignature.FromReader(header, reader);
            return MethodSignature.FromReader(header,reader);
        }

        public abstract bool IsMethod
        {
            get;
        }

        protected abstract TypeSignature TypeSignature
        {
            get;
        }

        TypeSignature IHasTypeSignature.TypeSignature
        {
            get { return TypeSignature; }
        }


    }
}
