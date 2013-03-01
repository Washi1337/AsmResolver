using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ParamPtr : MetaDataMember
    {
        ParameterDefinition reference;

        public ParameterDefinition Reference
        {
            get
            {
                if (reference == null)
                {
                    uint rowIndex = Convert.ToUInt32(metadatarow.parts[0]);
                    MetaDataTable table =netheader.TablesHeap.GetTable(MetaDataTableType.Param);
                    if (rowIndex > 0 && rowIndex <= table.AmountOfRows)
                        reference = table.Members[rowIndex] as ParameterDefinition;
                }
                return reference;
            }
        }

        public override void ClearCache()
        {
            reference = null;
        }
    }
}
