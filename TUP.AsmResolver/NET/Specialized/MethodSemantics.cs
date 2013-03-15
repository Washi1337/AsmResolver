using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodSemantics : MetaDataMember 
    {
        MethodDefinition method;
        MetaDataMember association;

        public MethodSemanticsAttributes Attributes
        {
            get { return (MethodSemanticsAttributes)Convert.ToUInt16(metadatarow.parts[0]); }
        }
        public MethodDefinition Method
        {
            get 
            {
                if (method == null)
                {
                    MetaDataTable methodtable = netheader.TablesHeap.GetTable(MetaDataTableType.Method);
                    int index = Convert.ToInt32(metadatarow.parts[1]) - 1;
                    if (index >= 0 && index < methodtable.members.Count)
                        method = (MethodDefinition)methodtable.members[index];
                }
                return method;
            }
        }
        public MetaDataMember Association
        {
            get
            {
                if (association == null)
                    association = netheader.TablesHeap.HasSemantics.GetMember(Convert.ToInt32(metadatarow.parts[2]));
                return association;
            }
        }
        public override void ClearCache()
        {
            method = null;
            association = null;
        }

    }
}
