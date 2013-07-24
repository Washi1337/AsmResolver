using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeSpecification : TypeReference , ISpecification
    {
        private TypeReference originaltype;

        public TypeSpecification(MetaDataRow row)
            : this(row.NETHeader.BlobHeap.ReadTypeSignature(Convert.ToUInt32(row._parts[0]), null))
        {
            metadatarow = row;
        }

        public TypeSpecification(TypeReference typeRef)
            : base(default(MetaDataRow))
        {
            originaltype = typeRef;
            netheader = typeRef.netheader;
        }

        public MemberReference TransformWith(IGenericContext context)
        {
            return netheader.BlobHeap.ReadTypeSignature(Signature, context); 
        }

        public TypeReference OriginalType
        {
            get
            {
                if (originaltype == null)
                    originaltype = netheader.BlobHeap.ReadTypeSignature(Signature, this);
                return originaltype;
            }
            internal set { originaltype = value; }
        }

        public override string Name
        {
            get
            {
                if (OriginalType != null)
                    return OriginalType.Name;
                else
                    return null;
            }
        }
        
        public override string Namespace
        {
            get
            {
                if (OriginalType != null)
                    return OriginalType.Namespace;
                else
                    return null;
            }
        }
        
        public override string FullName
        {
            get { return (Namespace == "" ? "" : Namespace + ".") + Name; }
        }

        public override IResolutionScope ResolutionScope
        {
            get
            {
                if (OriginalType != null)
                    return OriginalType.ResolutionScope;
                else
                    return null;
            }
        }
        
        public uint Signature
        {
            get 
            {
                if (HasSavedMetaDataRow)
                    return Convert.ToUInt32(metadatarow._parts[0]);
                else
                    return 0;
            }
        }

        //public override GenericParameter[] GenericParameters
        //{
        //    get
        //    {
        //        return OriginalType.GenericParameters;
        //    }
        //}
       
        public override TypeReference GetElementType()
        {
            return OriginalType.GetElementType();
        }

        public override string ToString()
        {
            return FullName;
        }

    }
}
