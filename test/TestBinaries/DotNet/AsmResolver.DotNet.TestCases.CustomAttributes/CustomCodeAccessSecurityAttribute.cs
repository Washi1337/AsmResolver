using System.Security;
using System.Security.Permissions;

namespace AsmResolver.DotNet.TestCases.CustomAttributes
{
    public class CustomCodeAccessSecurityAttribute : CodeAccessSecurityAttribute
    {
        public CustomCodeAccessSecurityAttribute(SecurityAction action)
            : base(action)
        {
        }

        public int PropertyA
        {
            get;
            set;
        }

        public int PropertyB
        {
            get;
            set;
        }

        public int PropertyC
        {
            get;
            set;
        }

        public override IPermission CreatePermission()
        {
            return null;
        }
    }
}