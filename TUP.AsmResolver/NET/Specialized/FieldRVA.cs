using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldRVA : MetaDataMember
    {
        FieldDefinition field;

        public FieldRVA(MetaDataRow row)
            : base(row)
        {
        }

        public FieldRVA(FieldDefinition field, uint rva)
            : base(new MetaDataRow(rva, field.TableIndex))
        {
            this.field = field;
        }

        public uint RVA
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }

        public FieldDefinition Field
        {
            get
            {
                if (field == null)
                {
                    int index = Convert.ToInt32(metadatarow.parts[0]);
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.Field);
                    if (index > 0 && index <= table.members.Length)
                        field = table.members[index - 1] as FieldDefinition;
                }
                return field;
            }
        }

        public override void ClearCache()
        {
            field = null;
        }
    }
}
