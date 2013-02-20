using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldRVA : MetaDataMember
    {
        public uint RVA
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public uint Field
        {
            get { return Convert.ToUInt32(metadatarow.parts[1]); }
        }
        public override void ClearCache()
        {
            
        }
    }
}
