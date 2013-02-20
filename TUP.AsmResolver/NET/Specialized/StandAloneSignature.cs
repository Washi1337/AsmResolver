using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class StandAloneSignature : MetaDataMember 
    {
        public uint Signature
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public override void ClearCache()
        {

        }
    }
}
