namespace ClassLibrary1
{
    public class SimpleClassInternalReferences
    {
        private readonly string _defaultPrefix;
        
        public SimpleClassInternalReferences()
        {
            _defaultPrefix = "abc";
        }

        public string SomeMethod(int x)
        {
            return SomeMethod(_defaultPrefix, x);
        }
        
        public string SomeMethod(string prefix, int x)
        {
            return prefix + x;
        }
    }
}