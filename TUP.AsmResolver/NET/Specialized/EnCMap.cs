using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EnCMap : MetaDataMember
    {
        public EnCMap(MetaDataRow row)
            : base(row)
        {
        }

        public EnCMap(uint token)
            : base(new MetaDataRow(token))
        {
        }

        public uint Token
        {
            get { return Convert.ToUInt32(_metadatarow._parts[0]); }
        }

        public override void ClearCache()
        {
        }

        public override void LoadCache()
        {
        } 
    }
}
