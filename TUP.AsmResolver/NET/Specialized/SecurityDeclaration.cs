using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class SecurityDeclaration : MetaDataMember
    {
        private MetaDataMember _parent;

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
            get { return (SecurityAction)Convert.ToUInt16(_metadatarow._parts[0]); }
        }

        public MetaDataMember Parent
        {
            get
            {
                if (_parent == null)
                    _netheader.TablesHeap.HasDeclSecurity.TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _parent);
                return _parent;
            }
        }

        public uint PermissionSet
        {
            get { return Convert.ToUInt32(_metadatarow._parts[2]); }
        }

        public override void ClearCache()
        {
            _parent = null;
        }

        public override void LoadCache()
        {
            _parent = Parent;
        }
    }
}
