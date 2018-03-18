using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a single module in a .NET assembly.
    /// </summary>
    public class ModuleDefinition : MetadataMember<MetadataRow<ushort, uint, uint, uint, uint>>, IHasCustomAttribute, IResolutionScope
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<Guid> _mvid;
        private readonly LazyValue<Guid> _encId;
        private readonly LazyValue<Guid> _encBaseId;
        private MetadataImage _image;

        public ModuleDefinition(string name)
            : base(new MetadataToken(MetadataTokenType.Module))
        {
            _name = new LazyValue<string>(name);
            _mvid = new LazyValue<Guid>();
            _encId = new LazyValue<Guid>();
            _encBaseId = new LazyValue<Guid>();
            TopLevelTypes = new DelegatedMemberCollection<ModuleDefinition, TypeDefinition>(this, GetTypeOwner, SetTypeOwner);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal ModuleDefinition(MetadataImage image, MetadataRow<ushort, uint, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var header = image.Header;
            var stringStream = header.GetStream<StringStream>();
            var guidStream = header.GetStream<GuidStream>();

            Generation = row.Column1;
           _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column2));
           _mvid = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column3));
           _encId = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column4));
           _encBaseId = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column5));
            TopLevelTypes = new ShallowTypeCollection(this, header.GetStream<TableStream>().GetTable<TypeDefinitionTable>());
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return Assembly != null ? Assembly.Image : null; }
        }

        public ushort Generation
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public Guid Mvid
        {
            get { return _mvid.Value; }
            set { _mvid.Value = value; }
        }

        public Guid EncId
        {
            get { return _encId.Value; }
            set { _encId.Value = value; }
        }

        public Guid EncBaseId
        {
            get { return _encBaseId.Value; }
            set { _encBaseId.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        public AssemblyDefinition Assembly
        {
            get;
            internal set;
        }

        public Collection<TypeDefinition> TopLevelTypes
        {
            get;
            private set;
        }

        public IEnumerable<TypeDefinition> GetAllTypes()
        {
            var stack = new Stack<TypeDefinition>();

            foreach (var type in TopLevelTypes.Reverse())
                stack.Push(type);
                   
            while (stack.Count > 0)
            {
                var type = stack.Pop();
                yield return type;
                foreach (var nestedClass in type.NestedClasses.Reverse())
                    stack.Push(nestedClass.Class);
            }
        }

        private static ModuleDefinition GetTypeOwner(TypeDefinition type)
        {
            return type.Module;
        }

        private static void SetTypeOwner(TypeDefinition type, ModuleDefinition module)
        {
            type.Module = module;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
