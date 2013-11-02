using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldReference : MemberReference
    {
        internal FieldSignature _signature = null;
        internal string _name;

        public FieldReference(MetaDataRow row)
            : base(row)
        {
        }

        public FieldReference(string name, TypeReference declaringType, uint signature)
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
                if (_signature != null)
                    return _signature;
                _signature = (FieldSignature)_netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(_metadatarow._parts[2]), this.DeclaringType);
                return _signature;
            }
        }

        public virtual FieldDefinition Resolve()
        {
            if (NETHeader == null)
                return null;
            return NETHeader.MetaDataResolver.ResolveField(this);
        }

        public override void ClearCache()
        {
            _signature = null;
            _declaringType = null;
            _name = null;
        }

        public override void LoadCache()
        {
            _signature = Signature;
            _declaringType = DeclaringType;
            _name = Name;
        }
    }
}
