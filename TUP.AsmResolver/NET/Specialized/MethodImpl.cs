using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodImplementation : MetaDataMember 
    {
        public uint Class
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        public uint MethodBody
        {
            get { return Convert.ToUInt32(metadatarow.parts[1]); }
        }
        public uint OriginalMethod
        {
            get { return Convert.ToUInt32(metadatarow.parts[2]); }
        }
        public override void ClearCache()
        {

        }
    }
}
