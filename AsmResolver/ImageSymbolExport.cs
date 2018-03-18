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
            return string.Format("Rva: {0:X8}, NameOrdinal: {1:X4}, Name: {2}", 
                Rva,
                NameOrdinal,
                Name ?? "null");
        }
    }
}
