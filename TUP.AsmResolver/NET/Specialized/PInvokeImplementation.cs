using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PInvokeImplementation : MetaDataMember 
    {
        MetaDataMember member=null;
        string entrypoint = null;
        ModuleReference importScope = null;

        public PInvokeImplAttributes Attributes
        {
            get { return (PInvokeImplAttributes)Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public MetaDataMember Member
        {
            get
            {
                if (member == null)
                    member = netheader.TablesHeap.MemberForwarded.GetMember(Convert.ToInt32(metadatarow.parts[1]));
                return member;
            }
        }
        public string Entrypoint
        {
            get
            {
                if (entrypoint == null)
                    entrypoint = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[2]));
                return entrypoint;
            }
        }
        public ModuleReference ImportScope
        {
            get 
            {
                if (importScope == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.ModuleRef);

                    importScope = (ModuleReference)table.Members[Convert.ToInt32(metadatarow.parts[3]) - 1];
                }
                return importScope; 
            }
        }
        public override string ToString()
        {
            return Entrypoint;
        }
        public override void ClearCache()
        {
            member = null;
            entrypoint = null;
            importScope = null;
        }

    }
}
