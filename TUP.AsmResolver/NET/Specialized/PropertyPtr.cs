using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PropertyPtr : MetaDataMember 
    {
        PropertyDefinition reference;

        public PropertyPtr(MetaDataRow row)
            : base(row)
        {
        }

        public PropertyPtr(uint reference)
            : base(new MetaDataRow(reference))
        {
        }

        public PropertyDefinition Reference
        {
            get
            {
                if (reference == null && netheader.TablesHeap.HasTable(MetaDataTableType.Property))
                {
                    MetaDataTable propertyTable = netheader.TablesHeap.GetTable(MetaDataTableType.Property);
                    uint index =  Convert.ToUInt32(metadatarow.parts[0]);
                    if (index < propertyTable.AmountOfRows)
                        reference = propertyTable.Members[index] as PropertyDefinition;
                }
                return reference;
            }
            set
            {
                reference = value;
                metadatarow.parts[0] = value.TableIndex;
            }
        }

        public override void ClearCache()
        {
            reference = null;
        }
    }
}
