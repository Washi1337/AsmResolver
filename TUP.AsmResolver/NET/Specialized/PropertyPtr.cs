using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PropertyPtr : MetaDataMember 
    {
        private PropertyDefinition _reference;

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
                if (_reference == null && _netheader.TablesHeap.HasTable(MetaDataTableType.Property))
                {
                    MetaDataTable propertyTable = _netheader.TablesHeap.GetTable(MetaDataTableType.Property);
                    uint index =  Convert.ToUInt32(_metadatarow._parts[0]);
                    if (index < propertyTable.AmountOfRows)
                        _reference = propertyTable.Members[index] as PropertyDefinition;
                }
                return _reference;
            }
            set
            {
                _reference = value;
                _metadatarow._parts[0] = value.TableIndex;
            }
        }

        public override void ClearCache()
        {
            _reference = null;
        }

        public override void LoadCache()
        {
            _reference = Reference;
        }
    }
}
