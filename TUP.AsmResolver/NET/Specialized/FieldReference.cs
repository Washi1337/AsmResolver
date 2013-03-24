using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldReference : MemberReference
    {
        internal FieldSignature signature = null;
        TypeReference declaringType;
        string name;

        public override TypeReference DeclaringType
        {
            get
            {
                if (declaringType == null)
                {
                    MetaDataMember member;
                    netheader.TablesHeap.MemberRefParent.TryGetMember(Convert.ToInt32(metadatarow.parts[0]), out member);
                    declaringType = member as TypeReference;
                }
                return declaringType;
            }

        }
        public override string Name
        {
            get
            {
                if (name == null)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]));
                return name;
            }
        }
        public override string FullName
        {
            get
            {
                try
                {
                    if (DeclaringType is TypeReference)
                        return (Signature != null ? Signature.ReturnType.FullName + " " : "") + ((TypeReference)DeclaringType).FullName + "::" + Name;

                    return Name;
                }
                catch { return Name; }
            }
        }

        public FieldSignature Signature
        {
            get
            {
                if (signature != null)
                    return signature;
                signature = (FieldSignature)netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(metadatarow.parts[2]), this.DeclaringType, null);
                return signature;
            }
        }

        public override void ClearCache()
        {
            signature = null;
            declaringType = null;
            name = null;
        }
    }
}
