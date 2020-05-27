using System;

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    public sealed class TemporaryModuleFixture
    {
        public ModuleDefinition Module
        {
            get;
        } = ModuleDefinition.FromFile(typeof(TemporaryModuleFixture).Assembly.Location);

        public TypeDefinition LookupType(Type type)
        {
            return (TypeDefinition) Module.LookupMember(type.MetadataToken);
        }
    }
}