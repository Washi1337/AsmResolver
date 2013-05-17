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
        //    System.Diagnostics.Debugger.Break();
            this.name = typeRef.Name;
            if (rank == 0)
                this.name += "[]";
            else
                for (int i = 0; i < rank; i++)
                    this.name = this.name + dimensions[i].ToString();
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
                return this.name;
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
