using System;

namespace AsmResolver
{
    /// <summary>
    /// Represents a reference to a segment in a binary file, such as the beginning of a function or method body, or
    /// a reference to a chunk of initialization data of a field. 
    /// </summary>
    public interface ISegmentReference : IOffsetProvider
    {
        /// <summary>
        /// Gets a value indicating whether the referenced segment can be read using a binary reader.
        /// </summary>
        bool CanRead
        {
            get;
        }
        
        /// <summary>
        /// Gets a value indicating whether the referenced segment is bounded to a fixed size.
        /// </summary>
        bool IsBounded
        {
            get;
        }
        
        /// <summary>
        /// Creates a binary reader starting at the beginning of the segment.
        /// </summary>
        /// <returns>The binary reader.</returns>
        /// <remarks>
        /// When <see cref="CanRead"/> is <c>false</c>, it is not guaranteed this method will succeed.
        /// </remarks>
        IBinaryStreamReader CreateReader();

        /// <summary>
        /// Obtains the segment referenced by this reference. 
        /// </summary>
        /// <returns>The segment.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the segment could not be obtained.</exception>
        /// <remarks>
        /// When <see cref="IsBounded"/> is <c>false</c>, it is not guaranteed this method will succeed.
        /// </remarks>
        ISegment GetSegment();
    }
}