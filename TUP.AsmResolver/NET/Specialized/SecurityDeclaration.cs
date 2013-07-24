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
            get { return (SecurityAction)Convert.ToUInt16(metadatarow._parts[0]); }
        }

        public MetaDataMember Parent
        {
            get
            {
                if (parent == null)
                    netheader.TablesHeap.HasDeclSecurity.TryGetMember(Convert.ToInt32(metadatarow._parts[1]), out parent);
                return parent;
            }
        }

        public uint PermissionSet
        {
            get { return Convert.ToUInt32(metadatarow._parts[2]); }
        }

        public override void ClearCache()
        {
            parent = null;
        }

        public override void LoadCache()
        {
            parent = Parent;
        }
    }
}
