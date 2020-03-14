using System.Collections.Generic;

namespace AsmResolver.DotNet.Cloning
{
    public class MetadataCloneResult
    {
        public MetadataCloneResult(IEnumerable<IMemberDescriptor> clonedMembers)
        {
            ClonedMembers = new List<IMemberDescriptor>(clonedMembers);
        }

        public ICollection<IMemberDescriptor> ClonedMembers
        {
            get;
        }
    }
}