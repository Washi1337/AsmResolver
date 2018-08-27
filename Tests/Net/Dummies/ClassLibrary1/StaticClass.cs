namespace ClassLibrary1
{
    public static class StaticClass
    {
        public static int SomeExtension(this int value)
        {
            return value + 3;
        }
    }
}