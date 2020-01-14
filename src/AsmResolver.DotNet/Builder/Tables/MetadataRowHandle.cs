namespace AsmResolver.DotNet.Builder.Tables
{
    /// <summary>
    /// Represents a reference to a metadata row that is not yet assigned a definite metadata token or row identifier. 
    /// </summary>
    public struct MetadataRowHandle
    {
        /// <summary>
        /// Creates a new handle for a metadata row.
        /// </summary>
        /// <param name="value">The value used to reference the row.</param>
        public MetadataRowHandle(int value)
        {
            Id = value;
        }

        /// <summary>
        /// Gets the raw reference to the metadata row.
        /// </summary>
        /// <remarks>
        /// The actual meaning of this property depends on the type of table buffer that is used.
        /// </remarks>
        public int Id
        {
            get;
        }

        /// <summary>
        /// Determines whether the handle is equal to another.
        /// </summary>
        /// <param name="other">The other handle.</param>
        /// <returns><c>true</c> if the handles are considered equal, <c>false</c> otherwise.</returns>
        public bool Equals(MetadataRowHandle other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is MetadataRowHandle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Id.ToString();
        }
    }
}