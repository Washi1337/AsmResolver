
namespace AsmResolver.Net.Signatures
{
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

        public int? Size
        {
            get;
            set;
        }

        public int? LowerBound
        {
            get;
            set;
        }
    }
}
