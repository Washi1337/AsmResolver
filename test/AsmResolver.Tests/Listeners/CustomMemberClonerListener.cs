using AsmResolver.DotNet;
using AsmResolver.DotNet.Cloning;
using System;

namespace AsmResolver.Tests.Listeners
{
    public class CustomMemberClonerListener : MemberClonerListener
    {
        public override void OnClonedMethod(MethodDefinition original, MethodDefinition cloned) =>
            cloned.Name = $"Method_{original.Name}";
    }
}
