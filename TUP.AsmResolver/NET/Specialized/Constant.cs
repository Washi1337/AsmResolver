using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class Constant : MetaDataMember
    {
        object _value;
        MetaDataMember _parent;

        public Constant(MetaDataRow row)
            : base(row)
        {
        }

        public Constant(MetaDataMember parent, ElementType type, uint signature)
            : base(new MetaDataRow((byte)type, 0U, signature))
        {
        }

        public ElementType ConstantType
        {
            get { return (Specialized.ElementType)Convert.ToByte(_metadatarow._parts[0]); }
        }

        public MetaDataMember Parent
        {
            get 
            {
                if (_parent != null || _netheader.TablesHeap.HasConstant.TryGetMember(Convert.ToInt32(_metadatarow._parts[2]), out _parent))
                    return _parent;
                
                return null;
            }
        }

        public uint Signature
        {
            get { return Convert.ToUInt32(_metadatarow._parts[3]); }
        }

        public object Value
        {
            get
            {
                if (_value == null)
                    _value = _netheader.BlobHeap.ReadConstantValue(ConstantType, Signature);
                return _value;

            }
        }

        public override void ClearCache()
        {
            _value = null;
            _parent = null;
        }

        public override void LoadCache()
        {
            _value = Value;
            _parent = Parent;
        }
    }
}
