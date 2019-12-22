using System;

namespace AsmResolver.DotNet.TestCases.Events
{
    public class MultipleEvents
    {
        public event EventHandler Event1;
        
        public event AssemblyLoadEventHandler Event2;
        
        public event ResolveEventHandler Event3;
    }
}