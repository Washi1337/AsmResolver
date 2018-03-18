namespace AsmResolver
{
    /// <summary>
    /// Represents a symbol imported by a windows assembly image.
    /// </summary>
    public class ImageSymbolImport
    {
        internal static ImageSymbolImport FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var application = context.Assembly;

            var optionalHeader = application.NtHeaders.OptionalHeader;

            var import = new ImageSymbolImport(optionalHeader.Magic == OptionalHeaderMagic.Pe32Plus
                ? reader.ReadUInt64()
                : reader.ReadUInt32());

            if (import.Lookup == 0)
                return import;

            import.IsImportByOrdinal = import.Lookup >> (optionalHeader.Magic == OptionalHeaderMagic.Pe32Plus ? 63 : 31) == 1;
            
            if (!import.IsImportByOrdinal)
                import.HintName =
                    HintName.FromReadingContext(context.CreateSubContext(application.RvaToFileOffset(import.HintNameRva)));
    
            return import;
        }

        private ImageSymbolImport(ulong lookup)
        {
            Lookup = lookup;
        }

        public ImageSymbolImport(HintName hintName)
        {
            IsImportByOrdinal = false;
            HintName = hintName;
        }

        public ImageSymbolImport(ushort ordinal)
        {
            IsImportByOrdinal = true;
            Ordinal = ordinal;
        }

        /// <summary>
        /// Gets the raw value of the symbol import.
        /// </summary>
        public ulong Lookup
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the ordinal of the symbol import.
        /// </summary>
        public ushort Ordinal
        {
            get { return (ushort)(!IsImportByOrdinal ? 0 : (Lookup & 0xFFFF)); }
            set { Lookup = unchecked((ulong)(long.MinValue | value)); }
        }

        /// <summary>
        /// Gets the relative virtual address of the hint-name entry used by the symbol import.
        /// </summary>
        public uint HintNameRva
        {
            get { return (uint)(IsImportByOrdinal ? 0 : (Lookup & 0x7FFFFFFFF)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the symbol should be imported by ordinal instead of by hint-name.
        /// </summary>
        public bool IsImportByOrdinal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the hint-name pair of the symbol.
        /// </summary>
        public HintName HintName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the imported module definining the symbol.
        /// </summary>
        public ImageModuleImport Module
        {
            get;
            internal set;
        }

        /// <summary>
        /// Determines the import address to use for the symbol.
        /// </summary>
        /// <param name="is32Bit">Specifies whether the address should be a 32-bit or 64-bit address.</param>
        /// <returns></returns>
        public ulong GetTargetAddress(bool is32Bit)
        {
            if (Module == null)
                return 0;
            return Module.GetSymbolImportAddress(this, is32Bit);
        }

        public override string ToString()
        {
            var prefix = Module == null ? string.Empty : Module.Name + "!";
            if (IsImportByOrdinal)
                return prefix + '#' + Ordinal.ToString("X");
            return prefix + HintName.Name;
        }
    }
}
