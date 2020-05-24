namespace AsmResolver.DotNet.Collections
{
    /// <summary>
    /// Represents an element in a collection owned by a single object.
    /// </summary>
    /// <typeparam name="TOwner">The type of the object that owns the collection.</typeparam>
    public interface IOwnedCollectionElement<TOwner>
    {
        /// <summary>
        /// Gets or sets the owner of the collection.
        /// </summary>
        /// <remarks>
        /// This property should not be assigned directly.
        /// </remarks>
        TOwner Owner
        {
            get;
            set;
        }
    }
}