using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public sealed class ReadingContext
    {
        private WindowsAssembly _assembly;

        public IBinaryStreamReader Reader
        {
            get;
            set;
        }

        public WindowsAssembly Assembly
        {
            get { return _assembly; }
            internal set
            {
                if (_assembly != null)
                    throw new InvalidOperationException();
                _assembly = value;
            }
        }

        public ReadingParameters Parameters
        {
            get;
            set;
        }

        public ReadingContext CreateSubContext(long address)
        {
            return new ReadingContext()
            {
                Assembly = Assembly,
                Reader = Reader.CreateSubReader(address),
            };
        }

        public ReadingContext CreateSubContext(long address, int size)
        {
            return new ReadingContext()
            {
                Assembly = Assembly,
                Reader = Reader.CreateSubReader(address, size),
            };
        }
    }
}
