using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EnCMap : MetaDataMember
    {
        public uint Token
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public override void ClearCache()
        {

        }
    }
}
