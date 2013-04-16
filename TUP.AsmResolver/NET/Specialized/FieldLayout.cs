using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldLayout : MetaDataMember
    {
        FieldDefinition field;

        public FieldLayout(MetaDataRow row)
            : base(row)
        {
        }

        public FieldLayout(FieldDefinition field, uint offset)
            : base(new MetaDataRow(offset, field.TableIndex))
        {
            this.field = field;
        }

        public uint Offset
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public FieldDefinition Field
        {
            get
            {
                if (field == null)
                    field = (FieldDefinition)netheader.TablesHeap.GetTable(MetaDataTableType.Field).Members[Convert.ToInt32(metadatarow.parts[1])];
                return field;
            }
        }
        public override void ClearCache()
        {
            field = null;
        }
    }
}
