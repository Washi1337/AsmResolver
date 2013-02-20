using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ArrayDimension
    {
        public ArrayDimension(int? lowerBound, int? upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }
        public bool Sized
        {
            get
            {
                return LowerBound.HasValue || UpperBound.HasValue;
            }
        }
        public int? LowerBound
        {
            get;
            internal set;
        }
        public int? UpperBound
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            if (Sized)
                return "[" + LowerBound.ToString() + "..." + UpperBound.ToString() + "]";
            else
                return "[]";


            

        }
    }
}
