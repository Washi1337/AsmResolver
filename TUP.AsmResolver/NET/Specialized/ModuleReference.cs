using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ModuleReference : MetaDataMember, IResolutionScope
    {
        string name = null;

        public ModuleReference(MetaDataRow row)
            : base(row)
        {
        }

        public ModuleReference(string name)
            : base(new MetaDataRow((uint)0))
        {
        }

        public virtual string Name
        {
            get
            {
                if (name == null)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[0]));
                return name;
            }
        }
        public override string ToString()
        {
            return Name;
        }
        public override void ClearCache()
        {
            name = null;
        }
    }
}
