namespace AsmResolver.DotNet.TestCases.Properties
{
    public class MultipleProperties
    {
        public int ReadOnlyProperty
        {
            get;
        }

        public int WriteOnlyProperty
        {
            set {}
        }

        public int ReadWriteProperty
        {
            get;
            set;
        }

        public int this[int index]
        {
            get => 0;
            set { }
        }
        
    }
}