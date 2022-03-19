namespace AsmResolver.PE.DotNet.Bundles
{
    public class BundleFile
    {
        private readonly LazyVariable<ISegment> _contents;

        public BundleFile()
        {
            _contents = new LazyVariable<ISegment>(GetContents);
        }

        public string RelativePath
        {
            get;
            set;
        }

        public BundleFileType Type
        {
            get;
            set;
        }

        public bool IsCompressed
        {
            get;
            set;
        }

        public ISegment Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
        }

        protected virtual ISegment? GetContents() => null;
    }
}
