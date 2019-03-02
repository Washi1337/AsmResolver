using System;
using System.Linq;

namespace AsmResolver.Net.Metadata
{
    /// <summary>
    /// When derived from this class, represents a single row in a metadata table.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the metadata token associated to the row.
        /// </summary>
        public MetadataToken MetadataToken
        {
            get { return _metadataToken; }
            set
            {
                AssertIsWritable();
                _metadataToken = value;
            }
        }

        /// <summary>
        /// Gets a value indicating the metadata row is editable or not.
        /// </summary>
        public bool IsReadOnly
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a collection of all column values in the row.
        /// </summary>
        /// <returns></returns>
        public abstract object[] GetAllColumns();

        /// <summary>
        /// Verifies whether the metadata row is writable or not.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when the metadata row is in read-only mode.</exception>
        protected void AssertIsWritable()
        {
            if (IsReadOnly)
                throw new MetadataLockedException("edit metadata row");
        }

        protected bool Equals(MetadataRow other)
        {
            return GetAllColumns().SequenceEqual(other.GetAllColumns());
        }

        public override string ToString()
        {
            return $"({string.Join(", ", GetAllColumns())})";
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
                AssertIsWritable();
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
                AssertIsWritable();
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
                AssertIsWritable();
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
                AssertIsWritable();
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
                AssertIsWritable();
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
                AssertIsWritable();
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
                AssertIsWritable();
                _column7 = value;
            }
        }

        public T8 Column8
        {
            get { return _column8; }
            set
            {
                AssertIsWritable();
                _column8 = value;
            }
        }

        public T9 Column9
        {
            get { return _column9; }
            set
            {
                AssertIsWritable();
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
