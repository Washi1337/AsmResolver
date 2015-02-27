using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Metadata
{
    public abstract class MetadataRow
    {
        public abstract object[] GetAllColumns();
    }

    public class MetadataRow<T1> : MetadataRow
    {
        public T1 Column1
        {
            get;
            set;
        }

        public override object[] GetAllColumns()
        {
            return new object[]
            {
                Column1
            };
        }
    }

    public class MetadataRow<T1, T2> : MetadataRow<T1>
    {
        public T2 Column2
        {
            get;
            set;
        }

        public override object[] GetAllColumns()
        {
            return new object[]
            {
                Column1,
                Column2
            };
        }
    }

    public class MetadataRow<T1, T2, T3> : MetadataRow<T1, T2>
    {
        public T3 Column3
        {
            get;
            set;
        }

        public override object[] GetAllColumns()
        {
            return new object[]
            {
                Column1,
                Column2,
                Column3
            };
        }
    }

    public class MetadataRow<T1, T2, T3, T4> : MetadataRow<T1, T2, T3>
    {
        public T4 Column4
        {
            get;
            set;
        }

        public override object[] GetAllColumns()
        {
            return new object[]
            {
                Column1,
                Column2,
                Column3,
                Column4,
            };
        }
    }

    public class MetadataRow<T1, T2, T3, T4, T5> : MetadataRow<T1, T2, T3, T4>
    {
        public T5 Column5
        {
            get;
            set;
        }

        public override object[] GetAllColumns()
        {
            return new object[]
            {
                Column1,
                Column2,
                Column3,
                Column4,
                Column5,
            };
        }
    }

    public class MetadataRow<T1, T2, T3, T4, T5, T6> : MetadataRow<T1, T2, T3, T4, T5>
    {
        public T6 Column6
        {
            get;
            set;
        }

        public override object[] GetAllColumns()
        {
            return new object[]
            {
                Column1,
                Column2,
                Column3,
                Column4,
                Column5,
                Column6,
            };
        }
    }

    public class MetadataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MetadataRow<T1, T2, T3, T4, T5, T6>
    {
        public T7 Column7
        {
            get;
            set;
        }

        public T8 Column8
        {
            get;
            set;
        }

        public T9 Column9
        {
            get;
            set;
        }

        public override object[] GetAllColumns()
        {
            return new object[]
            {
                Column1,
                Column2,
                Column3,
                Column4,
                Column5,
                Column6,
                Column7, 
                Column8, 
                Column9
            };
        }
    }
}
