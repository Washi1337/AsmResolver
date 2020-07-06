using System;

// Disable warnings for unused members. 
#pragma warning disable 67

namespace AsmResolver.DotNet.TestCases.Events
{
    public class BaseClassWithEvent
    {
        public virtual event EventHandler VirtualEvent;
    }
}