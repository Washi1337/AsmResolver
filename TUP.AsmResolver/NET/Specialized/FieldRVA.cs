using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldRVA : MetaDataMember
    {
        FieldDefinition _field;

        public FieldRVA(MetaDataRow row)
            : base(row)
        {
        }

        public FieldRVA(FieldDefinition field, uint rva)
            : base(new MetaDataRow(rva, field.TableIndex))
        {
            this._field = field;
        }

        public uint RVA
        {
            get { return Convert.ToUInt32(_metadatarow._parts[0]); }
        }

        public FieldDefinition Field
        {
            get
            {
                if (_field == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.Field).TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _field);
                }
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
