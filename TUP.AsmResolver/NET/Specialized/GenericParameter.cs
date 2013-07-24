using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericParameter : TypeReference
    {
        internal IGenericParamProvider owner = null;
        
        public GenericParameter(MetaDataRow row)
            : base(row)
        {
        }

        public GenericParameter(IGenericParamProvider owner, int index)
            :this(string.Empty, (ushort)index, GenericParameterAttributes.NonVariant, owner)
        {
        }

        public GenericParameter(string name, ushort index, GenericParameterAttributes attributes, IGenericParamProvider owner)
            : base(new MetaDataRow(index, (ushort)attributes, 0U, 0U))
        {
            this._name = name;
            if (string.IsNullOrEmpty(name))
            {
                this._name = string.Format("{0}{1}", owner.ParamType == GenericParamType.Type ? "!" : "!!", index);
            }
            this.owner = owner;
        }

        public ushort Index
        {
            get { return Convert.ToUInt16(metadatarow._parts[0]); }
            set
            {
                metadatarow._parts[0] = value;
            }
        }

        public GenericParameterAttributes GenericAttributes
        {
            get { return (GenericParameterAttributes)Convert.ToUInt16(metadatarow._parts[1]); }
        }

        public IGenericParamProvider Owner
        {
            get 
            {
                if (owner == null && Convert.ToInt32(metadatarow._parts[2]) != 0)
                    netheader.TablesHeap.TypeOrMethod.TryGetMember(Convert.ToInt32(metadatarow._parts[2]), out owner);
                return owner;
            }
        }

        public override string Name
        {
            get {
                if (string.IsNullOrEmpty(_name))
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow._parts[3]), out _name);
                return _name;
            }
        }

        public override string Namespace
        {
            get
            {
                return "";
            }
        }

        public override string FullName
        {
            get
            {
                return Name;
            }
        }

        public override void LoadCache()
        {
            base.LoadCache();
            owner = Owner;
        }

    }
}
