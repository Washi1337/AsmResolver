using System.Collections.Generic;

namespace AsmResolver
{
    public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public static ByteArrayEqualityComparer Instance
        {
            get;
        } = new ByteArrayEqualityComparer();
        
        public unsafe bool Equals(byte[] x, byte[] y)
        {
            // Original code by Hafthor Stefansson
            // Copyright (c) 2008-2013
            
            if (x == y)
                return true;
            if (x == null || y == null || x.Length != y.Length)
                return false;
            
            fixed (byte* p1 = x, p2 = y)
            {
                byte* x1 = p1;
                byte* x2 = p2;
                int length = x.Length;
                
                for (int i = 0; i < length / sizeof(long); i++, x1 += sizeof(long), x2 += sizeof(long))
                {
                    if (*(long*) x1 != *(long*) x2)
                        return false;
                }

                if ((length & sizeof(int)) != 0)
                {
                    if (*(int*) x1 != *(int*) x2)
                        return false;
                    
                    x1 += sizeof(int);
                    x2 += sizeof(int);
                }

                if ((length & sizeof(short)) != 0)
                {
                    if (*(short*) x1 != *(short*) x2)
                        return false;
                    
                    x1 += sizeof(short);
                    x2 += sizeof(short);
                }

                if ((length & sizeof(byte)) != 0)
                {
                    if (*x1 != *x2)
                        return false;
                }

                return true;
            }
        }

        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                int result = 0;
                foreach (byte b in obj)
                    result = (result * 31) ^ b;
                return result;
            }
        }
    }
}