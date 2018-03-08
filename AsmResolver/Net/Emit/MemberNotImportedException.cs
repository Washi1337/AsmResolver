using System;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Emit
{
    public class MemberNotImportedException : Exception
    {
        public MemberNotImportedException(IMetadataMember member)
            : base(string.Format("The member {0} is not imported into the assembly.", member))
        {
            Member = member;
        }

        public IMetadataMember Member
        {
            get;
            private set;
        }

    }
}