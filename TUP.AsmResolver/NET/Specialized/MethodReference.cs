using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodReference : MemberReference, IGenericParamProvider, IGenericContext
    {
        internal MethodSignature _signature = null;
        internal string _name = null;
        internal GenericParameter[] _genericParameters = new GenericParameter[0];

        public MethodReference(MetaDataRow row)
            : base(row)
        {
        }

        public MethodReference(string name, TypeReference declaringType, uint signature)
            : base(new MetaDataRow(0U, 0U, signature))
        {
            this._name = name;
            this._declaringType = declaringType;
        }


        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[1]), out _name);
                return _name;
            }
        }

        public virtual MethodSignature Signature
        {
            get
            {
                if (_signature != null)
                    return _signature;
                _signature = (MethodSignature)_netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(_metadatarow._parts[2]), this);
                return _signature;
                //return Convert.ToUInt32(metadatarow.parts[2]); 
            }
        }

        public virtual bool IsGenericMethod
        {
            get { return false; }
        }

        public virtual GenericParameter[] GenericParameters
        {
            get
            {
                return _genericParameters;
            }
        }
        
        public bool HasGenericParameters
        {
            get { return GenericParameters != null && GenericParameters.Length > 0; }
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

        public virtual MethodDefinition Resolve()
        {
            if (NETHeader == null)
                return null;
            return NETHeader.MetaDataResolver.ResolveMethod(this);
        }

        public override string ToString()
        {
            return FullName;
        }

        public override void ClearCache()
        {
            _signature = null;
            _name = null;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            _signature = Signature;
            _name = Name;
        }

        IGenericParamProvider IGenericContext.Method
        {
            get { return this; }
        }

        IGenericParamProvider IGenericContext.Type
        {
            get { return this.DeclaringType; }
        }

        GenericParamType IGenericParamProvider.ParamType
        {
            get { return GenericParamType.Method; }
        }
        
        void IGenericParamProvider.AddGenericParameter(GenericParameter parameter)
        {
            Array.Resize(ref _genericParameters, _genericParameters.Length + 1);
            _genericParameters[_genericParameters.Length - 1] = parameter;
        }
    }
}
