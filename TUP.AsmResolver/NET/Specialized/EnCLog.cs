using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EnCLog : MetaDataMember 
    {
        public EnCLog(MetaDataRow row)
            : base(row)
        {
        }

        public EnCLog(uint token, uint funcCode)
            :base(new MetaDataRow(token,funcCode))
        {
        }

        public uint Token
        {
            get { return Convert.ToUInt32(_metadatarow._parts[0]); }
        }

        public uint FuncCode
        {
            get { return Convert.ToUInt32(_metadatarow._parts[1]); }
        }

        public override void ClearCache()
        {
        }

        public override void LoadCache()
        {
        }

    }
}
