using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class ImageSymbolExport
    {
        public uint Rva
        {
            get;
            set;
        }

        public bool IsForwarder
        {
            get;
            set;
        }

        public ushort NameOrdinal
        {
            get;
            set;
        }

        public uint NameRva
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string ForwarderName
        {
            get;
            set;
        }

    }
}
