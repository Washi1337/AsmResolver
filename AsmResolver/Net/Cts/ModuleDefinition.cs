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
        private readonly LazyValue<Collection<TypeDefinition>> _topLevelTypes;
        
        public ModuleDefinition(string name)
            : base(new MetadataToken(MetadataTokenType.Module))
        {
            _name = new LazyValue<string>(name);
            _mvid = new LazyValue<Guid>();
            _encId = new LazyValue<Guid>();
            _encBaseId = new LazyValue<Guid>();
            _topLevelTypes = new LazyValue<Collection<TypeDefinition>>(
                new DelegatedMemberCollection<ModuleDefinition, TypeDefinition>(this, GetTypeOwner, SetTypeOwner));
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal ModuleDefinition(MetadataImage image, MetadataRow<ushort, uint, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            Assembly = image.Assembly;
            var header = image.Header;
            var stringStream = header.GetStream<StringStream>();
            var guidStream = header.GetStream<GuidStream>();
            var tableStream = header.GetStream<TableStream>();

            Generation = row.Column1;
            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column2));
            _mvid = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column3));
            _encId = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column4));
            _encBaseId = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column5));
            CustomAttributes = new CustomAttributeCollection(this);
            _topLevelTypes = new LazyValue<Collection<TypeDefinition>>(() =>
                new ShallowTypeCollection(this,
                    (TypeDefinitionTable) tableStream.GetTable(MetadataTokenType.TypeDef),
                    image.GetTopLevelTypes()));
        }

        /// <inheritdoc />
        public override MetadataImage Image => Assembly?.Image;

        /// <summary>
        /// Gets or sets the generation of the module.
        /// </summary>
        public ushort Generation
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the module.
        /// </summary>
        public Guid Mvid
        {
            get => _mvid.Value;
            set => _mvid.Value = value;
        }

        /// <summary>
        /// Gets or sets the unique Edit-n-Continue identification number. 
        /// </summary>
        public Guid EncId
        {
            get => _encId.Value;
            set => _encId.Value = value;
        }

        /// <summary>
        /// Gets or sets the unique Edit-n-Continue base identification number. 
        /// </summary>
        public Guid EncBaseId
        {
            get => _encBaseId.Value;
            set => _encBaseId.Value = value;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <summary>
        /// Gets the containing assembly definition that defines the module.
        /// </summary>
        public AssemblyDefinition Assembly
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a collection of the top-level type definitions declared in this module; that is, all types that are not
        /// nested into another types.
        /// </summary>
        public Collection<TypeDefinition> TopLevelTypes => _topLevelTypes.Value;

        /// <summary>
        /// Gets a collection of all types declared in this module. This includes both top level types as well as
        /// nested types.
        /// </summary>
        /// <returns>A collection of types defined in the module.</returns>
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
