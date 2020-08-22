using System;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Represents the exception that occurs when an external metadata member was used in a .NET module, but was not
    /// imported in said module.
    /// </summary>
    public class MemberNotImportedException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MemberNotImportedException"/>.
        /// </summary>
        /// <param name="member">The member that was not imported.</param>
        public MemberNotImportedException(IMetadataMember member)
            : base($"Member {member.SafeToString()} was not imported into the module.")
        {
            Member = member;
        }
        
        /// <summary>
        /// Gets the member that was not imported.
        /// </summary>
        public IMetadataMember Member
        {
            get;
        }
    }
}