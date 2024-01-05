using AsmResolver.Collections;

namespace AsmResolver.DotNet.Collections
{
    /// <summary>
    /// Represents an indexed member collection where each member is owned by some object, and prevents the member from
    /// being added to any other instance of the collection.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner object.</typeparam>
    /// <typeparam name="TMember">The type of elements to store.</typeparam>
    public class MemberCollection<TOwner, TMember> : OwnedCollection<TOwner, TMember>
        where TOwner : class
        where TMember : class, IOwnedCollectionElement<TOwner>
    {
        internal MemberCollection(TOwner owner)
            : base(owner)
        {
        }

        internal MemberCollection(TOwner owner, int capacity)
            : base(owner, capacity)
        {
        }

        internal void AddNoOwnerCheck(TMember member)
        {
            member.Owner = Owner;
            Items.Add(member);
        }
    }
}
