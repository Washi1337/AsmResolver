using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EnCLog : MetaDataMember 
    {
        public uint Token
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public uint FuncCode
        {
            get { return Convert.ToUInt32(metadatarow.parts[1]); }
        }
        public override void ClearCache()
        {

        }
    }
}
