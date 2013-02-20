using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ClassLayout : MetaDataMember
    {
        public ushort PackingSize
        {
            get { return Convert.ToUInt16(metadatarow.parts[0]); }
        }
        public uint ClassSize
        {
            get { return Convert.ToUInt32(metadatarow.parts[1]); }
        }
        public uint Parent
        {
            get { return Convert.ToUInt32(metadatarow.parts[2]); }
        }

        public override void ClearCache()
        {

        }
    }
}
