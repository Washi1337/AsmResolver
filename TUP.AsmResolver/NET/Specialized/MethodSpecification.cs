using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodSpecification : MethodReference, ISpecification
    {
        MethodReference originalmethod;
        TypeReference[] genericArgs;
        GenericParameter[] genericParams;
        IGenericContext context;

        public MethodSpecification(MetaDataRow row)
            : base(row)
        {
        }

        public MethodSpecification(MethodReference methodRef)
            : base(null)
        {
            OriginalMethod = methodRef;
            context = methodRef;            
        }

        public MemberReference TransformWith(IGenericContext context)
        {
            if (this.IsGenericMethod)
            {
                MethodSpecification copy = this.MemberwiseClone() as MethodSpecification;
                copy.context = context;
                copy.metadatarow = this.metadatarow;
                copy.genericArgs = netheader.BlobHeap.ReadGenericArgumentsSignature(copy.SpecificationSignature, context);
                uint signature = 0;
                if (copy.OriginalMethod.IsDefinition)
                    signature = Convert.ToUInt32(copy.OriginalMethod.MetaDataRow.Parts[4]);
                else
                    signature = Convert.ToUInt32(copy.originalmethod.MetaDataRow.Parts[2]);

                copy.signature = netheader.BlobHeap.ReadMemberRefSignature(signature, copy) as MethodSignature;
                return copy;
            }
            return this;
        }

        public MethodReference OriginalMethod
        {
            get
            {
                if (originalmethod == null)
                    originalmethod = (MethodReference)netheader.TablesHeap.MethodDefOrRef.GetMember(Convert.ToInt32(metadatarow.parts[0]));
                return originalmethod;
                //if ( == 0)
                //    return null;
                //return (MethodReference)netheader.tableheap.tablereader.MethodDefOrRef.GetMember(Convert.ToInt32(metadatarow.parts[0]));
            }
            private set { originalmethod = value; }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (originalmethod != null)
                    return originalmethod.DeclaringType;
                else
                    return null;
            }
        }
        public override bool IsDefinition
        {
            get
            {
                return false;
            }
        }
        public override string Name
        {
            get
            {
                if (originalmethod != null)
                    return originalmethod.Name;
                return null;
            }
        }
        public override bool IsGenericMethod
        {
            get
            {
                return true;
            }
        }

        public override GenericParameter[] GenericParameters
        {
            get
            {
                return OriginalMethod.GenericParameters;
                //if (genericParams == null)
                //{
                //    genericParams = new GenericParameter[GenericArguments.Length];
                //    for (int i = 0; i < genericParams.Length; i++)
                //        genericParams[i] = new GenericParameter("!!" + i, (ushort)i, (GenericParameterAttributes)0, this);
                //}
                //return genericParams;
            }
        }

        public override TypeReference[] GenericArguments
        {
            get
            {
                if (genericArgs == null)
                    genericArgs = netheader.BlobHeap.ReadGenericArgumentsSignature(SpecificationSignature, context);
                return genericArgs;

            }
        }
         
        public override MethodSignature Signature
        {
            get
            {
                if (signature == null)
                    signature = OriginalMethod.Signature; //netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(OriginalMethod.metadatarow.parts[2]), this, this) as MethodSignature;
                return signature;
            }
        }

        public uint SpecificationSignature
        {
            get { return Convert.ToUInt32(metadatarow.parts[1]); }
        }

        public override MethodReference GetElementMethod()
        {
            return OriginalMethod.GetElementMethod();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Signature.ReturnType.FullName);
            builder.Append(" ");
            builder.Append(DeclaringType.ToString());
            builder.Append("::");
            builder.Append(Name);
            if (GenericArguments != null && GenericArguments.Length > 0)
            {
                builder.Append("<");
                for (int i = 0;i < GenericArguments.Length; i++)
                    builder.Append(GenericArguments[i].FullName + (i == GenericArguments.Length-1 ? "": ", "));
                builder.Append(">");
            }
            builder.Append(Signature.GetParameterString());
            return builder.ToString();
        }
        //public uint Signature { get { return Convert.ToUInt32(metadatarow.parts[1]); } }

    }
}
