using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ParamPtr : MetaDataMember
    {
        private ParameterDefinition _reference;

        public ParamPtr(MetaDataRow row)
            : base(row)
        {
        }

        public ParamPtr(ParameterDefinition reference)
            : base(new MetaDataRow(reference.TableIndex))
        {
            this._reference = reference;
        }

        public ParameterDefinition Reference
        {
            get
            {
                if (_reference == null)
                {
                    uint rowIndex = Convert.ToUInt32(_metadatarow._parts[0]);
                    MetaDataTable table =_netheader.TablesHeap.GetTable(MetaDataTableType.Param);
                    if (rowIndex > 0 && rowIndex <= table.AmountOfRows)
                        _reference = table.Members[rowIndex] as ParameterDefinition;
                }
                return _reference;
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
