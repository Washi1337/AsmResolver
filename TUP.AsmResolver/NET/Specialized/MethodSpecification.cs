using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodSpecification : MethodReference, ISpecification, IGenericInstance
    {
        MethodReference _originalmethod;
        TypeReference[] _genericArgs;
        GenericParameter[] _genericParams;
        IGenericContext _context;

        public MethodSpecification(MetaDataRow row)
            : base(row)
        {
        }

        public MethodSpecification(MethodReference methodRef)
            : base(default(MetaDataRow))
        {
            OriginalMethod = methodRef;
            _context = methodRef;            
        }

        public MemberReference TransformWith(IGenericContext context)
        {
            if (this.IsGenericMethod)
            {
                MethodSpecification copy = this.MemberwiseClone() as MethodSpecification;
                copy._context = context;
                copy._metadatarow = this._metadatarow;
                copy._genericArgs = _netheader.BlobHeap.ReadGenericArgumentsSignature(copy.SpecificationSignature, context);
                uint signature = 0;
                if (copy.OriginalMethod.IsDefinition)
                    signature = Convert.ToUInt32(copy.OriginalMethod.MetaDataRow.Parts[4]);
                else
                    signature = Convert.ToUInt32(copy._originalmethod.MetaDataRow.Parts[2]);

                copy._signature = _netheader.BlobHeap.ReadMemberRefSignature(signature, copy) as MethodSignature;
                return copy;
            }
            return this;
        }

        public MethodReference OriginalMethod
        {
            get
            {
                if (_originalmethod == null)
                    _netheader.TablesHeap.MethodDefOrRef.TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _originalmethod);
                return _originalmethod;
            }
            private set { _originalmethod = value; }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (_originalmethod != null)
                    return _originalmethod.DeclaringType;
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
                if (_originalmethod != null)
                    return _originalmethod.Name;
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

        //public override GenericParameter[] GenericParameters
        //{
        //    get
        //    {
        //        return OriginalMethod.GenericParameters;
        //        //if (genericParams == null)
        //        //{
        //        //    genericParams = new GenericParameter[GenericArguments.Length];
        //        //    for (int i = 0; i < genericParams.Length; i++)
        //        //        genericParams[i] = new GenericParameter("!!" + i, (ushort)i, (GenericParameterAttributes)0, this);
        //        //}
        //        //return genericParams;
        //    }
        //}

        public TypeReference[] GenericArguments
        {
            get
            {
                if (_genericArgs == null)
                    _genericArgs = _netheader.BlobHeap.ReadGenericArgumentsSignature(SpecificationSignature, _context);
                return _genericArgs;

            }
        }
    
        public bool HasGenericArguments
        {
            get { return GenericArguments != null && GenericArguments.Length != 0; }
        }
         
        public override MethodSignature Signature
        {
            get
            {
                if (_signature == null)
                    _signature = OriginalMethod.Signature; //netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(OriginalMethod.metadatarow.parts[2]), this, this) as MethodSignature;
                return _signature;
            }
        }

        public uint SpecificationSignature
        {
            get { return Convert.ToUInt32(_metadatarow._parts[1]); }
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
