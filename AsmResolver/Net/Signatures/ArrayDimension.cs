
namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a single dimension in an array specification. 
    /// </summary>
    public class ArrayDimension 
    {
        public ArrayDimension()
        {
        }

        public ArrayDimension(int size)
            : this(size, null)
        {
        }

        public ArrayDimension(int? size, int? lowerBound)
        {
            Size = size;
            LowerBound = lowerBound;
        }

        /// <summary>
        /// Gets or sets the number of elements in the dimension (if specified).
        /// </summary>
        /// <remarks>
        /// When this value is not specified (<c>null</c>), no upper bound on the number of elements is assumed by the CLR.
        /// </remarks>
        public int? Size
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the lower bound for each index in the dimension (if specified). 
        /// </summary>
        /// <remarks>
        /// When this value is not specified (<c>null</c>), a lower bound of 0 is assumed by the CLR.
        /// </remarks>
        public int? LowerBound
        {
            get;
            set;
        }
    }
}
