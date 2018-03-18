using System;
using System.Linq;

namespace AsmResolver.Net.Metadata
{
    public abstract class MetadataRow
    {
        private MetadataToken _metadataToken;

        protected MetadataRow()
        {
        }
        
        protected MetadataRow(MetadataToken metadataToken)
        {
            _metadataToken = metadataToken;
        }

        public MetadataToken MetadataToken
        {
            get { return _metadataToken; }
            set
            {
                AssertIsWriteable();
                _metadataToken = value;
            }
        }

        public bool IsReadOnly
        {
            get;
            internal set;
        }

        public abstract object[] GetAllColumns();

        protected void AssertIsWriteable()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Metadata row cannot be modified in read-only mode.");
        }

        protected bool Equals(MetadataRow other)
        {
            return GetAllColumns().SequenceEqual(other.GetAllColumns());
        }
    }

    public class MetadataRow<T1> : MetadataRow
    {
        private T1 _column1;

        public MetadataRow()
        {
        }
        
        public MetadataRow(MetadataToken metadataToken)
            : base(metadataToken)
        {
        }

        public T1 Column1
        {
            get { return _column1; }
            set
            {
                AssertIsWriteable();
                _column1 = value;
            }
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
        private T2 _column2;

        public MetadataRow()
        {
        }

        public MetadataRow(MetadataToken metadataToken)
            : base(metadataToken)
        {
        }

        public T2 Column2
        {
            get { return _column2; }
            set
            {
                AssertIsWriteable();
                _column2 = value;
            }
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
        private T3 _column3;

        public MetadataRow()
        {
        }

        public MetadataRow(MetadataToken metadataToken)
            : base(metadataToken)
        {
        }

        public T3 Column3
        {
            get { return _column3; }
            set
            {
                AssertIsWriteable();
                _column3 = value;
            }
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
        private T4 _column4;

        public MetadataRow()
        {
        }

        public MetadataRow(MetadataToken metadataToken)
            : base(metadataToken)
        {
        }

        public T4 Column4
        {
            get { return _column4; }
            set
            {
                AssertIsWriteable();
                _column4 = value;
            }
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
        private T5 _column5;

        public MetadataRow()
        {
        }

        public MetadataRow(MetadataToken metadataToken)
            : base(metadataToken)
        {
        }

        public T5 Column5
        {
            get { return _column5; }
            set
            {
                AssertIsWriteable();
                _column5 = value;
            }
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
        private T6 _column6;

        public MetadataRow()
        {
        }

        public MetadataRow(MetadataToken metadataToken)
            : base(metadataToken)
        {
        }

        public T6 Column6
        {
            get { return _column6; }
            set
            {
                AssertIsWriteable();
                _column6 = value;
            }
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
        private T7 _column7;
        private T8 _column8;
        private T9 _column9;

        public MetadataRow()
        {
        }

        public MetadataRow(MetadataToken metadataToken)
            : base(metadataToken)
        {
        }

        public T7 Column7
        {
            get { return _column7; }
            set
            {
                AssertIsWriteable();
                _column7 = value;
            }
        }

        public T8 Column8
        {
            get { return _column8; }
            set
            {
                AssertIsWriteable();
                _column8 = value;
            }
        }

        public T9 Column9
        {
            get { return _column9; }
            set
            {
                AssertIsWriteable();
                _column9 = value;
            }
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
