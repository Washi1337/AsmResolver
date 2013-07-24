using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeSpecification : TypeReference , ISpecification
    {
        private TypeReference _originaltype;

        public TypeSpecification(MetaDataRow row)
            : this(row.NETHeader.BlobHeap.ReadTypeSignature(Convert.ToUInt32(row._parts[0]), null))
        {
            _metadatarow = row;
        }

        public TypeSpecification(TypeReference typeRef)
            : base(default(MetaDataRow))
        {
            _originaltype = typeRef;
            _netheader = typeRef._netheader;
        }

        public MemberReference TransformWith(IGenericContext context)
        {
            return _netheader.BlobHeap.ReadTypeSignature(Signature, context); 
        }

        public TypeReference OriginalType
        {
            get
            {
                if (_originaltype == null)
                    _originaltype = _netheader.BlobHeap.ReadTypeSignature(Signature, this);
                return _originaltype;
            }
            internal set { _originaltype = value; }
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
                    return Convert.ToUInt32(_metadatarow._parts[0]);
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
