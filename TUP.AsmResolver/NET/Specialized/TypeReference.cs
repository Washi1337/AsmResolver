using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeReference : MemberReference, IGenericParamProvider, IGenericContext, IResolutionScope
    {
        internal ElementType _elementType = ElementType.None;
        internal IResolutionScope _resolutionScope;
        internal string _name = string.Empty;
        internal string _namespace = string.Empty;
        internal string _fullname = string.Empty;
        internal GenericParameter[] _genericParameters = new GenericParameter[0];

        public TypeReference(MetaDataRow row)
            : base(row)
        {
        }

        public TypeReference(string @namespace, string name, IResolutionScope resolutionScope)
            : base(new MetaDataRow(0U, 0U, 0U))
        {
            this._name = name;
            this._namespace = @namespace;
            this._resolutionScope = resolutionScope;
        }

        public override TypeReference DeclaringType
        {
            get 
            {
                return ResolutionScope as TypeReference;
            }
        }

        public virtual IResolutionScope ResolutionScope
        {
            get
            {
                if (_resolutionScope != null || !HasSavedMetaDataRow)
                    return _resolutionScope;

                MetaDataMember member;
                _netheader.TablesHeap.ResolutionScope.TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out member);
                _resolutionScope = member as IResolutionScope;

                return _resolutionScope;
            }
        }
        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this._name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[1]), out _name);
                return this._name;
            }
        }

        public virtual string Namespace
        {
            get
            {
                if (HasSavedMetaDataRow)
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[2]), out _namespace);
                return _namespace;
            }
        }

        public override string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(_fullname))
                {

                    TypeReference declaringType = this.DeclaringType;
                    if (declaringType == null)
                    {
                        _fullname = (string.IsNullOrEmpty(Namespace) ? string.Empty : Namespace + ".") + Name;
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(Name);
                        while (declaringType != null)
                        {
                            builder.Insert(0, declaringType.FullName + "/");

                            declaringType = declaringType.DeclaringType;
                        }
                        _fullname = builder.ToString();
                    }
                }
                return _fullname;
            }
        }

        public virtual bool IsArray 
        { 
            get;
            internal set;
        }

        public virtual bool IsPointer 
        { 
            get;
            internal set; 
        }

        public virtual bool IsByReference 
        { 
            get;
            internal set; 
        }

        public virtual bool IsPinned 
        { 
            get;
            internal set; 
        }

        public virtual bool IsDefinition
        {
            get;
            internal set;
        }

        public virtual bool IsGenericInstance 
        {
            get; 
            internal set;
        }

        public virtual bool IsValueType
        {
            get;
            internal set;
        }

        public virtual bool IsElementType 
        { 
            get;
            internal set; 
        }

        public virtual bool IsNested
        {
            get
            {
                return DeclaringType != null;
            }
        }

        public virtual GenericParameter[] GenericParameters
        {
            get { return _genericParameters; }
        }

        public bool HasGenericParameters
        {
            get { return GenericParameters != null && GenericParameters.Length != 0; }
        }

        public virtual TypeReference GetElementType()
        {
            return this;
        }

        public virtual TypeDefinition Resolve()
        {
            if (NETHeader == null)
                return null;
            return NETHeader.MetaDataResolver.ResolveType(this);
        }

        public override void ClearCache()
        {
            _resolutionScope = null;
            _name = null;
            _namespace = null;
            _fullname = null;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            _resolutionScope = ResolutionScope;
            _name = Name;
            _namespace = Namespace;
            _fullname = FullName;
        }

        public override string ToString()
        {
            return FullName;
        }


        IGenericParamProvider IGenericContext.Method
        {
            get { return null; }
        }

        IGenericParamProvider IGenericContext.Type
        {
            get { return this; }
        }

        GenericParamType IGenericParamProvider.ParamType
        {
            get { return GenericParamType.Type; }
        }

        void IGenericParamProvider.AddGenericParameter(GenericParameter parameter)
        {
            Array.Resize(ref _genericParameters, _genericParameters.Length + 1);
            _genericParameters[_genericParameters.Length - 1] = parameter;
        }
    }
}
