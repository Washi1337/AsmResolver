using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides information about the result of a metadata cloning procedure.
    /// </summary>
    public class MemberCloneResult
    {
        private readonly IDictionary<IMemberDescriptor, IMemberDescriptor> _clonedMembers;
        private readonly IList<TypeDefinition> _topLevelTypes;

        /// <summary>
        /// Creates a new instance of the <see cref="MemberCloneResult"/> class.
        /// </summary>
        /// <param name="clonedMembers">The cloned members.</param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="clonedMembers"/> is null.</exception>
        public MemberCloneResult(IDictionary<IMemberDescriptor, IMemberDescriptor> clonedMembers)
        {
            _clonedMembers = clonedMembers ?? throw new ArgumentNullException(nameof(clonedMembers));
            ClonedMembers = new List<IMemberDescriptor>(clonedMembers.Values);
            OriginalMembers = new List<IMemberDescriptor>(clonedMembers.Keys);
            _topLevelTypes = clonedMembers.Values.OfType<TypeDefinition>().Where(type => !type.IsNested).ToList();
        }

        /// <summary>
        /// Gets a collection of all cloned members.
        /// </summary>
        public ICollection<IMemberDescriptor> ClonedMembers
        {
            get;
        }

        /// <summary>
        /// Gets a collection of all original members.
        /// </summary>
        public ICollection<IMemberDescriptor> OriginalMembers
        {
            get;
        }

        /// <summary>
        /// Verifies if the <paramref name="originalMember"/> is cloned by the <see cref="MemberCloner"/>.
        /// </summary>
        /// <param name="originalMember">The original <see cref="IMemberDescriptor"/></param>
        /// <returns><c>true</c> if the provided member was cloned, <c>false</c> otherwise.</returns>
        public bool ContainsClonedMember(IMemberDescriptor originalMember) => _clonedMembers.ContainsKey(originalMember);

        /// <summary>
        /// Gets the cloned <see cref="IMemberDescriptor"/> by its original <see cref="IMemberDescriptor"/>.
        /// </summary>
        /// <param name="originalMember">Original <see cref="IMemberDescriptor"/></param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <paramref name="originalMember"/> is not a member of <see cref="OriginalMembers"/></exception>
        /// <returns>Cloned <see cref="IMemberDescriptor"/></returns>
        public T GetClonedMember<T>(T originalMember) where T : IMemberDescriptor
        {
            if (!_clonedMembers.ContainsKey(originalMember))
                throw new ArgumentOutOfRangeException(nameof(originalMember));
            return (T)_clonedMembers[originalMember];
        }

        /// <summary>
        /// Gets all cloned members of type <see cref="TypeDefinition"/> that are not nested.
        /// </summary>
        /// <returns>List of the top level <see cref="TypeDefinition"/></returns>
        public IList<TypeDefinition> GetClonedTopLevelTypes()
        {
            return _topLevelTypes;
        }
    }
}