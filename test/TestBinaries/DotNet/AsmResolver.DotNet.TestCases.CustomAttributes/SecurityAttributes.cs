using System.Security.Permissions;

namespace AsmResolver.DotNet.TestCases.CustomAttributes
{
    public class SecurityAttributes
    {
        [CustomCodeAccessSecurity(SecurityAction.Assert)]
        public void NoParameters()
        {
        }
        
        [CustomCodeAccessSecurity(SecurityAction.Assert, PropertyA = 1)]
        public void SingleParameter()
        {
        }
        
        [CustomCodeAccessSecurity(SecurityAction.Assert, PropertyA = 1, PropertyB = 2, PropertyC = 3)]
        public void MultipleParameters()
        {
        }
    }
}