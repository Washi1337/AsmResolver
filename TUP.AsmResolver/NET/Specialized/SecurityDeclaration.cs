using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class SecurityDeclaration : MetaDataMember
    {
        MetaDataMember parent;
        public ushort Action
        {
            get { return Convert.ToUInt16(metadatarow.parts[0]); }
        }
        public MetaDataMember Parent
        {
            get
            {
                if (parent == null)
                    parent = tablereader.HasDeclSecurity.GetMember(Convert.ToInt32(metadatarow.parts[1]));
                return parent;
            }
        }
        public uint PermissionSet
        {
            get { return Convert.ToUInt32(metadatarow.parts[2]); }
        }
        public override void ClearCache()
        {
            parent = null;
        }
    }
}
