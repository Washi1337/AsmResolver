using System;

namespace AsmResolver.Emit
{
    public abstract class WindowsAssemblyBuilder : FileSegmentBuilder
    {
        protected WindowsAssemblyBuilder(WindowsAssembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            Assembly = assembly;
            
            Segments.Add(assembly.DosHeader);
            Segments.Add(assembly.NtHeaders);
            Segments.Add(SectionTable = new SectionTableBuffer(
                assembly.NtHeaders.OptionalHeader.FileAlignment,
                assembly.NtHeaders.OptionalHeader.SectionAlignment));
        }

        public WindowsAssembly Assembly
        {
            get;
            private set;
        }
        
        public SectionTableBuffer SectionTable
        {
            get;
            private set;
        }

        public void Build()
        {
            var context = new EmitContext(this);
            
            CreateSections(context);
            UpdateOffsets(context);
            UpdateReferences(context);
        }
        
        protected abstract void CreateSections(EmitContext context);

    }
}