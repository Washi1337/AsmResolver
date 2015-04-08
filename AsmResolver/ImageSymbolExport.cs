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

        public ushort? NameOrdinal
        {
            get;
            set;
        }

        public uint? NameRva
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

        public override string ToString()
        {
            return string.Format("Rva: {0:X8}, NameOrdinal: {1}, Name: {2}", 
                Rva,
                NameOrdinal.HasValue ? NameOrdinal.Value.ToString("X") : "null",
                Name ?? "null");
        }
    }
}
