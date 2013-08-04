using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ClassLayout : MetaDataMember
    {
        TypeDefinition _parent;

        public ClassLayout(MetaDataRow row)
            : base(row)
        {
        }

        public ClassLayout(TypeDefinition parent, uint classSize, ushort packingSize)
            :base(new MetaDataRow(packingSize, classSize, parent.TableIndex))
        {
            this._parent = parent;
        }

        public ushort PackingSize
        {
            get { return Convert.ToUInt16(_metadatarow._parts[0]); }
        }

        public uint ClassSize
        {
            get { return Convert.ToUInt32(_metadatarow._parts[1]); }
        }

        public TypeDefinition Parent
        {
            get 
            {
                if (_parent == null)
                    _netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef, false).TryGetMember(Convert.ToInt32(_metadatarow._parts[2]), out _parent);
                return _parent;
            }
        }

        public override void ClearCache()
        {
            this._parent = null;
        }

        public override void LoadCache()
        {
            this._parent = Parent;
        }
    }
}
