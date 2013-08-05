using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldLayout : MetaDataMember
    {
        FieldDefinition _field;

        public FieldLayout(MetaDataRow row)
            : base(row)
        {
        }

        public FieldLayout(FieldDefinition field, uint offset)
            : base(new MetaDataRow(offset, field.TableIndex))
        {
            this._field = field;
        }

        public uint Offset
        {
            get { return Convert.ToUInt32(_metadatarow._parts[0]); }
        }

        public FieldDefinition Field
        {
            get
            {
                if (_field == null)
                    _netheader.TablesHeap.GetTable(MetaDataTableType.Field).TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _field);
                return _field;
            }
        }

        public override void ClearCache()
        {
            _field = null;
        }

        public override void LoadCache()
        {
            _field = Field;
        }
    }
}
