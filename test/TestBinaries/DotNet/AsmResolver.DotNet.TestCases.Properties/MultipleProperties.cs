namespace AsmResolver.DotNet.TestCases.Properties
{
    public class MultipleProperties
    {
        public int ReadOnlyProperty
        {
            get;
        }

        public string WriteOnlyProperty
        {
            set {}
        }

        public MultipleProperties ReadWriteProperty
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