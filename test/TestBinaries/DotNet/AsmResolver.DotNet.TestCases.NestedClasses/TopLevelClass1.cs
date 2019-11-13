using System;

namespace AsmResolver.DotNet.TestCases.NestedClasses
{
    public class TopLevelClass1
    {
        public class Nested1
        {
            public class Nested1Nested1
            {
            }
            
            public class Nested1Nested2
            {
            }
        }
        
        public class Nested2
        {
            public class Nested2Nested1
            {
            }
            
            public class Nested2Nested2
            {
            }
        }
    }
}