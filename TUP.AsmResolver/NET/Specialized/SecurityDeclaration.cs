using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class SecurityDeclaration : MetaDataMember
    {
        MetaDataMember parent;

        public SecurityDeclaration(MetaDataRow row)
            : base(row)
        {
        }

        public SecurityDeclaration(SecurityAction Action, MetaDataMember parent, uint permissionSet)
            : base(new MetaDataRow((ushort)Action, 0U, permissionSet))
        {
        }

        public SecurityAction Action
        {
            get { return (SecurityAction)Convert.ToUInt16(metadatarow.parts[0]); }
        }

        public MetaDataMember Parent
        {
            get
            {
                if (parent == null)
                    parent = netheader.TablesHeap.HasDeclSecurity.GetMember(Convert.ToInt32(metadatarow.parts[1]));
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
