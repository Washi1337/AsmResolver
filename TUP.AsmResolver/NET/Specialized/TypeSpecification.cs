using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeSpecification : TypeReference , ISpecification
    {
        internal TypeSpecification()
        {
        }

        private TypeReference originaltype;

        public TypeSpecification(TypeReference typeRef)
        {
            originaltype = typeRef;
            netheader = typeRef.netheader;
            IsArray = typeRef.IsArray;
            IsPointer = typeRef.IsPointer;
            IsByReference = typeRef.IsByReference;
            IsDefinition = typeRef.IsDefinition;
            IsGenericInstance = typeRef.IsGenericInstance;
            IsPinned = typeRef.IsPinned;
            IsValueType = typeRef.IsValueType;
        }

        public MemberReference TransformWith(IGenericParametersProvider paramProvider, IGenericArgumentsProvider argProvider)
        {
            return netheader.BlobHeap.ReadTypeSignature(Signature, paramProvider, argProvider); 
        }

        public TypeReference OriginalType
        {
            get
            {
                if (originaltype == null)
                    originaltype = netheader.BlobHeap.ReadTypeSignature(Signature, this, this);
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
            get {
                if (HasSavedMetaDataRow)
                    return Convert.ToUInt32(metadatarow.parts[0]);
                else
                    return 0;
            }
        }

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
