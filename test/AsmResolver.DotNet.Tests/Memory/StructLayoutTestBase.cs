using System;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet.Memory;
using Xunit;

namespace AsmResolver.DotNet.Tests.Memory
{
    public class StructLayoutTestBase : IClassFixture<CurrentModuleFixture>
    {
        public StructLayoutTestBase(CurrentModuleFixture fixture)
        {
            Module = fixture.Module;
        }

        protected ModuleDefinition Module
        {
            get;
        }

        protected TypeDefinition FindTestType(Type type)
        {
            return (TypeDefinition) Module.LookupMember(type.MetadataToken);
        }     
        
        protected void VerifySize<T>()
        {
            var type = FindTestType(typeof(T));
            var layout = type.GetImpliedMemoryLayout(IntPtr.Size == 4);
            Assert.Equal((uint) Unsafe.SizeOf<T>(), layout.Size);
        }

    }
}