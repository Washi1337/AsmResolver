namespace AsmResolver
{
    public class WritingContext
    {
        public WritingContext(WindowsAssembly assembly, IBinaryStreamWriter writer)
        {
            Assembly = assembly;
            Writer = writer;
        }

        public WindowsAssembly Assembly
        {
            get;
            private set;
        }

        // TODO
        //public BuildingContext BuildingContext
        //{
        //    get;
        //    private set;
        //}

        public IBinaryStreamWriter Writer
        {
            get;
            private set;
        }

    }
}
