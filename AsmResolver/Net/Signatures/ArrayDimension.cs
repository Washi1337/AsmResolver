using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public class ArrayDimension 
    {
        public ArrayDimension()
        {
        }

        public ArrayDimension(int size)
        {
            Size = size;
        }

        public ArrayDimension(int size, int lowerBound)
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
