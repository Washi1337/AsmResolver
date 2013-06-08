using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodReference : MemberReference, IGenericContext
    {
        internal MethodSignature signature = null;
        internal TypeReference declaringType = null;
        internal string name = null;

        public MethodReference(MetaDataRow row)
            : base(row)
        {
        }

        public MethodReference(string name, TypeReference declaringType, uint signature)
            : base(new MetaDataRow(0U, 0U, signature))
        {
            this.name = name;
            this.declaringType = declaringType;
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (declaringType == null)
                {
                    netheader.TablesHeap.MemberRefParent.TryGetMember(Convert.ToInt32(metadatarow.parts[0]), out declaringType);
                }
                return declaringType;
            }
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]), out name);
                return name;
            }
        }

        public virtual MethodSignature Signature
        {
            get
            {
                if (signature != null)
                    return signature;
                signature = (MethodSignature)netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(metadatarow.parts[2]), this);
                return signature;
                //return Convert.ToUInt32(metadatarow.parts[2]); 
            }
        }

        public virtual bool IsDefinition
        {
            get { return false; }
        }

        public virtual bool IsGenericMethod
        {
            get { return false; }
        }

        public virtual GenericParameter[] GenericParameters
        {
            get
            {
                return null;
            }
        }

        public virtual TypeReference[] GenericArguments
        {
            get 
            { 
                return null;
            }
        }

        public bool HasGenericParameters
        {
            get { return GenericParameters != null && GenericParameters.Length > 0; }
        }

        public bool HasGenericArguments
        {
            get { return GenericArguments != null && GenericArguments.Length > 0; }
        }

        public override string FullName
        {
            get
            {
                try
                {

                    if (DeclaringType is TypeReference)
                        return Signature.ReturnType.ToString() + " " + ((TypeReference)DeclaringType).FullName + "::" + Name + Signature.GetParameterString();

                    return Name;
                }
                catch { return Name; }
            }
        }

        public virtual MethodReference GetElementMethod()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }

        public override void ClearCache()
        {
            signature = null;
            declaringType = null;
            name = null;
        }
    }
}
