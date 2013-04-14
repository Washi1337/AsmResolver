using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class StandAloneSignature : MetaDataMember 
    {
        public StandAloneSignature(uint signature) : this(new MetaDataRow((object)signature))
        {
        }

        public StandAloneSignature(MetaDataRow row)
            : base(row)
        {
        }

        public uint Signature
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public override void ClearCache()
        {

        }
    }
}
