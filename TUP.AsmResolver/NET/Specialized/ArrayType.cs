using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ArrayType : TypeSpecification
    {
        public ArrayType(TypeReference typeRef) : this (typeRef, 0,  null)
        {
            
        }
        public ArrayType(TypeReference typeRef, int rank, ArrayDimension[] dimensions) :base (typeRef)
        {
            Rank = rank;
            Dimensions = dimensions;
        }
        public bool IsVector
        {
            get
            {
                return (Dimensions != null && Dimensions.Length == 1 && Dimensions[0].LowerBound == 0);
            }

        }
        public override bool IsArray
        {
            get
            {
                return true;
            }
        }
        public override string Name
        {
            get
            {
                string name = base.Name;
                if (Rank == 0)
                    name += "[]";
                else
                    for (int i = 0; i < Rank; i++)
                        name += Dimensions[i].ToString();
                return name;
            }
        }
       //public override string FullName
       //{
       //    get { return (Namespace == "" ? "" : Namespace + ".") + Name; }
       //}
        public int Rank
        {
            get;
            private set;
        }

        public ArrayDimension[] Dimensions
        {
            get;
            private set;
        }
        public override string ToString()
        {
            return FullName;
        }
    }
}
