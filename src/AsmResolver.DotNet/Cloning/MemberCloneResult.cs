using System.Collections.Generic;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides information about the result of a metadata cloning procedure.
    /// </summary>
    public class MemberCloneResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MemberCloneResult"/> class.
        /// </summary>
        /// <param name="clonedMembers">The cloned members.</param>
        public MemberCloneResult(IEnumerable<IMemberDescriptor> clonedMembers)
        {
            ClonedMembers = new List<IMemberDescriptor>(clonedMembers);
        }

        /// <summary>
        /// Gets a collection of all cloned members.
        /// </summary>
        public ICollection<IMemberDescriptor> ClonedMembers
        {
            get;
        }
    }
}