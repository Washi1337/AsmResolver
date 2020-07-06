using System;

namespace AsmResolver.DotNet.TestCases.Events
{
    public class DerivedClassWithCustomEvent : BaseClassWithEvent
    {
        public override event EventHandler VirtualEvent
        {
            add => base.VirtualEvent += value;
            remove => base.VirtualEvent -= value;
        }
    }
}