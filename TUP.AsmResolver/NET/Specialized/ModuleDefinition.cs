using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ModuleDefinition : MetaDataMember
    {
        string name = null;
        Guid mvid = default(Guid);
        public string Name
        {
            get
            {
                if (name == null)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]));
                return name;
            }
        }
        public Guid Mvid
        {
            get
            {
                if (mvid == default(Guid))
                    mvid = netheader.GuidHeap.GetGuidByOffset(Convert.ToUInt32(metadatarow.parts[2]));
                return mvid;
            }
        }
        public override string ToString()
        {
            return Name;
        }
        public override void ClearCache()
        {
            name = null;
            mvid = default(Guid);
        }
    }
}
