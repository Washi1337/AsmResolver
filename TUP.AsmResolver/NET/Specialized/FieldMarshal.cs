using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldMarshal : MetaDataMember
    {
        MetaDataMember _parent = null;

        public FieldMarshal(MetaDataRow row)
            : base(row)
        {
        }

        public FieldMarshal(MetaDataMember parent, uint nativeType)
            :base(new MetaDataRow((uint)0, nativeType))
        {
            this._parent = parent;
        }

        public MetaDataMember Parent
        {
            get
            {
                if (_parent == null)
                {
                    _netheader.TablesHeap.HasFieldMarshall.TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _parent);
                }
                return _parent;
                
            }
        }

        public uint NativeType
        {
            get { return Convert.ToUInt32(_metadatarow._parts[1]); }
        }

        public override void ClearCache()
        {
            _parent = null;
        }

        public override void LoadCache()
        {
            _parent = Parent;
        }
    }
}
