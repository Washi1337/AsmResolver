using System;

namespace AsmResolver.DotNet.Builder
{
    public class MemberNotImportedException : Exception
    {
        public MemberNotImportedException(IMemberDescriptor member)
            : base($"Member {member} was not imported into the module.")
        {
            Member = member;
        }
        
        public IMemberDescriptor Member
        {
            get;
        }
    }
}