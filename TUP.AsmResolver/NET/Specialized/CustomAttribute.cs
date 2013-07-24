using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class CustomAttribute : MetaDataMember
    {
        MetaDataMember _parent;
        MethodReference _constructor;
        CustomAttributeSignature _signature;

        public CustomAttribute(MetaDataRow row)
            : base(row)
        {
        }

        public CustomAttribute(MetaDataMember parent, MethodReference constructor, uint value)
            :base(new MetaDataRow(0U,0U, value))
        {
            this._parent = parent;
            this._constructor = constructor;
        }

        public MetaDataMember Parent
        {
            get
            {
                if (_parent == null)
                {
                    _netheader.TablesHeap.HasCustomAttribute.TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _parent);
                }
                return _parent;
            }
        }

        public MethodReference Constructor
        {
            get
            {
                if (_constructor == null)
                {
                    _netheader.TablesHeap.CustomAttributeType.TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _constructor);
                }
                return _constructor;
            }
        }

        public uint Value
        {
            get { return Convert.ToUInt32(_metadatarow._parts[2]); }
        }

        public CustomAttributeSignature Signature
        {
            get
            {
                if (_signature == null)
                {
                    _signature = _netheader.BlobHeap.ReadCustomAttributeSignature(this, Value);
                }
                return _signature;
            }
        }
    
        public override string ToString()
        {
            return Constructor.FullName;
        }

        public override void ClearCache()
        {
            _parent = null;
            _constructor = null;
            _signature = null;
        }

        public override void LoadCache()
        {
            _parent = Parent;
            _constructor = Constructor;
            _signature = Signature;
        }
    }
}
